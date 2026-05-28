using CommunityToolkit.Maui.Views;

namespace ChatShaker.ChatMauiApp.Views.Modals;

public partial class ErrorPopupPage : Popup
{
	public ErrorPopupPage(string message)
	{
		InitializeComponent();
        Message.Text = message;
	}

    private void OnCloseClicked(object sender, EventArgs e)
    {
        Close();
    }
}