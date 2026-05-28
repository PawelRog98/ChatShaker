using System.Collections.ObjectModel;
using System.Text.Json;
using ChatShaker.ChatMauiApp.Models.Dto;
using ChatShaker.ChatMauiApp.Models.Enums;
using ChatShaker.ChatMauiApp.Models.Local;
using ChatShaker.ChatMauiApp.Services.Api;
using ChatShaker.ChatMauiApp.Services.Interfaces;
using Microsoft.Maui.Graphics.Platform;
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

    private string _selectedImagePath;

    public string SelectedImagePath
    {
        get {  return _selectedImagePath; }
        set { SetProperty(ref _selectedImagePath, value); }
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
    private readonly IImageService _imageService;
    
    public ChatHistoryViewModel History { get; }

    private Guid _roomId;
    private string _userId;

    public DelegateCommand SendMessageCommand { get;}
    public DelegateCommand LoadMoreCommand { get;}
    public DelegateCommand PickImageCommand { get; }
    public DelegateCommand ClearImageCommand { get; }
    public DelegateCommand<MessageDto> OpenImageCommand { get; }
    
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
        IImageService imageService,
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
        _imageService = imageService;
        
        History = history;

        SendMessageCommand = new DelegateCommand(SendMessage, () => !IsBusy).ObservesProperty(() => IsBusy);
        LoadMoreCommand = new DelegateCommand(LoadMore, () => !IsBusy).ObservesProperty(() => IsBusy);
        PickImageCommand = new DelegateCommand(PickImage, () => !IsBusy).ObservesProperty(() => IsBusy);
        ClearImageCommand = new DelegateCommand(ClearImage);
        OpenImageCommand = new DelegateCommand<MessageDto>(OpenImage);
    }

    private void SubscribeEvents()
    {
        UnsubscribeEvents();
        _chatHub.OnMessageSent += OnMessageSent;
        _chatHub.OMessageDelivered += OnMessageDelivered;
        _chatHub.OnMessageRead += OnMessageRead;
        _chatHub.OnUserIdentityChanged += OnUserIdentityChanged;
        History.OnMessageLoaded += OnMessagesLoaded;
    }

    private void UnsubscribeEvents()
    {
        _chatHub.OnMessageSent -= OnMessageSent;
        _chatHub.OMessageDelivered -= OnMessageDelivered;
        _chatHub.OnMessageRead -= OnMessageRead;
        _chatHub.OnUserIdentityChanged -= OnUserIdentityChanged;
        History.OnMessageLoaded -= OnMessagesLoaded;
    }

    private async void OnUserIdentityChanged(Guid userPublicId)
    {
        await _popupService.ShowSuccess("One of the participants has changed their security keys. Future messages will be encrypted using the new keys.");
    }

    private void ClearImage()
    {
        SelectedImagePath = null;
    }

    private async void OpenImage(MessageDto message)
    {
        if (message.MessageType != MessageTypeEnum.Image || message.ImageContent == null)
            return;

        if (IsBusy) 
            return;

        try
        {
            IsBusy = true;
            if (string.IsNullOrEmpty(message.FullImageLocalPath))
            {
                message.IsDownloadingFullImage = true;
                message.FullImageLocalPath = await LoadImageToFile(message.ImageContent.FilePublicId, message.KeyVersion);
            }

            await _popupService.ShowImage(message.FullImageLocalPath);
        }
        catch (Exception ex)
        {
            await _popupService.ShowError($"Failed to download full image: {ex.Message}");
        }
        finally
        {
            message.IsDownloadingFullImage = false;
            IsBusy = false;
        }
    }

    private async Task<string> LoadImageToFile(string publicIdStr, long keyVersion)
    {
        var publicId = Guid.Parse(publicIdStr);
        var bytes = await _chatDataService.GetDecryptedFile(_roomId, publicId, keyVersion);
        
        var localPath = Path.Combine(FileSystem.CacheDirectory, $"{publicIdStr}.jpg");
        await File.WriteAllBytesAsync(localPath, bytes);
        
        return localPath;
    }

    private async void SendMessage()
    {
        if (IsBusy) return;

        if (string.IsNullOrWhiteSpace(OngoingText) && string.IsNullOrEmpty(SelectedImagePath))
            return;

        IsBusy = true;
        try
        {
            var currentKeyVersion = await _roomApiService.GetKeyVersion(_roomId);

            if (!currentKeyVersion.Success)
            {
                await _popupService.ShowError($"Failed to get key version: {currentKeyVersion.Message}");
            }
            
            string encryptedMessageContent;
            string nonceData;
            MessageTypeEnum messageType;

            var currentImagePath = SelectedImagePath;
            var currentText = OngoingText;

            if (!string.IsNullOrWhiteSpace(currentImagePath))
            {
                var newName = Guid.NewGuid().ToString();
                
                var thumbBytes = await _imageService.GenerateThumbnail(currentImagePath);
                var thumbId = await _chatDataService.SaveEncryptedFile(_roomId, thumbBytes, "thumb_"+newName, "image/jpeg", currentKeyVersion.Data);
                
                var imageBytes = await File.ReadAllBytesAsync(currentImagePath);
                var fullImageId = await _chatDataService.SaveEncryptedFile(_roomId, imageBytes, newName, "image/jpeg", currentKeyVersion.Data);

                var imageContent = new ImageMessageContent
                {
                    FilePublicId = fullImageId,
                    ThumbnailPublicId = thumbId,
                    Text = currentText ?? "",
                    KeyVersion =  currentKeyVersion.Data,
                };
                
                var jsonString = JsonSerializer.Serialize(imageContent);
                var (cipher, nonce) = await _chatDataService.SaveEncryptedMessage(_roomId, jsonString, currentKeyVersion.Data);

                encryptedMessageContent = cipher;
                nonceData = nonce;
                messageType = MessageTypeEnum.Image;
            }
            else
            {
                var (encryptedText, nonce) = await _chatDataService.SaveEncryptedMessage(_roomId, currentText, currentKeyVersion.Data);
                encryptedMessageContent = encryptedText;
                nonceData = nonce;
                messageType = MessageTypeEnum.Text;
            }

            await _chatHub.SendMessage(new MessageDto
            {
                CipherText = encryptedMessageContent,
                Nonce = nonceData,
                ClientMessageId = Guid.NewGuid(),
                ChatRoomPublicId = _roomId,
                Status = MessageStatusEnum.Sent,
                SentAtUtc = DateTime.UtcNow,
                MessageType = messageType,
                KeyVersion = currentKeyVersion.Data
            });

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                SelectedImagePath = null;
                OngoingText = string.Empty;
            });
        }
        catch (Exception ex)
        {
            await _popupService.ShowError($"Failed to send message: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void PickImage()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var result = await MediaPicker.Default.PickPhotoAsync();

            if (result == null)
                return;

            SelectedImagePath = result.FullPath;
        }
        catch (Exception ex)
        {
            await _popupService.ShowError($"Failed to load image: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void LoadMore()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            await History.LoadMessages(_roomId);
        }
        catch (Exception ex)
        {
            await _popupService.ShowError($"Failed to load more messages: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void OnMessageSent(MessageDto message)
    {
        try
        {
            await ProcessMessage(message);

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

    private async Task ProcessMessage(MessageDto message, bool shouldDecrypt = true)
    {
        if (shouldDecrypt)
        {
            var roomKey = await _roomKeyService.GetRoomKey(_roomId, message.KeyVersion);
            message.CipherText = await _cryptoService.DecryptMessage(roomKey, message.CipherText, message.Nonce);
        }

        if (message.MessageType == MessageTypeEnum.Image)
        {
            try
            {
                message.ImageContent = JsonSerializer.Deserialize<ImageMessageContent>(message.CipherText);
                if (message.ImageContent != null)
                {
                    message.ThumbnailLocalPath = await LoadImageToFile(message.ImageContent.ThumbnailPublicId, message.KeyVersion);
                }
            }
            catch (Exception ex)
            {
                await _popupService.ShowError($"Error processing received message with deserialization: {ex.Message}");
            }
        }
        
        message.IsMine = message.SenderPublicId?.ToString() == _userId;
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
        var messageList = messages.ToList();
        
        foreach (var message in messageList)
        {
            await ProcessMessage(message, shouldDecrypt: false);
        }

        MainThread.BeginInvokeOnMainThread(() => {
            foreach(var message in messageList)
            {
                if (Messages.All(m => m.ClientMessageId != message.ClientMessageId))
                {
                    Messages.Insert(0, message);
                }
            }
        });

        var userIdAsGuid = Guid.Parse(_userId);
        var messagesUnred = messageList.Where(x => x.Status != MessageStatusEnum.Read && x.SenderPublicId != userIdAsGuid).ToList();

        foreach (var message in messagesUnred)
        {
            await _chatHub.MarkAsRead(message.PublicId);
        }
    }

    private async Task InitializeChatData()
    {
        var response =  await _roomApiService.GetRoom(_roomId);

        var room = response.Data;

        await CheckAndRotateKey(room);
        
        var keys = room.ChatRoomKeyBlobDtos.Select(x => x.UserPublicId).ToList();
        var usersData = await _userApiService.GetParticipants(keys);
        
        await _roomKeyService.InitializeRoomKeyForExistingRoom(usersData.Data, room.ChatRoomPublicId.Value);
    }

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
        UnsubscribeEvents();
    }

    public async void OnNavigatedTo(NavigationContext navigationContext)
    {
        IsBusy = true;
        try
        {
            await InternalOnNavigatedTo(navigationContext.Parameters);
        }
        catch (Exception ex)
        {
            throw;
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
        var token = await _authTokenProvider.GetAuthToken();
        _userId = token.UserId;
        
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
        
        SubscribeEvents();
        _chatHub.BindEvents();
        await History.LoadMessages(_roomId);
    }

    private async Task CheckAndRotateKey(RoomDto room)
    {
        var latestKey = room.ChatRoomKeyBlobDtos.OrderByDescending(x => x.Version).FirstOrDefault();
        if (latestKey == null)
            return;

        if ((DateTime.UtcNow - latestKey.CreatedAtUtc).TotalDays >= 30)
        {
            try
            {
                await _roomKeyService.SyncAndRotateKey(_roomId);
            }
            catch (Exception ex)
            {
                
            }
        }
    }
}
