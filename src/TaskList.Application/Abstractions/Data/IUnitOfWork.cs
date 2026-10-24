namespace TaskList.Application.Abstractions.Data;

/// <summary>Saves all changes of a use case in one transaction. Called by command handlers, never by repositories.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
