using ChatShaker.ChatMauiApp.Services;
using ChatShaker.ChatMauiApp.Services.Interfaces;
using ChatShaker.ChatMauiApp.ViewModels;
using ChatShaker.ChatMauiApp.Views;
using CommunityToolkit.Maui;
using DryIoc;
using Microsoft.Extensions.Logging;

namespace ChatShaker.ChatMauiApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UsePrism(prism =>
                {
                    prism.RegisterTypes(container =>
                    {
                        container.Register<IApiService, ApiService>();
                        container.Register<IAuthTokenProvider, AuthTokenProvider>();
                        container.Register<IAuthService, AuthService>();
                        container.Register<IAppPopupService,  AppPopupService>();

                        container.RegisterForNavigation<SplashPage, SplashPageViewModel>();
                        container.RegisterForNavigation<LoginPage, LoginPageViewModel>();
                        container.RegisterForNavigation<RegisterPage,  RegisterPageViewModel>();
                        container.RegisterForNavigation<MainPage>();

                    });

                    prism.ConfigureModuleCatalog(module =>
                    {

                    });

                    prism.CreateWindow(async nav =>
                    {
                        await nav.CreateBuilder()
                        .AddSegment("SplashPage")
                        .NavigateAsync();
                    });

                })
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            builder.Services.AddHttpClient("ShakerApiClient", client =>
            {
                //client.BaseAddress = new Uri("https://www.chat-shaker.io");
                client.BaseAddress = new Uri("https://puny-crabs-begin.loca.lt");
            });

            return builder.Build();
        }
    }
}
