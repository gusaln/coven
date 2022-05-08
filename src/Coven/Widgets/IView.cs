using Spectre.Console.Rendering;

namespace Coven.Widgets;

public interface IView : IRenderable
{
    void UpdateByteCell(int cursor, IRenderable content);

    void ReloadBytes();
}
