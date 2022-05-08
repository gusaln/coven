namespace Coven.Command;

public interface ICommand
{
    Task Do(Application app);

    Task Undo(Application app);
}
