using Coven.Command;
using Coven.Widgets;

using Spectre.Console.Rendering;

namespace Coven;

public class Application : IRenderable, IDisposable, IAsyncDisposable
{
    public const int BYTES_PER_ROW = 16;
    public const int ROWS = 16;
    public const int BUFFER_SIZE = BYTES_PER_ROW * ROWS;
    public const int MAX_CURSOR_VALUE = BUFFER_SIZE - 1;

    public int Cursor { get; private set; }
    public int Page { get; private set; }

    public byte[] CurrentBuffer { get => _currentPageBuffer; }
    public long ContentLength { get => _contentStream.Length; }
    public bool BufferIsDirty { get => _changesCursor != _savedChangesCursor; }
    public string OriginalFilePath { get => _originalFilePath; }
    public bool HasOriginalFile { get => !string.IsNullOrEmpty(_originalFilePath); }
    public int LastPage
    {
        get => (new int[] { (int)(ContentLength / BUFFER_SIZE) - 1, 0 }.Max());
    }
    public int AbsoluteCursor { get => Page * BUFFER_SIZE + Cursor; }

    private const char HELP_KEY = '?';

    private const string EXIT_KEY = "Q";

    private IMode _mode;
    private ApplicationWidget _view;

    private Stream _contentStream;
    private Stream _originalStream;
    private string _originalFilePath = string.Empty;
    private byte[] _currentPageBuffer;
    private byte[]? _modifiedContentBuffer = null;
    private bool _hasContentBufferBeenModified { get => _modifiedContentBuffer is not null; }

    private List<ICommand> _changesStack = new();
    private int _changesCursor = 0;
    private int _savedChangesCursor = 0;

    /// <summary>
    /// Creates a new application by opening the file stream and loading the initial buffer asynchronously.
    /// </summary>
    /// <param name="path">The file's path</param>
    public static async Task<Application> CreateAsync(string path)
    {
        var buffer = new byte[BUFFER_SIZE];

        var f = File.Open(path, FileMode.Open);
        await f.ReadAsync(buffer, 0, BUFFER_SIZE);

        return new Application(f, buffer, path);
    }

    /// <summary>
    /// Loads the file and loads the initial buffer synchronously.
    /// </summary>
    /// <param name="path">The file's path</param>
    public Application(string path) : this(File.Open(path, FileMode.Open), path)
    {
    }

    /// <summary>
    /// Uses the stream to load content and loads the initial buffer synchronously.
    /// </summary>
    /// <param name="content">A stream to read the content</param>
    /// <param name="path">The path of the file to be taken as origin</param>
    public Application(Stream content, string path) : this(content, createBuffer(content), path)
    {
    }

    // /// <summary>
    // /// Uses the stream to load content.
    // /// </summary>
    // /// <param name="content">A stream to read the content</param>
    // /// <param name="buffer">The initial buffer to show</param>
    // public Application(Stream content, byte[] buffer) : this(content, buffer, string.Empty)
    // {
    // }

    /// <summary>
    /// Uses the stream to load content.
    /// </summary>
    /// <param name="content">A stream to read the content</param>
    /// <param name="buffer">The initial buffer to show</param>
    /// <param name="path">The path of the file to be taken as origin</param>
    public Application(Stream content, byte[] buffer, string path)
    {
        _contentStream = content;
        _originalStream = content;
        _currentPageBuffer = buffer;
        _originalFilePath = path;

        _view = new ApplicationWidget();

        _mode = new NavigationMode(this);
        _mode.BuildView(_view);
    }

    public async Task<bool> CaptureInput()
    {
        var keyInfo = System.Console.ReadKey(intercept: true);

        if (keyInfo.Key.ToString() == EXIT_KEY)
        {
            if (!BufferIsDirty)
            {
                return false;
            }

            _view.SetStatus("The current buffer was modified. To exit without saving press Ctrl + C.");
            return true;
        }

        var mode = keyInfo.KeyChar == HELP_KEY && _mode.GetType() != typeof(HelpMode)
            ? new HelpMode(this, _mode)
            : await _mode.HandleInputAsync(keyInfo, _view);

        if (mode.GetHashCode() != _mode.GetHashCode())
        {
            mode.BuildView(_view);
        }

        _mode = mode;

        return true;
    }

