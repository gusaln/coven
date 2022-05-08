using Coven.Widgets;

using Spectre.Console;
using Spectre.Console.Rendering;

namespace Coven;

public class HelpMode : IMode
{
    private Application _app;
    private IMode _next;

    public HelpMode(Application app, IMode next)
    {
        _app = app;
        _next = next;
    }

    public void BuildView(IApplicationController controller)
    {
        var sections = new List<IRenderable>();

        sections.Add(new Text("# General"));
        sections.Add(buildTable(new()
        {
            ["?"] = "Help",
            ["q"] = "Quit program",
            ["Enter"] = "Accept change",
            ["Esc"] = "Cancel / Go back to Navigation",
        }));

        sections.Add(Text.Empty);
        sections.Add(new Text("# Navigation"));
        sections.Add(buildTable(new()
        {
            ["h,j,k,l"] = "Move cursor",
            ["arrows"] = "Move cursor (alt)",
            ["n,p"] = "Next / Prev. page",
            ["Home"] = "First page",
            ["End"] = "Last page",
            ["w"] = "Write changes",
            ["u,r"] = "Undo / Redo",
        }));

        sections.Add(Text.Empty);
        sections.Add(new Text("# Edit"));
        sections.Add(buildTable(new()
        {
            ["arrows"] = "Select nibble",
            ["hex char"] = "Edit current nibble",
        }));

        sections.Add(Text.Empty);
        sections.Add(new Text("# Insert"));
        sections.Add(buildTable(new()
        {
            ["0-9, Backspace"] = "Input number of bytes to insert",
        }));

        sections.Add(Text.Empty);
        sections.Add(new Text("# Delete"));
        sections.Add(buildTable(new()
        {
            ["arrows"] = "Increase / Decrease selection of bytes to deleted",
        }));

        controller.Set(
            new InformationPanel().SetContent(new Rows(sections).Expand()),
            caption: "Press any key to dismiss",
            status: ""
            );
    }

    public Task<IMode> HandleInputAsync(ConsoleKeyInfo keyInfo, IApplicationController controller)
    {
        return Task.FromResult(_next);
    }

    private Table buildTable(Dictionary<string, string> keys)
    {
        var table = new Table().HideHeaders().NoBorder().Expand().AddColumns("", "");

        foreach (var key in keys)
        {
            table.AddRow(new string[] { key.Key, key.Value });
        }

        return table;
    }
}