
using System.Collections.ObjectModel;
using ChatShaker.ChatMauiApp.Models.Dto;
using ChatShaker.ChatMauiApp.Services.Interfaces;
using Prism.Commands;
using Prism.Navigation;
using Prism.Navigation.Regions;

namespace ChatShaker.ChatMauiApp.ViewModels;

public class ChatListViewModel : BindableBase, INavigatedAware
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
    private readonly IRegionManager _regionManager;

    private CancellationTokenSource _cancellationToken;

    public ObservableCollection<ChatListItem> Chats { get; } = new();
    public DelegateCommand<ChatListItem> OpenChatCommand { get; }

    public ChatListViewModel(IRoomApiService roomApiService, 
        ISignalRConnectionManager connectionManager, 
        INavigationService navigationService, 
        IChatConnectionService chatConnectionService,
        IAppPopupService popupService,
        IRegionManager regionManager)
    {
        _roomApiService = roomApiService;
        _connectionManager = connectionManager;
        _navigationService = navigationService;
        _chatConnectionService = chatConnectionService;
        _popupService = popupService;
        _regionManager = regionManager;

        OpenChatCommand = new DelegateCommand<ChatListItem>(OpenChat);
    }

    public async void OnNavigatedTo(INavigationParameters parameters)
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

    public void OnNavigatedFrom(INavigationParameters parameters)
    {
        _cancellationToken?.Cancel();
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
            {"RoomId", item.RoomPublicId}
        };

        _regionManager.RequestNavigate("MainRegion", "ChatRoomPage", navigationResult =>
        {
            if (navigationResult.Success == false && navigationResult.Exception != null)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation failed: {navigationResult.Exception.Message}");
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await _popupService.ShowError($"Navigation failed: {navigationResult.Exception.Message}");
                });
            }
        }, parameters);
    }
}
