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

                Application.Current.MainPage = new AppShell();
                if (token != null)
                    await _navigationService.NavigateAsync("MainPage");
                else
                    await _navigationService.NavigateAsync("LoginPage");
            }
            catch (Exception ex) 
            {
                await _popupService.ShowError(ex.Message);
                throw;
            }
        }
    }
}
