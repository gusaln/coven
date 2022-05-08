
using System.Data;

using Spectre.Console;
using Spectre.Console.Rendering;

namespace Coven.Widgets;

public class InformationPanel : IView
{
    private IRenderable _content;
    private IRenderable _view;

    public InformationPanel()
    {
        _content = new Text(string.Empty);
        _view = generateView();
    }

    public InformationPanel SetContent(IRenderable content)
    {
        _content = content;
        _view = generateView();

        return this;
    }

    public void UpdateByteCell(int cursor, IRenderable content)
    { }

    public void ReloadBytes()
    { }

    public Measurement Measure(RenderContext context, int maxWidth)
    {
        return ((IRenderable)_view).Measure(context, new int[] { maxWidth, CanonicalBytesRepresentationTable.TABLE_WIDTH }.Min());
    }

    public IEnumerable<Segment> Render(RenderContext context, int maxWidth)
    {
        return ((IRenderable)_view).Render(context, new int[] { maxWidth, CanonicalBytesRepresentationTable.TABLE_WIDTH }.Min());
    }

    private Panel generateView()
    {
        return new Panel(_content).Padding(1, 2, 1, 2);
    }
}