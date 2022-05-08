
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Coven.Widgets;

public class ApplicationWidget : JustInTimeRenderable, IApplicationController
{
    private IView _content;
    private string _caption;
    private string _status;

    public ApplicationWidget()
    {
        _content = new InformationPanel().SetContent(new Text("Loading..."));
        _caption = string.Empty;
        _status = string.Empty;
    }

    public void UpdateByteCell(int cursor, IRenderable content)
    {
        _content.UpdateByteCell(cursor, content);
    }

    public void ReloadBytes()
    {
        _content.ReloadBytes();
    }

    public IApplicationController Set(IView view)
    {
        _content = view;
        MarkAsDirty();

        return this;
    }

    public IApplicationController Set(IView view, string caption, string status)
    {
        _content = view;
        _caption = caption;
        _status = status;
        MarkAsDirty();

        return this;
    }

    public IApplicationController Set(string caption, string status)
    {
        if (_caption != caption || _status != status)
        {
            _caption = caption;
            _status = status;
            MarkAsDirty();
        }

        return this;
    }

    public IApplicationController SetCaption(string text)
    {
        return Set(text, _status);
    }

    public IApplicationController SetStatus(string text)
    {
        return Set(_caption, text);
    }

    protected override IRenderable Build()
    {
        var rows = new IRenderable[] { _content, Text.Empty, Text.Empty };

        if (_caption != string.Empty)
        {
            rows[1] = new Text(
                _caption.PadRight(CanonicalBytesRepresentationTable.TABLE_WIDTH),
                new Style(decoration: Decoration.Invert))
            ;
        }

        if (_status != string.Empty)
        {
            rows[2] = new Text($"{_status}");
        }

        return new Rows(rows).Expand();
    }
}