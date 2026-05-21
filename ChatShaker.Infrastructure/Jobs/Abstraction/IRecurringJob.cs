namespace ChatShaker.Infrastructure.Jobs.Interfaces;

public interface IRecurringJob
{
    Task ExecuteJob(CancellationToken cancellationToken);
}