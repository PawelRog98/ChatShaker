using ChatShaker.Application.Jobs.Abstraction;
using ChatShaker.Application.Tokens.Commands.DeleteExpiredTokens;
using MediatR;

namespace ChatShaker.Application.Jobs.Recurring;

[RecurringJob("expired_token_cleaner", "0 0 * * *")]
public class ExpiredTokenCleanerJob : IRecurringJob
{
    private readonly IMediator _mediator;

    public ExpiredTokenCleanerJob(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task ExecuteJob(CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteExpiredTokenCommand(), cancellationToken);
        await Task.CompletedTask;
    }
}