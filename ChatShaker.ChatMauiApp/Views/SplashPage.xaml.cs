using ChatShaker.ChatMauiApp.ViewModels;
using System.Threading.Tasks;

namespace ChatShaker.ChatMauiApp.Views;

public partial class SplashPage : ContentPage
{

    private bool _hasLoaded = false;
	public SplashPage()
	{
		InitializeComponent();
	}
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_hasLoaded)
            return;
        _hasLoaded = true;

        var viewModel = BindingContext as SplashPageViewModel;
        if (viewModel != null)
            await viewModel.OnStart();
    }
}