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
    public class RegisterPageViewModel : ValidatableViewModel<RegisterDto, RegisterDtoValidator>
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;
        private readonly IAppPopupService _popupService;   

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        #region View Poperties
        private string _email;
        public string Email
        {
            get { return _email; }
            set { SetProperty(ref _email, value); }
        }

        private string _publicNick;
        public string PublicNick
        {
            get { return _publicNick; }
            set { SetProperty(ref _publicNick, value); }
        }

        private string _firstName;
        public string FirstName
        {
            get { return _firstName; }
            set { SetProperty(ref _firstName, value); }
        }

        private string _lastName;
        public string LastName
        {
            get { return _lastName; }
            set { SetProperty(ref _lastName, value); }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get { return _confirmPassword; }
            set { SetProperty(ref _confirmPassword, value); }
        }

        private DateTime _dateOfBirth;
        public DateTime DateOfBirth
        {
            get { return _dateOfBirth; }
            set { SetProperty(ref _dateOfBirth, value); }
        }
        #endregion

        #region Validation Properties
        private string _emailError;
        public string EmailError
        {
            get { return _emailError; }
            set { SetProperty(ref _emailError, value); }
        }

        private string _publicNickError;
        public string PublicNickError
        {
            get { return _publicNickError; }
            set { SetProperty(ref _publicNickError, value); }
        }

        private string _firstNameError;
        public string FirstNameError
        {
            get { return  _firstNameError; }
            set { SetProperty(ref _firstNameError, value); }
        }

        private string _lastNameError;
        public string LastNameError
        {
            get { return _lastNameError; }
            set { SetProperty(ref _lastNameError, value); }
        }

        private string _passwordError;
        public string PasswordError
        {
            get { return _passwordError; }
            set { SetProperty(ref _passwordError, value); }
        }

        private string _confirmPasswordError;
        public string ConfirmPasswordError
        {
            get { return _confirmPasswordError; }
            set { SetProperty(ref _confirmPasswordError, value); }
        }

        private string _dateofBirthError;
        public string DateOfBirthError
        {
            get { return _dateofBirthError; }
            set { SetProperty(ref _dateofBirthError, value); }
        }
        #endregion
        public DelegateCommand RegisterCommand { get; set; }   
        public RegisterPageViewModel(IAuthService authService, INavigationService navigationService, IAppPopupService popupService)
        {
            _authService = authService;
            _navigationService = navigationService;
            _popupService = popupService;

            RegisterCommand = new DelegateCommand(async () => await Register());
        }

        private async Task Register()
        {

            try
            {
                IsBusy = true;
                var registerDto = new RegisterDto
                {
                    Email = _email,
                    PublicNick = _publicNick,
                    FirstName = _firstName,
                    LastName = _lastName,
                    Password = _password,
                    ConfirmPassword = _confirmPassword,
                    DateOfBirth = _dateOfBirth
                };

                if (!Validate(registerDto))
                {
                    EmailError = GetErrorForProperty(nameof(Email));
                    PublicNickError = GetErrorForProperty(nameof(PublicNick));
                    FirstNameError = GetErrorForProperty(nameof(FirstName));
                    LastNameError = GetErrorForProperty(nameof(LastName));
                    PasswordError = GetErrorForProperty(nameof(Password));
                    ConfirmPasswordError = GetErrorForProperty(nameof(ConfirmPassword));
                    DateOfBirthError = GetErrorForProperty(nameof(DateOfBirth));

                    return;
                }

                var result = await _authService.Register(registerDto);

                if (result.Success)
                    await _navigationService.NavigateAsync("/LoginPage");
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
    }
}
