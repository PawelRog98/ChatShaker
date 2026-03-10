using ChatShaker.ChatMauiApp.Models.Dto;
using ChatShaker.ChatMauiApp.Services.Interfaces;

namespace ChatShaker.ChatMauiApp.ViewModels;

public class ChatHistoryViewModel : BindableBase
{
    private readonly IChatDataService _messages;

    public event Action<IEnumerable<MessageDto>>? OnMessageLoaded;
    
    public bool IsLoading { get; private set; }
    public int PageIndex { get; private set; } = 0;
    public int PageSize { get; }

    public ChatHistoryViewModel(IChatDataService messages)
    {
        _messages = messages;
    }

    public async Task LoadMessages(Guid roomPublicId)
    {
        if(IsLoading)
            return;
        
        IsLoading = true;
        RaisePropertyChanged(nameof(IsLoading));
        
        var data = await _messages.GetDecryptedMessages(roomPublicId, PageIndex++, PageSize);
        OnMessageLoaded?.Invoke(data);
        IsLoading = false; 
        
        RaisePropertyChanged(nameof(IsLoading));
    }

    public void Reset()
    {
        PageIndex = 0;
    }
}
