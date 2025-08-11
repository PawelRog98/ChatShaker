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
    }
}
