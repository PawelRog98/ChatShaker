using ChatShaker.ChatMauiApp.Models.Dto;
using ChatShaker.ChatMauiApp.Models.Validators;
using ChatShaker.ChatMauiApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.ChatMauiApp.ViewModels
{
    public class LoginPageViewModel : ValidatableViewModel<LoginDto, LoginDtoValidator>, IPageLifecycleAware
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;
        private readonly IAppPopupService _popupService;

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

        public LoginPageViewModel(IAuthService authService, INavigationService navigationService, IAppPopupService popupService)
        {
            _authService = authService;
            _navigationService = navigationService;
            _popupService = popupService;

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
                    await _navigationService.NavigateAsync("/MainPage");
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
