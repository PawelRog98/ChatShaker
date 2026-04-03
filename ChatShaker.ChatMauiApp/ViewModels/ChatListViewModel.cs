
using System.Collections.ObjectModel;
using ChatShaker.ChatMauiApp.Models.Dto;
using ChatShaker.ChatMauiApp.Services.Interfaces;
using Prism.Commands;
using Prism.Navigation;

namespace ChatShaker.ChatMauiApp.ViewModels;

public class ChatListViewModel : BindableBase, IInitializeAsync
{
    private string _name;
    public string Name
    {
        get { return _name; }
        set { SetProperty(ref _name, value); }
    }

    private readonly IRoomApiService _roomApiService;
    private readonly ISignalRConnectionManager _connectionManager;
    private readonly INavigationService _navigationService;
    private readonly IChatConnectionService _chatConnectionService;
    private readonly IAppPopupService _popupService;

    private CancellationTokenSource _cancellationToken;

    public ObservableCollection<ChatListItem> Chats { get; } = new();
    public DelegateCommand<ChatListItem> OpenChatCommand { get; }

    public ChatListViewModel(IRoomApiService roomApiService, 
        ISignalRConnectionManager connectionManager, 
        INavigationService navigationService, 
        IChatConnectionService chatConnectionService,
        IAppPopupService popupService)
    {
        _roomApiService = roomApiService;
        _connectionManager = connectionManager;
        _navigationService = navigationService;
        _chatConnectionService = chatConnectionService;
        _popupService = popupService;

        OpenChatCommand = new DelegateCommand<ChatListItem>(OpenChat);
    }

    public async Task InitializeAsync(INavigationParameters parameters)
    {
        try
        {
            _cancellationToken = new CancellationTokenSource();

            var rooms = await GetRoomItems();
            Chats.Clear();

            await _connectionManager.Connect(_cancellationToken.Token);
            _chatConnectionService.BindEvents();
    
            foreach (var room in rooms)
            {
                Chats.Add(room);
                await _chatConnectionService.JoinRoom(room.RoomPublicId);
            }
        }
        catch(Exception ex)
        {
            await _popupService.ShowError(ex.Message);
        }
    }

    private async Task<List<ChatListItem>> GetRoomItems()
    {
        var rooms = await _roomApiService.GetRooms();

        return rooms.Data;
    }

    private async void OpenChat(ChatListItem item)
    {
        var parameters = new NavigationParameters
        {
            {"roomId", item.RoomPublicId}
        };

        await _navigationService.NavigateAsync("ChatRoomPage", parameters);
    }
}
