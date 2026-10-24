namespace TaskList.Domain.Common;

public abstract class Entity
{
    protected Entity(Guid id) => Id = id;

    // Required by EF Core.
    protected Entity()
    {
    }

    public Guid Id { get; private init; }
}
