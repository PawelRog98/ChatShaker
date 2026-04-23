using ChatShaker.ChatMauiApp.Models.Dto;
using ChatShaker.ChatMauiApp.Services.Interfaces;

namespace ChatShaker.ChatMauiApp.ViewModels;

public class ChatHistoryViewModel : BindableBase
{
    private readonly IChatDataService _messages;
    private readonly IAppPopupService _popupService;

    public event Action<IEnumerable<MessageDto>>? OnMessageLoaded;
    
    public bool IsLoading { get; private set; }
    public int PageIndex { get; private set; } = 0;
    public int PageSize { get; } = 20;

    public ChatHistoryViewModel(IChatDataService messages, IAppPopupService popupService)
    {
        _messages = messages;
        _popupService = popupService;
    }

    public async Task LoadMessages(Guid roomPublicId)
    {
        if(IsLoading)
            return;
        
        IsLoading = true;
        RaisePropertyChanged(nameof(IsLoading));
        
        try
        {
            var data = await _messages.GetDecryptedMessages(roomPublicId, PageIndex++, PageSize);
            OnMessageLoaded?.Invoke(data);
        }
        catch (Exception ex)
        {
            await _popupService.ShowError($"Failed to load history: {ex.Message}");
        }
        finally
        {
            IsLoading = false; 
            RaisePropertyChanged(nameof(IsLoading));
        }
    }

    public void Reset()
    {
        PageIndex = 0;
    }
}
