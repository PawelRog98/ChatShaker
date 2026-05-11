using System.Collections.ObjectModel;
using ChatShaker.ChatMauiApp.Models.Dto;
using ChatShaker.ChatMauiApp.Services.Interfaces;
using Prism.Commands;
using Prism.Navigation.Regions;

namespace ChatShaker.ChatMauiApp.ViewModels;

public class SendInvitationViewModel : BaseViewModel, IRegionAware
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

    public ObservableCollection<InvitationDto> SentInvitations { get; } = new();
    public DelegateCommand SendInvitationCommand { get; }

    public SendInvitationViewModel(IFriendshipApiService friendshipApiService,  IAppPopupService popupService)
    {
        _friendshipApiService = friendshipApiService;
        _popupService = popupService;
        
        SendInvitationCommand = new DelegateCommand(SendInvitation);
    }

    private async void SendInvitation()
    {
        if (string.IsNullOrWhiteSpace(OngoingText))
            return;

        IsBusy = true;
        try
        {
            var response = await _friendshipApiService.SendInvitation(_ongoingText);
            if (response.Success)
            {
                OngoingText = string.Empty;
                await LoadSentInvitations();
            }
            else
            {
                await _popupService.ShowError(response.Message);
            }
        }
        catch (Exception ex)
        {
            await _popupService.ShowError(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadSentInvitations()
    {
        IsBusy = true;
        try
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
        catch (Exception ex)
        {
            await _popupService.ShowError(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
        
    }

    public async void OnNavigatedTo(NavigationContext navigationContext)
    {
        await LoadSentInvitations();
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
}