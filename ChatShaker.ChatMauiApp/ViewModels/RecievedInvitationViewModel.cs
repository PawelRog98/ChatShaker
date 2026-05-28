using System.Collections.ObjectModel;
using ChatShaker.ChatMauiApp.Models.Dto;
using ChatShaker.ChatMauiApp.Services.Interfaces;
using Prism.Commands;
using Prism.Navigation;
using Prism.Navigation.Regions;

namespace ChatShaker.ChatMauiApp.ViewModels;

public class RecievedInvitationViewModel : BaseViewModel, IRegionAware
{
    private readonly IFriendshipApiService _friendshipApiService;
    private readonly IAppPopupService _popupService;

    public ObservableCollection<InvitationDto> RecievedInvitations { get; } = new();

    public DelegateCommand<InvitationDto> AcceptCommand { get; }
    public DelegateCommand<InvitationDto> RejectCommand { get; }

    public RecievedInvitationViewModel(IFriendshipApiService friendshipApiService, IAppPopupService popupService)
    {
        _friendshipApiService = friendshipApiService;
        _popupService = popupService;

        AcceptCommand = new DelegateCommand<InvitationDto>(async (invitation) => await Respond(invitation, true));
        RejectCommand = new DelegateCommand<InvitationDto>(async (invitation) => await Respond(invitation, false));
    }

    private async Task Respond(InvitationDto invitation, bool accept)
    {
        if (invitation == null) return;

        IsBusy = true;
        try
        {
            var response = await _friendshipApiService.RespondToInvitation(invitation.PublicId, accept);
            if (response.Success)
            {
                RecievedInvitations.Remove(invitation);
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

    private async Task LoadRecievedInvitations()
    {
        IsBusy = true;
        try
        {
            var response = await _friendshipApiService.GetRecievedInvitations();

            if (!response.Success)
            {
                await _popupService.ShowError(response.Message);
                return;
            }

            RecievedInvitations.Clear();
            foreach (var invitation in response.Data)
            {
                RecievedInvitations.Add(invitation);
            }
        }
        catch(Exception ex)
        {
            await _popupService.ShowError(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
    }

    public async void OnNavigatedTo(NavigationContext navigationContext)
    {
        await LoadRecievedInvitations();
    }
}
