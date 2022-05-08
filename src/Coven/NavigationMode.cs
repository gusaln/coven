using Coven.Widgets;

using Spectre.Console;

namespace Coven;

public class NavigationMode : IMode
{
    private const string ADVANCE_ONE_KEY = "RightArrow";
    private const string RETREAT_ONE_KEY = "LeftArrow";
    private const string ADVANCE_ONE_ROW_KEY = "DownArrow";
    private const string RETREAT_ONE_ROW_KEY = "UpArrow";
    private const string ADVANCE_ONE_ALT_KEY = "L";
    private const string RETREAT_ONE_ALT_KEY = "H";
    private const string ADVANCE_ONE_ROW_ALT_KEY = "J";
    private const string RETREAT_ONE_ROW_ALT_KEY = "K";
    private const string ADVANCE_ONE_PAGE_KEY = "N";
    private const string RETREAT_ONE_PAGE_KEY = "P";
    private const string GO_TO_FIRST_PAGE_KEY = "Home";
    private const string GO_TO_LAST_PAGE_KEY = "End";
    private const string CHANGE_TO_EDIT_MODE_KEY = "E";
    private const string CHANGE_TO_INSERT_AT_MODE_KEY = "I";
    private const string CHANGE_TO_INSERT_AFTER_MODE_KEY = "A";
    private const string CHANGE_TO_DELETE_MODE_KEY = "X";
    private const string CHANGE_TO_SELECT_FILE_MODE_KEY = "W";
    private const string UNDO_KEY = "U";
    private const string REDO_KEY = "R";

    private static Style _selectedStyle = new Style(Color.Black, Color.White);
    private static Style _normalStyle = new Style(Color.White);

    private Application _app;

    public NavigationMode(Application app)
    {
        _app = app;
    }

    public void BuildView(IApplicationController controller)
    {
        controller.Set(
            new CanonicalBytesRepresentationTable(_app),
            caption: "Navigation mode: [?] Help [q] Exit program",
            status: ""
            );

        selectPosition(_app.Cursor, controller);
    }

    public async Task<IMode> HandleInputAsync(ConsoleKeyInfo keyInfo, IApplicationController controller)
    {
        var oldCursor = _app.Cursor;
        var oldPage = _app.Page;
        switch (keyInfo.Key.ToString())
        {
            case GO_TO_FIRST_PAGE_KEY:
                await _app.GoToFirstPage();
                break;

            case GO_TO_LAST_PAGE_KEY:
                await _app.GoToLastPage();
                break;

            case ADVANCE_ONE_KEY:
            case ADVANCE_ONE_ALT_KEY:
                await _app.AdvanceOne();
                break;

            case ADVANCE_ONE_ROW_KEY:
            case ADVANCE_ONE_ROW_ALT_KEY:
                await _app.AdvanceOneRow();
                break;

            case ADVANCE_ONE_PAGE_KEY:
                await _app.AdvanceOnePage();
                break;

            case RETREAT_ONE_KEY:
            case RETREAT_ONE_ALT_KEY:
                await _app.RetreatOne();
                break;

            case RETREAT_ONE_ROW_KEY:
            case RETREAT_ONE_ROW_ALT_KEY:
                await _app.RetreatOneRow();
                break;

            case RETREAT_ONE_PAGE_KEY:
                await _app.RetreatOnePage();
                break;

            case UNDO_KEY:
                await _app.Undo();
                break;

            case REDO_KEY:
                await _app.Redo();
                break;

            case CHANGE_TO_EDIT_MODE_KEY:
                return new EditMode(_app);

            case CHANGE_TO_INSERT_AT_MODE_KEY:
                return new InsertBytesMode(_app, afterCursor: false);

            case CHANGE_TO_INSERT_AFTER_MODE_KEY:
                return new InsertBytesMode(_app, afterCursor: true);

            case CHANGE_TO_DELETE_MODE_KEY:
                return new DeleteMode(_app);

            case CHANGE_TO_SELECT_FILE_MODE_KEY:
                return new SelectFileMode(_app);

            default:
                break;
        }

        if (_app.Page != oldPage)
        {
            controller.ReloadBytes();
            selectPosition(_app.Cursor, controller);
        }
        else if (_app.Cursor != oldCursor)
        {
            unselectPosition(oldCursor, controller);
            selectPosition(_app.Cursor, controller);
        }

        return this;
    }

    private void selectPosition(int cursor, IApplicationController controller)
    {
        controller.UpdateByteCell(cursor, new Text(_app.CurrentBuffer[cursor].ToString("X2"), _selectedStyle));
    }

    private void unselectPosition(int cursor, IApplicationController controller)
    {
        controller.UpdateByteCell(cursor, new Text(_app.CurrentBuffer[cursor].ToString("X2"), _normalStyle));
    }
}