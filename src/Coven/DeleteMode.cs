using System.Collections.Immutable;

using Coven.Command;
using Coven.Widgets;

using Spectre.Console;
using Spectre.Console.Rendering;

namespace Coven;

public class DeleteMode : IMode
{
    private const string INCREASE_RANGE_KEY = "RightArrow";
    private const string INCREASE_RANGE_ALT_KEY = "L";
    private const string DECREASE_RANGE_KEY = "LeftArrow";
    private const string DECREASE_RANGE_ALT_KEY = "H";
    private const string ACCEPT_KEY = "Enter";
    private const string EXIT_MODE_KEY = "Escape";

    private static Style _editStyle = new Style(Color.White, decoration: Decoration.Underline);

    private Application _app;
    private Style _style;
    private IRenderable _selectedCell;

    private int _numberOfBytes;

    public DeleteMode(Application app)
    {
        _app = app;
        _numberOfBytes = 1;

        _style = Style.Parse("underline slowblink");
        _selectedCell = new Text("xx", _style);
    }

    public void BuildView(IApplicationController controller)
    {
        controller.Set(
            new CanonicalBytesRepresentationTable(_app),
            caption: "Delete mode: [?] Help [Enter] Accept [Esc] Cancel [q] Exit program",
            status: ""
            );

        selectedStartingPosition(controller);
    }

    public async Task<IMode> HandleInputAsync(ConsoleKeyInfo keyInfo, IApplicationController controller)
    {
        switch (keyInfo.Key.ToString())
        {
            case EXIT_MODE_KEY:
                return new NavigationMode(_app);

            case ACCEPT_KEY:
                await _app.DeleteBytes(_app.Page, _app.Cursor, _numberOfBytes);
                await _app.Commit(
                    new Delete
                    {
                        Page = _app.Page,
                        Position = _app.Cursor,
                        BytesDeleted = _app.CurrentBuffer
                            .TakeWhile((_, i) => i >= _app.Cursor && i < _app.Cursor + _numberOfBytes)
                            .ToImmutableArray(),
                    }
                );

                return new NavigationMode(_app);

            case INCREASE_RANGE_KEY:
            case INCREASE_RANGE_ALT_KEY:
                increaseRange(controller);
                break;

            case DECREASE_RANGE_KEY:
            case DECREASE_RANGE_ALT_KEY:
                decreaseRange(controller);
                break;

            default:
                break;
        }


        return this;
    }

    private void selectedStartingPosition(IApplicationController controller)
    {
        controller.UpdateByteCell(_app.Cursor, new Text("XX", _style));
    }

    private void increaseRange(IApplicationController controller)
    {
        if (_numberOfBytes == Application.BUFFER_SIZE - _app.Cursor)
        {
            return;
        }

        controller.UpdateByteCell(_app.Cursor + _numberOfBytes, _selectedCell);
        _numberOfBytes++;
    }

    private void decreaseRange(IApplicationController controller)
    {
        if (_numberOfBytes == 1)
        {
            return;
        }

        var position = _app.Cursor + _numberOfBytes - 1;
        controller.UpdateByteCell(position, new Text(_app.CurrentBuffer[position].ToString("X2")));
        _numberOfBytes--;
    }
}