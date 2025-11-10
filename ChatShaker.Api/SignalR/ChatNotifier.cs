using ChatShaker.Api.Clients;
using ChatShaker.Api.Hubs;
using ChatShaker.Application.Interfaces;
using ChatShaker.Application.MessagesManagment.Commands.SendMessage;
using Microsoft.AspNetCore.SignalR;

namespace ChatShaker.Api.SignalR;

public class ChatNotifier : IChatNotifier
{
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;

    public ChatNotifier(IHubContext<ChatHub, IChatClient> hubContext)
    {
        _hubContext = hubContext;
    }
    
    public async Task MessageSent(Guid roomPublicId, MessageDto messageDto, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.Group(roomPublicId.ToString())
            .MessageSent(messageDto)
            .WaitAsync(cancellationToken);
    }

    public async Task MessageRead(Guid roomPublicId, Guid messagePublicId, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.Group(roomPublicId.ToString())
            .MessageRead(messagePublicId)
            .WaitAsync(cancellationToken);
    }

    public async Task UserAdded(Guid roomPublicId, Guid userPublicId, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.Group(roomPublicId.ToString())
            .UserAdded(userPublicId)
            .WaitAsync(cancellationToken);
    }
}
