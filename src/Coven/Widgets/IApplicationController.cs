using Spectre.Console.Rendering;

namespace Coven.Widgets;

public interface IApplicationController
{
    void UpdateByteCell(int cursor, IRenderable content);

    void ReloadBytes();

    IApplicationController Set(IView view);

    IApplicationController Set(IView view, string caption, string status);

    IApplicationController Set(string caption, string status);

    IApplicationController SetCaption(string text);

    IApplicationController SetStatus(string text);
}
