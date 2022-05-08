using Coven.Command;
using Coven.Widgets;

using Spectre.Console;

namespace Coven;

public class InsertBytesMode : IMode
{
    private const string ACCEPT_INSERT_KEY = "Enter";
    private const string EXIT_MODE_KEY = "Escape";

    private Application _app;

    private int _numberOfBytes;
    private bool _afterCursor;

    public InsertBytesMode(Application app, bool afterCursor)
    {
        _app = app;
        _numberOfBytes = 0;

        _afterCursor = afterCursor;
    }

    public void BuildView(IApplicationController controller)
    {
        controller.Set(
            new CanonicalBytesRepresentationTable(_app),
            caption: "Insert mode: [?] Help [Enter] Accept [Esc] Cancel [q] Exit program",
            status: getStatus()
            );

        updateSelectedByte(controller);
    }

    public async Task<IMode> HandleInputAsync(ConsoleKeyInfo keyInfo, IApplicationController controller)
    {
        switch (keyInfo.Key.ToString())
        {
            case EXIT_MODE_KEY:
                return new NavigationMode(_app);

            case ACCEPT_INSERT_KEY:
                var pos = _afterCursor ? _app.Cursor + 1 : _app.Cursor;
                await _app.Commit(
                    new Insert
                    {
                        Page = _app.Page,
                        Position = pos,
                        NumberOfBytes = _numberOfBytes,
                    }
                );

                return new NavigationMode(_app);

            case "D0":
            case "D1":
            case "D2":
            case "D3":
            case "D4":
            case "D5":
            case "D6":
            case "D7":
            case "D8":
            case "D9":
                _numberOfBytes = _numberOfBytes * 10 + int.Parse(keyInfo.KeyChar.ToString());
                controller.SetStatus(getStatus());

                break;

            case "Backspace":
                _numberOfBytes = _numberOfBytes / 10;
                controller.SetStatus(getStatus());

                break;

            default:
                break;
        }

        return this;
    }

    public string getStatus()
    {
        return $"How many bytes do you wish to insert?: {_numberOfBytes}";
    }

    private void updateSelectedByte(IApplicationController controller)
    {
        var marker = _afterCursor ? "I>" : "<I";

        controller.UpdateByteCell(
            _app.Cursor,
            new Text(marker, Style.Parse("underline slowblink"))
        );
    }
}