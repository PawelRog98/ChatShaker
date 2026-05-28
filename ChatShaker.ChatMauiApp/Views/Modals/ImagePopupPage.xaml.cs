using CommunityToolkit.Maui.Views;

namespace ChatShaker.ChatMauiApp.Views.Modals;

public partial class ImagePopupPage : Popup
{
	public ImagePopupPage(string imagePath)
	{
		InitializeComponent();
        FullImage.Source = imagePath;
	}

    private void OnCloseClicked(object sender, EventArgs e)
    {
        Close();
    }
}