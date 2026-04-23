using ChatShaker.ChatMauiApp.Services.Interfaces;
using Prism.Commands;
using Prism.Navigation;
using Prism.Navigation.Regions;

namespace ChatShaker.ChatMauiApp.ViewModels;
public class MainPageViewModel : BaseViewModel, INavigationAware
{
    private readonly IRegionManager _regionManager;
    private readonly INavigationService _navigationService;
    private readonly IAuthTokenProvider _authTokenProvider;
    
    public DelegateCommand<string> NavigateRegionCommand { get; }
    public DelegateCommand LogoutCommand { get; }

    private string _currentView;
    public string CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    public MainPageViewModel(IRegionManager regionManager, INavigationService navigationService, IAuthTokenProvider authTokenProvider)
    {
        _regionManager = regionManager;
        _navigationService = navigationService;
        _authTokenProvider = authTokenProvider;

        NavigateRegionCommand = new DelegateCommand<string>(Navigate);
        LogoutCommand = new DelegateCommand(async () => await Logout());
    }

    private void Navigate(string viewName)
    {
        _regionManager.RequestNavigate("MainRegion", viewName, navigationResult =>
        {
            if (navigationResult.Success == false && navigationResult.Exception != null)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation failed: {navigationResult.Exception.Message}");
            }
            else
            {
                CurrentView = viewName;
            }
        });
    }

    private async Task Logout()
    {
        await _authTokenProvider.ClearTokens();
        await _navigationService.NavigateAsync("LoginPage");
    }

    public void OnNavigatedFrom(INavigationParameters parameters)
    {
    }

    public void OnNavigatedTo(INavigationParameters parameters)
    {
        if (string.IsNullOrEmpty(CurrentView))
        {
            Navigate("ChatListPage");
        }
    }
}