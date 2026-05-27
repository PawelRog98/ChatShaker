using ChatShaker.ChatMauiApp.Models.Dto;
using ChatShaker.ChatMauiApp.Models.Validators;
using ChatShaker.ChatMauiApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatShaker.ChatMauiApp.Services.Api;

namespace ChatShaker.ChatMauiApp.ViewModels
{
    public class LoginPageViewModel : ValidatableViewModel<LoginDto, LoginDtoValidator>, IPageLifecycleAware
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;
        private readonly IAppPopupService _popupService;
        private readonly ICryptoService _cryptoService;
        private readonly IUserApiService _userApiService;

        #region Properties
        private string _email;
        public string Email
        {
            get { return _email; }
            set { SetProperty(ref _email, value); }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }
        #endregion

        #region Validation
        private string _emailError;
        public string EmailError
        {
            get { return _emailError; }
            set { SetProperty(ref _emailError, value); }
        }

        private string _passwordError;
        public string PasswordError
        {
            get { return _passwordError; }
            set { SetProperty(ref _passwordError, value); }
        }
        #endregion
        public DelegateCommand LoginCommand { get; set; }
        public DelegateCommand MoveToRegisterCommand { get; set; }

        public LoginPageViewModel(IAuthService authService, INavigationService navigationService, IAppPopupService popupService, ICryptoService cryptoService, IUserApiService userApiService)
        {
            _authService = authService;
            _navigationService = navigationService;
            _popupService = popupService;
            _cryptoService = cryptoService;
            _userApiService = userApiService;

            LoginCommand = new DelegateCommand(async () => await  Login());
            MoveToRegisterCommand = new DelegateCommand(async () => await MoveToRegister());
        }

        private async Task Login()
        {
            try
            {
                IsBusy = true;

                var loginDto = new LoginDto
                {
                    Email = _email,
                    Password = _password
                };

                if (!Validate(loginDto))
                {
                    EmailError = GetErrorForProperty(nameof(Email));
                    PasswordError = GetErrorForProperty(nameof(Password));

                    return;
                }

                var result = await _authService.Login(loginDto);
                
                if (result.Success)
                {
                    var clearOtherDevices = false;
                    var userIdentities = await _userApiService.GetParticipants(new List<Guid> { Guid.Parse(result.Data.UserId) });
                    
                    if (userIdentities.Success && userIdentities.Data.Any())
                    {
                        var hasLocalKey = await SecureStorage.GetAsync("identity_private_key_" + result.Data.UserId) != null;
                        if (!hasLocalKey)
                        {
                            clearOtherDevices = await Application.Current.MainPage.DisplayAlert(
                                "New Device Detected", 
                                "You have other registered devices. Do you want to clear them and make this your only active device? (Recommended if you reset your device)", 
                                "Clear Others", 
                                "Keep All");
                        }
                    }

                    await _cryptoService.SaveIdentityKey(result.Data.UserId, clearOtherDevices);
                    var navResult = await _navigationService.NavigateAsync("/MainView");
                    if (!navResult.Success)
                    {
                        await _popupService.ShowError($"Navigation failed: {navResult.Exception?.Message}");
                    }
                }
                else if (result.Errors.Contains("Email is not confirmed."))
                {
                    var parameters = new NavigationParameters
                    {
                        { "Email", Email }
                    };
                    var navResult = await _navigationService.NavigateAsync("/ConfirmationAccountPage", parameters);
                    if (!navResult.Success)
                    {
                        await _popupService.ShowError($"Navigation failed: {navResult.Exception?.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                await _popupService.ShowError(ex.Message);
            }
            finally 
            { 
                IsBusy = false; 
            }

        }

        private async Task MoveToRegister()
        {
            await _navigationService.NavigateAsync("/RegisterPage");
        }

        public void OnAppearing()
        {
        }

        public void OnDisappearing()
        {
        }
    }
}
