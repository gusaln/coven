using Coven.Widgets;

using Spectre.Console;

namespace Coven;

public class InformationBoxMode : IMode
{
    private Application _app;
    private string _message;
    private IMode _next;

    public InformationBoxMode(Application app, string message, IMode next)
    {
        _app = app;
        _message = message;
        _next = next;
    }

    public void BuildView(IApplicationController controller)
    {
        controller.Set(
            new InformationPanel().SetContent(new Text(_message)),
            caption: "Press any key to dismiss",
            status: ""
            );
    }

    public Task<IMode> HandleInputAsync(ConsoleKeyInfo keyInfo, IApplicationController controller)
    {
        return Task.FromResult(_next);
    }
}