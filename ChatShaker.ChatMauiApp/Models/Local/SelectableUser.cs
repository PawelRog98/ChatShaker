using ChatShaker.ChatMauiApp.Models.Dto;

namespace ChatShaker.ChatMauiApp.Models.Local;

public class SelectableUser : BindableBase
{
    public UserItemDto User { get; }

    private bool _isSelected;

    public bool IsSelected
    {
        get {return _isSelected;}
        set { SetProperty(ref _isSelected, value); }
    }

    public SelectableUser(UserItemDto user)
    {
        User = user;
    }
}