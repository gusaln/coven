using Coven.Widgets;

using Spectre.Console.Rendering;

namespace Coven;

public interface IMode
{
    void BuildView(IApplicationController controller);

    Task<IMode> HandleInputAsync(ConsoleKeyInfo keyInfo, IApplicationController controller);

    // string GetCaption();

    // string GetStatus();
}
