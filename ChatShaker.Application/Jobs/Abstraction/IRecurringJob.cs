namespace ChatShaker.Application.Jobs.Abstraction;

public interface IRecurringJob
{
    Task ExecuteJob(CancellationToken cancellationToken);
}