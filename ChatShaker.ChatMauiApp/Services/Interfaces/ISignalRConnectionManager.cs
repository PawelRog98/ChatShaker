using ChatShaker.ChatMauiApp.Models.Dto;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChatShaker.ChatMauiApp.Services.Interfaces;

public interface ISignalRConnectionManager
{
    Task Connect(CancellationToken cancellationToken);
    Task Disconnect(CancellationToken cancellationToken);
    HubConnection Connection { get;}
}
