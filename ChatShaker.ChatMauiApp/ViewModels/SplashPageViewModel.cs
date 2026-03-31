using ChatShaker.ChatMauiApp.Services.Interfaces;
using Prism.Navigation.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.ChatMauiApp.ViewModels
{
    public class SplashPageViewModel : BindableBase
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;
        private readonly IAppPopupService _popupService;
        public SplashPageViewModel(IAuthService authService, INavigationService navigationService, IAppPopupService popupService) 
        { 
            _authService = authService;
            _navigationService = navigationService;
            _popupService = popupService;
        }
        public async Task OnStart()
        {
            try
            {
                var token = await _authService.GetAccessToken();

                if (token != null)
                {
                    // Navigate to MainView and load ChatListPage in the region by default
                    await _navigationService.CreateBuilder()
                        .AddSegment("MainView")
                        .AddSegment("ChatListPage")
                        .NavigateAsync();
                }
                else
                {
                    await _navigationService.NavigateAsync("LoginPage");
                }
            }
            catch (Exception ex) 
            {
                await _popupService.ShowError(ex.Message);
                // In splash we might not want to rethrow if we handled it with a popup, 
                // but usually splash failures are critical.
            }
        }
    }
}