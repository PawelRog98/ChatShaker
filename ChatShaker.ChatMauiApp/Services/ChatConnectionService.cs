using ChatShaker.ChatMauiApp.Models.Dto;
using ChatShaker.ChatMauiApp.Services.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChatShaker.ChatMauiApp.Services;

public class ChatConnectionService : IChatConnectionService
{
    private readonly ISignalRConnectionManager _signalRConnectionManager;

    public ChatConnectionService(ISignalRConnectionManager signalRConnectionManager)
    {
        _signalRConnectionManager = signalRConnectionManager;
    }

    public event Action<MessageDto>? OnMessageSent;
    public event Action<Guid>? OMessageDelivered;
    public event Action<Guid>? OnMessageRead;
    public event Action<Guid>? OnUserAdded;

    private bool _eventsBound = false;
    public void BindEvents()
    {
        if (_eventsBound) return;
        _eventsBound = true;

        var connection = _signalRConnectionManager.Connection;

        connection.On<MessageDto>("MessageSent", message =>
            OnMessageSent?.Invoke(message));

        connection.On<Guid>("MessageDelivered", id =>
            OMessageDelivered?.Invoke(id));

        connection.On<Guid>("MessageRead", id => 
            OnMessageRead?.Invoke(id));

        connection.On<Guid>("UserAdded", id =>
            OnUserAdded?.Invoke(id));
    }

    public async Task AddUser(Guid roomPublicId)
        => await _signalRConnectionManager.Connection.InvokeAsync("AddUser");

    public async Task MarkAsRead(Guid messagePublicId)
       => await _signalRConnectionManager.Connection.InvokeAsync("MarkAsRead", messagePublicId);

    public async Task MarkAsDelivered(Guid messagePublicId)
        => await _signalRConnectionManager.Connection.InvokeAsync("MarkAsDelivered", messagePublicId);

    public async Task SendMessage(MessageDto messageDto)
        => await _signalRConnectionManager.Connection.InvokeAsync("SendMessage", messageDto);

    public async Task JoinRoom(Guid roomPublicId)
        => await _signalRConnectionManager.Connection.InvokeAsync("JoinRoom", roomPublicId);
}
