using CommunityToolkit.Maui.Views;

namespace ChatShaker.ChatMauiApp.Views.Modals;

public partial class SuccessPopupPage : Popup
{
	public SuccessPopupPage(string message)
	{
		InitializeComponent();
        Message.Text = message;
	}

    private void OnCloseClicked(object sender, EventArgs e)
    {
        Close();
    }
}