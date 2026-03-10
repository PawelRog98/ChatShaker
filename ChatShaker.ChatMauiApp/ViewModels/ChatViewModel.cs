using System.Collections.ObjectModel;
using ChatShaker.ChatMauiApp.Models.Dto;
using ChatShaker.ChatMauiApp.Models.Enums;
using ChatShaker.ChatMauiApp.Services.Interfaces;

namespace ChatShaker.ChatMauiApp.ViewModels;

public class ChatViewModel : BaseViewModel, INavigationAware
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
    
    public ChatHistoryViewModel History { get; }

    private Guid _roomId;
    private byte[] _roomKey;

    public DelegateCommand SendMessageCommand { get;}
    public DelegateCommand LoadMoreCommand { get;}
    
    public ObservableCollection<MessageDto> Messages { get; } = new();

    public ChatViewModel(IChatConnectionService chatHub, IChatDataService chatDataService, ChatHistoryViewModel history)
    {
        _chatHub = chatHub;
        _chatDataService = chatDataService;
        History = history;

        SendMessageCommand = new DelegateCommand(SendMessage);
        LoadMoreCommand = new DelegateCommand(LoadMore);
        

        _chatHub.OnMessageSent += OmMessageSent;
        _chatHub.OMessageDelivered += OnMessageDelivered;
        _chatHub.OnMessageRead += OnMessageRead;
        History.OnMessageLoaded += OnMessagesLoaded;
    }

    private async void SendMessage()
    {
        if(string.IsNullOrWhiteSpace(OngoingText))
            return;
        
        var (encryptedText, nonce) = await _chatDataService.SaveEncryptedMessage(_roomId, OngoingText);

        await _chatHub.SendMessage(new MessageDto
        {
            CipherText = encryptedText,
            Nonce = nonce,
            ClientMessageId = Guid.NewGuid(),
            ChatRoomPublicId =  _roomId,
            Status = MessageStatusEnum.Sent,
            SentDataTimeUtc =  DateTime.UtcNow
        });
        
        OngoingText = string.Empty;
        RaisePropertyChanged(nameof(OngoingText));
    }

    private async void LoadMore()
    {
        await History.LoadMessages(_roomId);
    }

    private void OmMessageSent(MessageDto message)
    {
        MainThread.BeginInvokeOnMainThread(() =>{
            Messages.Add(message);
        });
    }

    private void OnMessageRead(Guid messagePublicId)
    {
        var msg = Messages.FirstOrDefault(m => m.ClientMessageId == messagePublicId);
        msg.Status = MessageStatusEnum.Read;
    }

    private void OnMessageDelivered(Guid messagePublicId)
    {
        var msg = Messages.FirstOrDefault(m => m.ClientMessageId == messagePublicId);
        msg.Status = MessageStatusEnum.Delivered;
    }

    private void OnMessagesLoaded(IEnumerable<MessageDto> messages)
    {
        foreach(var message in messages.Reverse())
            Messages.Insert(0,message);
    }

    public void OnNavigatedFrom(INavigationParameters parameters)
    {
        
    }

    public async void OnNavigatedTo(INavigationParameters parameters)
    {
        _roomId = parameters.GetValue<Guid>("RoomId");
        
        History.Reset();
        
        _chatHub.BindEvents();
        await History.LoadMessages(_roomId);
    }
}
