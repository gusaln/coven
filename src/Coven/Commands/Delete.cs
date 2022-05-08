using System.Collections.Immutable;

namespace Coven.Command;

public class Delete : ICommand
{
    public int Page { get; init; }
    public int Position { get; init; }
    public ImmutableArray<byte> BytesDeleted { get; init; }

    public Task Do(Application app)
    {
        return app.DeleteBytes(Page, Position, BytesDeleted.Length);
    }

    public async Task Undo(Application app)
    {
        await app.InsertEmptyBytes(Page, Position, BytesDeleted.Length);

        for (int i = 0; i < BytesDeleted.Length; i++)
        {
            await app.EditByte(Page, Position, BytesDeleted[Position + i]);
        }
    }
}
