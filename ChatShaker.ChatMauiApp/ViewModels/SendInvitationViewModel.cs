using System.Collections.ObjectModel;
using ChatShaker.ChatMauiApp.Models.Dto;
using ChatShaker.ChatMauiApp.Services.Interfaces;

namespace ChatShaker.ChatMauiApp.ViewModels;

public class SendInvitationViewModel : BaseViewModel, INavigationAware
{
    #region Properties

    private string _ongoingText;

    public string OngoingText
    {
        get { return _ongoingText; }
        set { SetProperty(ref _ongoingText, value); }
    }
    #endregion
    
    private readonly IFriendshipApiService _friendshipApiService;
    private readonly IAppPopupService _popupService;

    public ObservableCollection<SentInvitationDto> SentInvitations { get; } = new();
    public DelegateCommand SendInvitationCommand { get; }

    public SendInvitationViewModel(IFriendshipApiService friendshipApiService,  IAppPopupService popupService)
    {
        _friendshipApiService = friendshipApiService;
        _popupService = popupService;
        
        SendInvitationCommand = new DelegateCommand(SendInvitation);
    }

    private async void SendInvitation()
    {
        if(string.IsNullOrWhiteSpace(OngoingText))
            return;
        
        await _friendshipApiService.SendInvitation(_ongoingText);
    }

    private async Task LoadSentInvitations()
    {
        var response = await _friendshipApiService.GetSentInvitations();

        if (!response.Success)
        {
            await _popupService.ShowError(response.Message);
            return;
        }
        
        var list = response.Data;
        SentInvitations.Clear();
        
        foreach (var invitation in list)
            SentInvitations.Add(invitation);
            
    }

    public void OnNavigatedFrom(INavigationParameters parameters)
    {
        
    }

    public async void OnNavigatedTo(INavigationParameters parameters)
    {
        await LoadSentInvitations();
    }
}