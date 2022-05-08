using System.Globalization;

using Coven.Command;
using Coven.Widgets;

using Spectre.Console;

namespace Coven;

public class EditMode : IMode
{
    private const string MOVE_NIBBLE_RIGHT_KEY = "RightArrow";
    private const string MOVE_NIBBLE_LEFT_KEY = "LeftArrow";
    private const string ACCEPT_INSERT_KEY = "Enter";
    private const string EXIT_MODE_KEY = "Escape";

    private static Style _editStyle = new Style(Color.White, decoration: Decoration.Underline);

    private Application _app;

    private bool _mostSignificant = true;
    private int _newValue;

    public EditMode(Application app)
    {
        _app = app;
        _newValue = app.CurrentBuffer[app.Cursor];
    }

    public void BuildView(IApplicationController controller)
    {
        controller.Set(
            new CanonicalBytesRepresentationTable(_app),
            caption: "Edit mode: [?] Help [Enter] Accept [Esc] Cancel [q] Exit program",
            status: ""
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
                await _app.Commit(
                    new Edit
                    {
                        Page = _app.Page,
                        Position = _app.Cursor,
                        NewByte = (byte)_newValue,
                        OriginalByte = _app.CurrentBuffer[_app.Cursor],
                    }
                );

                return new NavigationMode(_app);

            case MOVE_NIBBLE_RIGHT_KEY:
            case MOVE_NIBBLE_LEFT_KEY:
                _mostSignificant = !_mostSignificant;
                updateSelectedByte(controller);
                break;

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
            case "A":
            case "B":
            case "C":
            case "D":
            case "E":
            case "F":
                var s = keyInfo.Key.ToString();
                var value = int.Parse((s.Length == 1 ? s[0] : s[1]).ToString(), NumberStyles.HexNumber);
                if (_mostSignificant)
                {
                    _newValue = (_newValue & 0x0f) | value << 4;
                }
                else
                {
                    _newValue = (_newValue & 0xf0) | value << 0;
                }
                _mostSignificant = !_mostSignificant;

                updateSelectedByte(controller);

                break;

            default:
                break;
        }

        return this;
    }

    private void updateSelectedByte(IApplicationController controller)
    {
        var originalHex = _app.CurrentBuffer[_app.Cursor].ToString("X2");
        var hex = _newValue.ToString("X2");

        if (_mostSignificant)
        { controller.UpdateByteCell(_app.Cursor, new Markup($"[underline slowblink]{hex[0]}[/]{hex[1]}")); }
        else
        { controller.UpdateByteCell(_app.Cursor, new Markup($"{hex[0]}[underline slowblink]{hex[1]}[/]")); }
    }
}