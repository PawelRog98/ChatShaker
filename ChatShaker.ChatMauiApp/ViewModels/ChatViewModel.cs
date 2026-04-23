using System.Collections.ObjectModel;
using ChatShaker.ChatMauiApp.Models.Dto;
using ChatShaker.ChatMauiApp.Models.Enums;
using ChatShaker.ChatMauiApp.Services.Api;
using ChatShaker.ChatMauiApp.Services.Interfaces;
using Prism.Navigation.Regions;

namespace ChatShaker.ChatMauiApp.ViewModels;

public class ChatViewModel : BaseViewModel, IRegionAware
{
    #region Properties
    private string _ongoingText;
    public string OngoingText
    {
        get { return _ongoingText; }
        set { SetProperty(ref _ongoingText, value); }
    }
    #endregion
    
    private readonly IChatConnectionService _chatHub;
    private readonly IChatDataService _chatDataService;
    private readonly IRoomKeyService _roomKeyService;
    private readonly IUserApiService _userApiService;
    private readonly IRoomApiService _roomApiService;
    private readonly IKeyApiService _keyApiService;
    private readonly ICryptoService _cryptoService;
    private readonly IAuthTokenProvider _authTokenProvider;
    private readonly IAppPopupService _popupService;
    
    public ChatHistoryViewModel History { get; }

    private Guid _roomId;
    private string _userId;

    public DelegateCommand SendMessageCommand { get;}
    public DelegateCommand LoadMoreCommand { get;}
    
    public ObservableCollection<MessageDto> Messages { get; } = new();

    public ChatViewModel(IChatConnectionService chatHub, 
        IChatDataService chatDataService, 
        IRoomKeyService roomKeyService,
        IUserApiService userApiService,
        IRoomApiService roomApiService,
        IKeyApiService  keyApiService,
        ICryptoService cryptoService,
        IAuthTokenProvider authTokenProvider,
        IAppPopupService popupService,
        ChatHistoryViewModel history)
    {
        _chatHub = chatHub;
        _chatDataService = chatDataService;
        _roomKeyService = roomKeyService;
        _userApiService = userApiService;
        _roomApiService = roomApiService;
        _keyApiService = keyApiService;
        _cryptoService = cryptoService;
        _authTokenProvider = authTokenProvider;
        _popupService = popupService;
        
        History = history;

        SendMessageCommand = new DelegateCommand(SendMessage);
        LoadMoreCommand = new DelegateCommand(LoadMore);
        
        _chatHub.OnMessageSent += OnMessageSent;
        _chatHub.OMessageDelivered += OnMessageDelivered;
        _chatHub.OnMessageRead += OnMessageRead;
        History.OnMessageLoaded += OnMessagesLoaded;
    }

    private async void SendMessage()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(OngoingText))
                return;

            var (encryptedText, nonce) = await _chatDataService.SaveEncryptedMessage(_roomId, OngoingText);

            await _chatHub.SendMessage(new MessageDto
            {
                CipherText = encryptedText,
                Nonce = nonce,
                ClientMessageId = Guid.NewGuid(),
                ChatRoomPublicId = _roomId,
                Status = MessageStatusEnum.Sent,
                SentAtUtc = DateTime.UtcNow
            });

            OngoingText = string.Empty;
            RaisePropertyChanged(nameof(OngoingText));
        }
        catch (Exception ex)
        {
            await _popupService.ShowError($"Failed to send message: {ex.Message}");
        }
    }

    private async void LoadMore()
    {
        try
        {
            await History.LoadMessages(_roomId);
        }
        catch (Exception ex)
        {
            await _popupService.ShowError($"Failed to load more messages: {ex.Message}");
        }
    }

    private async void OnMessageSent(MessageDto message)
    {
        try
        {
            await Task.Run(async () =>
            {
                var roomKey = await _roomKeyService.GetRoomKey(_roomId);
                message.CipherText = await _cryptoService.DecryptMessage(roomKey, message.CipherText, message.Nonce);
            });

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (Messages.All(m => m.ClientMessageId != message.ClientMessageId))
                {
                    message.IsMine = message.SenderPublicId?.ToString() == _userId;
                    Messages.Add(message);
                }
            });

            var userIdAsGuid = Guid.Parse(_userId);
            if (message.SenderPublicId != userIdAsGuid)
            {
                await _chatHub.MarkAsRead(message.PublicId);
            }
        }
        catch (Exception ex)
        {
            await _popupService.ShowError($"Error processing received message: {ex.Message}");
        }
    }

    private void OnMessageRead(Guid messagePublicId)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var msg = Messages.FirstOrDefault(m => m.PublicId == messagePublicId);
            if (msg != null)
            {
                msg.Status = MessageStatusEnum.Read;
            }
        });
    }

    private void OnMessageDelivered(Guid messagePublicId)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var msg = Messages.FirstOrDefault(m => m.PublicId == messagePublicId);
            if (msg != null)
            {
                msg.Status = MessageStatusEnum.Delivered;
            }
        });
    }

    private async void OnMessagesLoaded(IEnumerable<MessageDto> messages)
    {
        MainThread.BeginInvokeOnMainThread(() => {
            foreach(var message in messages)
            {
                if (Messages.All(m => m.ClientMessageId != message.ClientMessageId))
                {
                    message.IsMine = message.SenderPublicId?.ToString() == _userId;
                    Messages.Insert(0, message);
                }
            }
        });

        var userIdAsGuid = Guid.Parse(_userId);
        var messagesUnred = messages.Where(x => x.Status != MessageStatusEnum.Read && x.SenderPublicId != userIdAsGuid).ToList();

        foreach (var message in messagesUnred)
        {
            await _chatHub.MarkAsRead(message.PublicId);
        }
    }

    private async Task InitializeChatData()
    {
        var response =  await _roomApiService.GetRoom(_roomId);

        var room = response.Data;
        var keys = room.ChatRoomKeyBlobDtos.Select(x => x.UserPublicId).ToList();
        var usersData = await _userApiService.GetParticipants(keys);
        
        await _roomKeyService.InitializeRoomKeyForExistingRoom(usersData.Data, room.ChatRoomPublicId.Value);
    }

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
        UnsubscribeEvents();
    }

    private void UnsubscribeEvents()
    {
        _chatHub.OnMessageSent -= OnMessageSent;
        _chatHub.OMessageDelivered -= OnMessageDelivered;
        _chatHub.OnMessageRead -= OnMessageRead;
        History.OnMessageLoaded -= OnMessagesLoaded;
    }

    public async void OnNavigatedTo(NavigationContext navigationContext)
    {
        IsBusy = true;
        try
        {
            await Task.Run(async () => await InternalOnNavigatedTo(navigationContext.Parameters));
        }
        finally
        {
            IsBusy = false;
        }
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    private async Task InternalOnNavigatedTo(INavigationParameters parameters)
    {
        _roomId = parameters.GetValue<Guid>("RoomId");
        _userId = _authTokenProvider.GetAuthToken().Result.UserId;
        
        var isInitialized = await _roomApiService.CheckIfRoomInitialized(_roomId);
        if (isInitialized.Data != true)
        {
            await InitializeChatData();
        }
        
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            Messages.Clear();
        });
        
        History.Reset();
        
        _chatHub.BindEvents();
        await History.LoadMessages(_roomId);
    }
}
