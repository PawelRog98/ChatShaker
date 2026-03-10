using ChatShaker.ChatMauiApp.Models.Dto;
using ChatShaker.ChatMauiApp.Services.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;


namespace ChatShaker.ChatMauiApp.Services;

public class GlobalConnectionService : IGlobalConnectionService
{
    private readonly ISignalRConnectionManager _connectionManager;

    public event Action<ChatListItem>? OnRoomActivity;

    public GlobalConnectionService(ISignalRConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public void Bind()
    {
        _connectionManager.Connection.On<ChatListItem>("RoomActivity", item => 
            OnRoomActivity?.Invoke(item));
    }
}