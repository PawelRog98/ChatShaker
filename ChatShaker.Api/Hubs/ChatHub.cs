using System.Security.Claims;
using ChatShaker.Api.Clients;
using ChatShaker.Application.Chats.Commands.JoinRoom;
using ChatShaker.Application.MessagesManagment.Commands.SendMessage;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatShaker.Api.Hubs;

[Authorize]
public class ChatHub : Hub<IChatClient>
{
    private readonly IMediator _mediator;
    public ChatHub(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task SendMessage(SendMessageDto sendMessageDto)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new HubException("Unauthorized");

        var cancellationToken = Context.ConnectionAborted;
        
        await _mediator.Send(new SendMessageCommand(sendMessageDto, long.Parse(userId)), cancellationToken);
    }

    public async Task JoinRoom(Guid roomPublicId)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new HubException("Unauthorized");
        
        var cancellationToken = Context.ConnectionAborted;

        await _mediator.Send(new JoinRoomCommand(roomPublicId, long.Parse(userId)), cancellationToken);

        await Groups.AddToGroupAsync(Context.ConnectionId, roomPublicId.ToString());
    }
}
