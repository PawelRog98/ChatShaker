using System.Collections.ObjectModel;
using ChatShaker.ChatMauiApp.Models.Local;
using ChatShaker.ChatMauiApp.Services.Api;
using ChatShaker.ChatMauiApp.Services.Interfaces;

namespace ChatShaker.ChatMauiApp.ViewModels;

public class ChatCreateRoomViewModel : BindableBase, IInitializeAsync
{
    private string _name;
    public string Name
    {
        get { return _name; }
        set { SetProperty(ref _name, value); }
    }

    private string _searchName;
    public string SearchName
    {
        get { return _searchName; }
        set
        {
            SetProperty(ref _searchName, value); 
            Filter();
        }
    }

    private readonly IRoomKeyService _roomKeyService;
    private readonly IUserApiService _userApiService;
    private readonly INavigationService _navigationService;

    public ObservableCollection<SelectableUser> Users { get; } = new();
    public ObservableCollection<SelectableUser> FilteredUsers { get; } = new();
    
    public DelegateCommand ConfirmCommand { get;  }
    
    public bool IsLoading { get; private set; }

    public ChatCreateRoomViewModel(IRoomKeyService roomKeyService,  IUserApiService userApiService,  INavigationService navigationService)
    {
        _roomKeyService = roomKeyService;
        _userApiService = userApiService;
        _navigationService = navigationService;

        ConfirmCommand = new DelegateCommand(Confirm);
    }

    private async Task LoadFriends()
    {
        IsLoading = true;

        var users = await _userApiService.GetFriends(_name);
        foreach (var user in users.Data)
            Users.Add(new SelectableUser(user));
        
        IsLoading = false;
    }

    public async Task InitializeAsync(INavigationParameters parameters)
    {
        await LoadFriends();
        
    }

    private async void Confirm()
    {
        IsLoading = true;
        var selectedUsers = Users.Where(x=>x.IsSelected)
            .Select(x=>x.User)
            .ToList();
        
        await _roomKeyService.GenerateAndSaveRoomKey(selectedUsers, _name);
        IsLoading = false;

        await _navigationService.GoBackAsync();
    }

    private void Filter()
    {
        var text = SearchName?.ToLower() ?? "";

        var filteredData = string.IsNullOrWhiteSpace(text)
            ? Users
            : Users.Where(x => x.User.Name.ToLower().Contains(text));
        
        FilteredUsers.Clear();
        foreach (var user in filteredData)
            FilteredUsers.Add(user);
    }
}