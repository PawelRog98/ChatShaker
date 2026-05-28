using System.Reflection;
using ChatShaker.Application.Jobs.Abstraction;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace ChatShaker.Infrastructure.Jobs;

public sealed class RecurringJobRegistrar
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IRecurringJobManager _recurringJobManager;

    public RecurringJobRegistrar(IServiceProvider serviceProvider, IRecurringJobManager recurringJobManager)
    {
        _serviceProvider = serviceProvider;
        _recurringJobManager = recurringJobManager;
    }

    public void Register()
    {
        using var scope = _serviceProvider.CreateScope();
        
        var jobs = scope.ServiceProvider.GetServices<IRecurringJob>();

        foreach (var job in jobs)
        {
            var type  = job.GetType();

            var attribute = type.GetCustomAttribute<RecurringJobAttribute>();
            if (attribute == null)
                continue;
            
            _recurringJobManager.AddOrUpdate(
                recurringJobId: attribute.JobName,
                methodCall: () => job.ExecuteJob(CancellationToken.None),
                cronExpression: attribute.CronExpression,
                queue: attribute.Queue);
        }
    }
}