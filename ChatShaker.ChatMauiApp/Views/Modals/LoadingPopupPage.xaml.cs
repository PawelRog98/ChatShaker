using CommunityToolkit.Maui.Views;

namespace ChatShaker.ChatMauiApp.Views.Modals;

public partial class LoadingPopupPage : Popup
{
	public LoadingPopupPage(string message)
	{
		InitializeComponent();
        Message.Text = message;
	}
}