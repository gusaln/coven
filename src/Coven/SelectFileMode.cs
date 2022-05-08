using Coven.Widgets;

using Spectre.Console;

namespace Coven;

public class SelectFileMode : IMode
{
    private const string ACCEPT_INSERT_KEY = "Enter";
    private const string EXIT_MODE_KEY = "Escape";

    private Application _app;
    private string _path;

    public SelectFileMode(Application app)
    {
        _app = app;
        _path = getOriginalPath();
    }

    public void BuildView(IApplicationController controller)
    {
        controller.Set(
            new CanonicalBytesRepresentationTable(_app),
            caption: "[?] Help [Enter] Write file [Esc] Cancel write [q] Exit program",
            status: getStatus()
            );
    }

    public async Task<IMode> HandleInputAsync(ConsoleKeyInfo keyInfo, IApplicationController controller)
    {
        switch (keyInfo.Key.ToString())
        {
            case EXIT_MODE_KEY:
                return new NavigationMode(_app);

            case ACCEPT_INSERT_KEY:
                var result = await _app.WriteChanges(_path);
                if (!result.WasSuccessful)
                {
                    return new InformationBoxMode(_app, result.Message, this);
                }

                return new InformationBoxMode(_app, _path, new NavigationMode(_app));

            case "Backspace":
                _path = string.Join("", _path.SkipLast(1));
                controller.SetStatus(getStatus());
                break;

            default:
                if (char.IsLetterOrDigit(keyInfo.KeyChar)
                    || char.IsPunctuation(keyInfo.KeyChar)
                    || char.IsSeparator(keyInfo.KeyChar))
                {
                    _path = _path + keyInfo.KeyChar;
                }
                controller.SetStatus(getStatus());
                break;
        }

        return this;
    }

    private string getStatus()
    {
        return $"Input the file's path: {_path}";

        // _view.SetContent(new Markup($"Input the file's path: {_path}[invert slowblink] [/]"));
    }

    // private void setPath(string path)
    // {
    //     _path = path;
    //     _view.SetContent(new Markup($"Input the file's path: {_path}[invert slowblink] [/]"));
    // }

    private string getOriginalPath()
    {
        if (_app.HasOriginalFile)
        {
            return _app.OriginalFilePath;
        }

        return Directory.GetCurrentDirectory();
    }
}