    public async Task Commit(ICommand command)
    {
        if (_changesStack.Count > _changesCursor)
        {
            _changesStack[_changesCursor] = command;
        }
        else
        {
            _changesStack.Add(command);
        }
        _changesCursor++;

        await command.Do(this);

        return;
    }

    public async Task Redo()
    {
        if (_changesStack.Count > _changesCursor)
        {
            await _changesStack[_changesCursor].Do(this);
            _changesCursor++;

            await loadPage(Page);
            _view.SetStatus("Change redone!");
            _view.ReloadBytes();
        }
        else
        {
            _view.SetStatus("Last change reached");
        }

        return;
    }

    public async Task Undo()
    {
        if (_changesCursor > 0)
        {
            _changesCursor--;
            await _changesStack[_changesCursor].Undo(this);

            await loadPage(Page);
            _view.SetStatus("Change undone!");
            _view.ReloadBytes();
        }
        else
        {
            _view.SetStatus("Initial state reached");
        }

        return;
    }

    public Task AdvanceOne()
    {
        if (Cursor < MAX_CURSOR_VALUE)
        {
            Cursor++;
        }
        else if (!inLastPage())
        {
            Cursor = 0;
            Page++;
        }
        return Task.CompletedTask;
    }

    public Task RetreatOne()
    {
        if (Cursor > 0)
        {
            Cursor--;
        }
        else if (!inFirstPage())
        {
            Cursor = MAX_CURSOR_VALUE;
            Page--;
        }
        return Task.CompletedTask;
    }

    public Task AdvanceOneRow()
    {
        if (MAX_CURSOR_VALUE - Cursor > BYTES_PER_ROW)
        {
            Cursor += BYTES_PER_ROW;
            return Task.CompletedTask;
        }
        if (!inLastPage())
        {
            Cursor = (Cursor + BYTES_PER_ROW) % BUFFER_SIZE;
            return AdvanceOnePage();
        }

        return Task.CompletedTask;
    }

    public Task RetreatOneRow()
    {
        if (Cursor >= BYTES_PER_ROW)
        {
            Cursor -= BYTES_PER_ROW;
            return Task.CompletedTask;
        }
        if (!inFirstPage())
        {
            Cursor = BUFFER_SIZE + Cursor - BYTES_PER_ROW;
            return RetreatOnePage();
        }

        return Task.CompletedTask;
    }

    public Task AdvanceOnePage()
    {
        if (!inLastPage())
        {
            Page++;
            return loadPage(Page);
        }

        return Task.CompletedTask;
    }

    public Task RetreatOnePage()
    {
        if (!inFirstPage())
        {
            Page--;
            return loadPage(Page);
        }
        return Task.CompletedTask;
    }

    public Task GoToFirstPage()
    {
        if (!inFirstPage())
        {
            Page = 0;
            return loadPage(Page);
        }
        return Task.CompletedTask;
    }

    public Task GoToLastPage()
    {
        if (!inLastPage())
        {
            Page = LastPage;
            return loadPage(Page);
        }
        return Task.CompletedTask;
    }

    public async Task EditByte(int page, int position, byte value)
    {
        if (page == Page)
        {
            _currentPageBuffer[position] = value;
        }

        await createModifiedBufferIfNeeded();

        _modifiedContentBuffer[page * BUFFER_SIZE + position] = value;
    }

    public async Task InsertEmptyBytes(int page, int position, int numberBytes)
    {
        if (numberBytes <= 0)
        {
            return;
        }

        await createModifiedBufferIfNeeded();

        var insertPosition = page * BUFFER_SIZE + position;
        var restPosition = insertPosition + numberBytes;
        var newModifiedBuffer = new byte[_modifiedContentBuffer.Length + numberBytes];
        for (int i = _modifiedContentBuffer.Length + numberBytes - 1; i >= restPosition; i--)
        {
            newModifiedBuffer[i] = _modifiedContentBuffer[i - numberBytes];
        }

        for (int i = 0; i < insertPosition; i++)
        {
            newModifiedBuffer[i] = _modifiedContentBuffer[i];
        }

        setModifiedBuffer(newModifiedBuffer);

        if (Page == page)
        {
            await loadPage(Page);
        }
    }

