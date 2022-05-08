namespace Coven.Command;

public class Insert : ICommand
{
    public int Page { get; init; }
    public int Position { get; init; }
    public int NumberOfBytes { get; init; }

    public Task Do(Application app)
    {
        return app.InsertEmptyBytes(Page, Position, NumberOfBytes);
    }

    public Task Undo(Application app)
    {
        return app.DeleteBytes(Page, Position, NumberOfBytes);
    }
}
