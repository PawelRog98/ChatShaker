using ChatShaker.ChatMauiApp.Services.Api;
using ChatShaker.ChatMauiApp.Services.Interfaces;
using Prism.Navigation.Regions;
using Prism.Navigation;

namespace ChatShaker.ChatMauiApp.ViewModels
{
    public class SplashPageViewModel : BindableBase, INavigatedAware
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;
        private readonly IAppPopupService _popupService;
        private readonly IUserApiService _userApiService;

        public SplashPageViewModel(IAuthService authService, INavigationService navigationService, IAppPopupService popupService,  IUserApiService userApiService) 
        { 
            _authService = authService;
            _navigationService = navigationService;
            _popupService = popupService;
            _userApiService = userApiService;
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
        }

        public async void OnNavigatedTo(INavigationParameters parameters)
        {
            await OnStart();
        }

        public async Task OnStart()
        {
            try
            {
                //await Task.Delay(1500); 
                var token = await _authService.GetAccessToken();

                if (token != null)
                {
                    await _navigationService.NavigateAsync("/MainView");
                }
                else
                {
                    await _navigationService.NavigateAsync("/LoginPage");
                }
            }
            catch (Exception ex) 
            {
                await _popupService.ShowError(ex.Message);
            }
        }
    }
}