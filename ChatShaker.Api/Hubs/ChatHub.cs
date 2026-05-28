using System.Security.Claims;
using ChatShaker.Api.Clients;
using ChatShaker.Application.Chats.Commands.JoinRoom;
using ChatShaker.Application.MessagesManagment.Commands.MarkMessageAsDelivered;
using ChatShaker.Application.MessagesManagment.Commands.MarkMessageAsRead;
using ChatShaker.Application.MessagesManagment.Commands.SendMessage;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatShaker.Api.Hubs;

[Authorize]
public class ChatHub : Hub<IChatClient>
{
    private readonly IMediator _mediator;
    private readonly ILogger<ChatHub> _logger;
    public ChatHub(IMediator mediator, ILogger<ChatHub> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task SendMessage(SendMessageDto sendMessageDto)
    {
        try
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new HubException("Unauthorized");

            var cancellationToken = Context.ConnectionAborted;
            
            await _mediator.Send(new SendMessageCommand(sendMessageDto, long.Parse(userId)), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SendMessage: {Message}", ex.Message);
            throw new HubException(ex.Message);
        }
    }

    public async Task MarkAsRead(Guid messagePublicId)
    {
        try
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? throw new HubException("Unauthorized");

            var cancellationToken = Context.ConnectionAborted;

            await _mediator.Send(new MarkMessageAsReadCommand(messagePublicId, long.Parse(userId)), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in MarkAsRead: {Message}", ex.Message);
            throw new HubException(ex.Message);
        }
    }

    public async Task MarkAsDelivered(Guid messagePublicId)
    {
        try
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? throw new HubException("Unauthorized");

            var cancellationToken = Context.ConnectionAborted;

            await _mediator.Send(new MarkMessageAsDeliveredCommand(messagePublicId, long.Parse(userId)), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in MarkAsDelivered: {Message}", ex.Message);
            throw new HubException(ex.Message);
        }
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
