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
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var popup = new ErrorPopupPage(message);
                await Application.Current.MainPage.ShowPopupAsync(popup);
            });
        }

        public async Task ShowSuccess(string message)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var popup = new SuccessPopupPage(message);
                await Application.Current.MainPage.ShowPopupAsync(popup);
            });
        }

        public async Task<Popup> ShowLoading(string message)
        {
            return await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var popup = new LoadingPopupPage(message);
                await Application.Current.MainPage.ShowPopupAsync(popup);
                return popup;
            });
        }
    }
}