    public async Task DeleteBytes(int page, int position, int numberBytes)
    {
        if (numberBytes <= 0)
        {
            return;
        }

        await createModifiedBufferIfNeeded();

        var absolutePosition = page * BUFFER_SIZE + position;

        var newLength = _modifiedContentBuffer.Length - numberBytes;
        var newModifiedBuffer = new byte[newLength];
        for (int i = 0; i < absolutePosition; i++)
        {
            newModifiedBuffer[i] = _modifiedContentBuffer[i];
        }
        for (int i = absolutePosition; i < newLength; i++)
        {
            newModifiedBuffer[i] = _modifiedContentBuffer[i + numberBytes];
        }

        setModifiedBuffer(newModifiedBuffer);

        if (Page == page)
        {
            await loadPage(Page);
        }
    }

    public async Task<SaveResult> WriteChanges(string path)
    {
        var dir = Path.GetDirectoryName(path);

        try
        {
            Directory.CreateDirectory(dir);
        }
        catch (IOException ex)
        {
            return SaveResult.Failure($"Error trying to ensure directory exists: {ex.Message}");
        }

        var tmpFilePath = Path.Combine(dir, "." + Path.GetFileNameWithoutExtension(path) + ".dirty");

        try
        {
            using (var tmpFile = File.Open(tmpFilePath, FileMode.Create, FileAccess.Write))
            {
                _contentStream.Seek(0, SeekOrigin.Begin);
                await _contentStream.CopyToAsync(tmpFile);
            }

            await _originalStream.DisposeAsync();
            File.Move(tmpFilePath, path, overwrite: true);
        }
        catch (IOException ex)
        {
            if (File.Exists(tmpFilePath))
            {
                File.Delete(tmpFilePath);
            }

            return SaveResult.Failure($"Error trying to save file: {ex.Message}");
        }

        _originalStream = File.Open(path, FileMode.Open);
        _originalFilePath = path;

        resetBuffer();

        return SaveResult.Success($"File saved {path}");
    }

    public Measurement Measure(RenderContext context, int maxWidth)
    {
        return ((IRenderable)_view).Measure(context, maxWidth);
    }

    public IEnumerable<Segment> Render(RenderContext context, int maxWidth)
    {
        return ((IRenderable)_view).Render(context, maxWidth);
    }

    public void Dispose()
    {
        _contentStream.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        return _contentStream.DisposeAsync();
    }

    private bool inFirstPage() => Page == 0;

    private bool inLastPage() => Page >= LastPage;

    private static byte[] createBuffer(Stream s)
    {
        var buffer = new byte[BUFFER_SIZE];

        s.Seek(0, SeekOrigin.Begin);
        s.Read(buffer, 0, BUFFER_SIZE);

        return buffer;
    }

    private async Task createModifiedBufferIfNeeded()
    {
        if (_hasContentBufferBeenModified)
        {
            return;
        }

        var buffer = new byte[_originalStream.Length];

        _originalStream.Seek(0, SeekOrigin.Begin);
        await _originalStream.ReadAsync(buffer);

        setModifiedBuffer(buffer);
    }

    private void resetBuffer()
    {
        _modifiedContentBuffer = null;
        _contentStream = _originalStream;
    }

    private void setModifiedBuffer(byte[] buffer)
    {
        setModifiedBuffer(buffer, buffer.Length);
    }

    private void setModifiedBuffer(byte[] buffer, int length)
    {
        _modifiedContentBuffer = buffer;
        _contentStream = new MemoryStream(_modifiedContentBuffer, 0, length);
    }

    private async Task loadPage(int page)
    {
        _currentPageBuffer = new byte[BUFFER_SIZE];
        _contentStream.Seek(page * BUFFER_SIZE, SeekOrigin.Begin);
        await _contentStream.ReadAsync(_currentPageBuffer, 0, BUFFER_SIZE);
    }

    public record SaveResult(string Message, bool WasSuccessful)
    {
        public static SaveResult Success(string message)
        {
            return new SaveResult(message, true);
        }

        public static SaveResult Failure(string message)
        {
            return new SaveResult(message, false);
        }
    }
}
