using ChatShaker.Application.Jobs.Triggered.Interfaces;
using ChatShaker.Domain.Entities;
using MediatR;
using Hangfire;

namespace ChatShaker.Application.Events;

public class UserRegisteredEvent : INotification
{
    public UserRegisteredEvent(string email, string verificationToken)
    {
        Email = email;
        VerificationToken = verificationToken;
    }
    
    public string Email { get; }
    public string VerificationToken { get; }
}

public class UserRegisteredEventHandler : INotificationHandler<UserRegisteredEvent>
{
    private readonly IBackgroundJobClient _jobs;

    public UserRegisteredEventHandler(IBackgroundJobClient jobs)
    {
        _jobs = jobs;
    }

    public Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        _jobs.Enqueue<ISendVerificationCodeJob>(x=>x.Execute(notification.Email, notification.VerificationToken));
        
        return Task.CompletedTask;
    }
}