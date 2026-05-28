namespace ChatShaker.Domain.Repositories;

public interface IUnitOfWork
{
    Task BeginTransaction(CancellationToken cancellationToken);
    Task SaveChanges(CancellationToken cancellationToken);
    Task Commit(CancellationToken cancellationToken);
    Task Rollback(CancellationToken cancellationToken);
}
