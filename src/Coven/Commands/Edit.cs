namespace Coven.Command;

public class Edit : ICommand
{
    public int Page { get; init; }
    public int Position { get; init; }
    public byte NewByte { get; init; }
    public byte OriginalByte { get; init; }

    public Task Do(Application app)
    {
        return app.EditByte(Page, Position, NewByte);
    }

    public async Task Undo(Application app)
    {
        await app.EditByte(Page, Position, OriginalByte);
    }
}