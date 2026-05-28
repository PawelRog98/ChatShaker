using ChatShaker.ChatMauiApp.Services.Interfaces;

namespace ChatShaker.ChatMauiApp.ViewModels;

public class ActivationAccountViewModel : BaseViewModel, INavigationAware
{
    private readonly ITokenApiService _tokenApiService;
    private readonly IAppPopupService _appPopupService;
    private readonly INavigationService _navigationService;
    
    
    private string _code;
    public string Code
    {
        get { return _code; }
        set { SetProperty(ref _code, value); }
    }
    
    private bool _canResendCode = true;
    public bool CanResendCode
    {
        get { return _canResendCode; }
        set { SetProperty(ref _canResendCode, value); }
    }

    private int _cooldownSeconds;
    public int CooldownSeconds
    {
        get { return _cooldownSeconds; }
        set { SetProperty(ref _cooldownSeconds, value); }
    }

    private bool _isCooldownActive;
    public bool IsCooldownActive
    {
        get { return _isCooldownActive; }
        set { SetProperty(ref _isCooldownActive, value); }
    }
    
    private string _email;

    public DelegateCommand ActivateAccountCommand { get; set; }
    public DelegateCommand ResetCodeCommand { get; set; }

    public ActivationAccountViewModel(ITokenApiService tokenApiService,  IAppPopupService appPopupService, INavigationService navigationService)
    {
        _tokenApiService = tokenApiService;
        _appPopupService = appPopupService;
        _navigationService = navigationService;

        ActivateAccountCommand = new DelegateCommand(async () => await ActivateAccount(), () => !IsBusy).ObservesProperty(() => IsBusy);
        ResetCodeCommand = new DelegateCommand(async () => await CreateNewToken(), () => !IsBusy && CanResendCode)
            .ObservesProperty(() => IsBusy)
            .ObservesProperty(() => CanResendCode);
    }

    private async Task ActivateAccount()
    {
        try
        {
            IsBusy = true;
            
            var result = await _tokenApiService.ActivateAccount(Code, _email);

            if (result.Success)
            {
                var navResult = await _navigationService.NavigateAsync("/MainView");
                if (!navResult.Success)
                {
                    await _appPopupService.ShowError($"Navigation failed: {navResult.Exception?.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            await _appPopupService.ShowError(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CreateNewToken()
    {
        try
        {
            IsBusy = true;

            var result = await _tokenApiService.CreateNewActivationToken(_email);
            if (result.Success)
            {
                _ = StartCooldown();
            }
        }
        catch (Exception ex)
        {
            await _appPopupService.ShowError(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task StartCooldown()
    {
        CanResendCode = false;
        IsCooldownActive = true;
        CooldownSeconds = 60;
        
        while (CooldownSeconds > 0)
        {
            await Task.Delay(1000);
            CooldownSeconds--;
        }

        IsCooldownActive = false;
        CanResendCode = true;
    }

    public void OnNavigatedFrom(INavigationParameters parameters)
    {
        
    }

    public void OnNavigatedTo(INavigationParameters parameters)
    {
        IsBusy =  true;
        try
        {
            _email = parameters.GetValue<string>("Email");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
        finally
        {
            IsBusy = false;
        }
    }
}