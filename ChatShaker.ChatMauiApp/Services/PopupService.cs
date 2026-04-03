using ChatShaker.ChatMauiApp.Services.Interfaces;
using ChatShaker.ChatMauiApp.Views.Modals;
using CommunityToolkit.Maui.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.ChatMauiApp.Services
{
    public class AppPopupService : IAppPopupService
    {
        public async Task ShowError(string message)
        {
            var popup = new ErrorPopupPage(message);

            await Application.Current.MainPage.ShowPopupAsync(popup);
        }

        public async Task ShowSuccess(string message)
        {
            var popup = new SuccessPopupPage(message);

            await Application.Current.MainPage.ShowPopupAsync(popup);
        }

        public async Task<Popup> ShowLoading(string message)
        {
            var popup = new LoadingPopupPage(message);

            _ = Application.Current.MainPage.ShowPopupAsync(popup);
            return popup;
        }
    }
}
