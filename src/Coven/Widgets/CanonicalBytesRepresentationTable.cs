
using System.Data;

using Spectre.Console;
using Spectre.Console.Rendering;

namespace Coven.Widgets;

public class CanonicalBytesRepresentationTable : IView
{
    public const byte LINE_INDEX_COLUMN_WIDTH = 11;
    public const byte BYTE_COLUMN_WIDTH = 3;
    public const byte ASCII_COLUMN_WIDTH = 2 + Application.BYTES_PER_ROW;
    public const byte TABLE_WIDTH = (
        LINE_INDEX_COLUMN_WIDTH
        + (BYTE_COLUMN_WIDTH * Application.BYTES_PER_ROW)
        + ASCII_COLUMN_WIDTH
    );

    private Application _app;

    private Style _accentStyle;
    private Table _bytesTable;

    public CanonicalBytesRepresentationTable(Application app)
    {
        _app = app;
        _bytesTable = generateTable();
        _accentStyle = Style.Parse("red");
    }

    public void UpdateByteCell(int cursor, IRenderable content)
    {
        _bytesTable.UpdateCell(
            cursor / Application.BYTES_PER_ROW,
            1 + cursor % Application.BYTES_PER_ROW,
            content
            );
    }

    public void ReloadBytes()
    {
        _bytesTable = generateTable();
    }

    public Measurement Measure(RenderContext context, int maxWidth)
    {
        return ((IRenderable)_bytesTable).Measure(context, maxWidth);
    }

    public IEnumerable<Segment> Render(RenderContext context, int maxWidth)
    {
        return ((IRenderable)_bytesTable).Render(context, maxWidth);
    }

    private Table generateTable()
    {
        var table = new Table()
            .NoBorder()
            .HideHeaders()
            .Expand()
            .Width(TABLE_WIDTH);

        table.AddColumn(new TableColumn("line index").Width(LINE_INDEX_COLUMN_WIDTH).Padding(0, 0, 0, 0));
        for (int i = 0; i < Application.BYTES_PER_ROW; i++)
        {
            table.AddColumn(new TableColumn("byte").Width(BYTE_COLUMN_WIDTH).Padding(0, 0, 0, 0));
        }
        table.AddColumn(new TableColumn("ascii").Width(ASCII_COLUMN_WIDTH).Padding(0, 0, 0, 0));

        foreach (var (bytes, lineIndex) in _app.CurrentBuffer
            .Chunk(Application.BYTES_PER_ROW)
            .Select((bytes, lineIndex) => (bytes, _app.Page * Application.ROWS + lineIndex)))
        {
            var columns = (new[] { new Text($"{lineIndex.ToString("X8")}:", Style.Parse("red")) })
                .Concat(
                    BitConverter.ToString(bytes).Split('-').Select(b => new Text(b))
                )
                .Append(
                    new Text("|"
                    + string.Join("", bytes.Select(b => b switch
                        {
                            // ASCII values without visual representation
                            < 32 => ' ',
                            // ASCII values without visual representation
                            >= 127 => ' ',
                            _ => (char)b,
                        }))
                    + "|",
                    Style.Parse("red"))
                );

            table.AddRow(columns);
        }

        if (_app.BufferIsDirty)
        {
            table.Caption($"Buffer has been modified - Page {_app.Page + 1}/{_app.LastPage + 1}");
        }
        else
        {
            table.Caption($"Page {_app.Page + 1}/{_app.LastPage + 1}");
        }

        return table;
    }
}