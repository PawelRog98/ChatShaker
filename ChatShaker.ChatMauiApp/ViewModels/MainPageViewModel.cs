namespace ChatShaker.ChatMauiApp.ViewModels;

public class MainPageViewModel : BaseViewModel
{
    public DelegateCommand<string> NavigateRegionCommand { get; }

    private readonly IRegionManager _regionManager;

    public MainPageViewModel(IRegionManager regionManager)
    {
        _regionManager = regionManager;

        NavigateRegionCommand = new DelegateCommand<string>(Navigate);
    }

    private void Navigate(string viewName)
    {
        _regionManager.RequestNavigate("MainRegion", viewName);
    }
} 