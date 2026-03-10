using ChatShaker.ChatMauiApp.Handlers;
using ChatShaker.ChatMauiApp.Services;
using ChatShaker.ChatMauiApp.Services.Api;
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
                        container.Register<IKeyApiService, KeyApiService>();
                        container.Register<IMessagesApiService, MessagesApiService>();
                        container.Register<IRoomApiService, RoomsApiService>();
                        
                        container.Register<IAuthTokenProvider, AuthTokenProvider>();
                        container.Register<IAuthService, AuthService>();
                        container.Register<ICryptoService, CryptoService>();
                        container.Register<IAppPopupService,  AppPopupService>();
                        container.RegisterSingleton<ISignalRConnectionManager, SignalRConnectionManager>();
                        container.RegisterSingleton<IChatConnectionService, ChatConnectionService>();
                        container.Register<IGlobalConnectionService, GlobalConnectionService>();
                        container.Register<IRoomKeyService, RoomKeyService>();

                        container.RegisterForNavigation<SplashPage, SplashPageViewModel>();
                        container.RegisterForNavigation<LoginPage, LoginPageViewModel>();
                        container.RegisterForNavigation<RegisterPage,  RegisterPageViewModel>();
                        container.RegisterForNavigation<ChatListPage, ChatListViewModel>();
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
                client.BaseAddress = new Uri("http://10.0.2.2:8080");
            })
            .AddHttpMessageHandler<AuthHeaderHandler>();

            return builder.Build();
        }
    }
}
