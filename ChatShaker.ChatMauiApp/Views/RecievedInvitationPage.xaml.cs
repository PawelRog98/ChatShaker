using Prism.Navigation.Regions;

namespace ChatShaker.ChatMauiApp.Views;

public partial class RecievedInvitationPage : ContentView, IRegionMemberLifetime
{
    public RecievedInvitationPage()
    {
        InitializeComponent();
    }

    public bool KeepAlive => false;
}