using CommunityToolkit.Maui.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.ChatMauiApp.Services.Interfaces
{
    public interface IAppPopupService
    {
        Task ShowError(string message);
        Task ShowSuccess(string message);
        Task ShowImage(string imagePath);
        Task<Popup> ShowLoading(string message);
    }
}
