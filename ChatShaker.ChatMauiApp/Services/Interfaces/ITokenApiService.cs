using ChatShaker.ChatMauiApp.Helpers;

namespace ChatShaker.ChatMauiApp.Services.Interfaces;

public interface ITokenApiService
{
    Task<Response<object>> ActivateAccount(string code, string email);
    Task<Response<object>> CreateNewActivationToken(string email);
}