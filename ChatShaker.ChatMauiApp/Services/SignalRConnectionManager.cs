using ChatShaker.ChatMauiApp.Models.Dto;
using ChatShaker.ChatMauiApp.Services.Interfaces;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChatShaker.ChatMauiApp.Services;

public class SignalRConnectionManager : ISignalRConnectionManager, IDisposable
{
    private HubConnection _connection;
    private IAuthService _authService;

    public SignalRConnectionManager(IAuthService authService)
    {
        _authService = authService;
    }

    public HubConnection Connection => _connection;

    public async Task Connect(CancellationToken cancellationToken)
    {
        if (_connection != null && _connection.State != HubConnectionState.Disconnected)
            return;

        var token = await _authService.GetAccessToken();
        
        
#if ANDROID
        // Use 10.0.2.2 for Android emulator. For physical devices, use the host machine's IP.
        var hubUrl = "http://10.0.2.2:8080/chatHub";
#else
        var hubUrl = "http://localhost:8080/chatHub";
#endif

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.Transports = HttpTransportType.WebSockets;
            })
            .WithAutomaticReconnect()
            .Build();

        await _connection.StartAsync(cancellationToken);
    }

    public async Task Disconnect(CancellationToken cancellationToken = default)
    {
        if (_connection != null)
        {
            await _connection.StopAsync(cancellationToken);
            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    public void Dispose()
    {
        _ = Disconnect();
    }
}
