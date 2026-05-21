namespace ChatShaker.Infrastructure.Jobs;

[AttributeUsage(AttributeTargets.Class)]
public class RecurringJobAttribute : Attribute
{
    public string JobName { get; }
    public string CronExpression { get; }
    public RecurringJobAttribute(string jobName, string cronExpression)
    {
        JobName = jobName;
        CronExpression = cronExpression;
    }
}