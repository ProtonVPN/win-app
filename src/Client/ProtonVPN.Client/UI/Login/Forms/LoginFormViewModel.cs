/*
 * Copyright (c) 2023 Proton AG
 *
 * This file is part of ProtonVPN.
 *
 * ProtonVPN is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ProtonVPN is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with ProtonVPN.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Security;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Auth.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.GuestHole;
using ProtonVPN.Client.Messages;
using ProtonVPN.Client.Models;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Models.Urls;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts.Observers;
using ProtonVPN.Client.UI.Login.Enums;
using ProtonVPN.Client.UI.Login.Overlays;
using ProtonVPN.Client.UI.ReportIssue;
using ProtonVPN.Common.Core.Extensions;
using Windows.System;

namespace ProtonVPN.Client.UI.Login.Forms;

public partial class LoginFormViewModel : PageViewModelBase<ILoginViewNavigator>, IEventMessageReceiver<LoggedInMessage>, IEventMessageReceiver<FeatureFlagsChangedMessage>
{
    private readonly IUrls _urls;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IDialogActivator _dialogActivator;
    private readonly IReportIssueViewNavigator _reportIssueViewNavigator;
    private readonly IFeatureFlagsObserver _featureFlagsObserver;
    private readonly IUIThreadDispatcher _uiThreadDispatcher;
    private readonly IApiAvailabilityVerifier _apiAvailabilityVerifier;
    private readonly IGuestHoleActionExecutor _guestHoleActionExecutor;

    private readonly SsoLoginOverlayViewModel _ssoLoginOverlayViewModel;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    [NotifyPropertyChangedFor(nameof(IsLoginFormEnabled))]
    private bool _isLoggingIn;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    [NotifyPropertyChangedFor(nameof(UsernameFieldLabel))]
    [NotifyPropertyChangedFor(nameof(SwitchPageLabel))]
    [NotifyPropertyChangedFor(nameof(IsPasswordFieldVisible))]
    private LoginFormType _loginFormType;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _username = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _password = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateAccountCommand))]
    private bool _isToShowCreateAccountSpinner;

    public bool IsLoginFormEnabled => !IsLoggingIn;

    public bool IsPasswordFieldVisible => LoginFormType == LoginFormType.SRP;

    public string UsernameFieldLabel => LoginFormType switch
    {
        LoginFormType.SRP => Localizer.Get("Login_Form_Username"),
        LoginFormType.SSO => Localizer.Get("Login_Form_Email"),
        _ => Localizer.Get("Login_Form_Username")
    };

    public string SwitchPageLabel => LoginFormType switch
    {
        LoginFormType.SRP => Localizer.Get("Login_Form_SignInWithSso"),
        LoginFormType.SSO => Localizer.Get("Login_Form_SignInWithPassword"),
        _ => string.Empty
    };

    public override bool IsBackEnabled => false;

    public string CreateAccountUrl => _urls.CreateAccount;

    private bool CanCreateAccount => !IsToShowCreateAccountSpinner;

    public LoginFormViewModel(
        ILoginViewNavigator loginViewNavigator,
        ILocalizationProvider localizationProvider,
        IUrls urls,
        IEventMessageSender eventMessageSender,
        IUserAuthenticator userAuthenticator,
        IApiAvailabilityVerifier apiAvailabilityVerifier,
        IGuestHoleActionExecutor guestHoleActionExecutor,
        IDialogActivator dialogActivator,
        IReportIssueViewNavigator reportIssueViewNavigator,
        IFeatureFlagsObserver featureFlagsObserver,
        IUIThreadDispatcher uiThreadDispatcher,
        SsoLoginOverlayViewModel ssoLoginOverlayViewModel)
        : base(loginViewNavigator, localizationProvider)
    {
        _urls = urls;
        _eventMessageSender = eventMessageSender;
        _userAuthenticator = userAuthenticator;
        _apiAvailabilityVerifier = apiAvailabilityVerifier;
        _guestHoleActionExecutor = guestHoleActionExecutor;
        _dialogActivator = dialogActivator;
        _reportIssueViewNavigator = reportIssueViewNavigator;
        _featureFlagsObserver = featureFlagsObserver;
        _uiThreadDispatcher = uiThreadDispatcher;
        _ssoLoginOverlayViewModel = ssoLoginOverlayViewModel;
    }

    [RelayCommand(CanExecute = nameof(CanLogIn))]
    public async Task LoginAsync()
    {
        try
        {
            IsLoggingIn = true;

            _eventMessageSender.Send(new LoginStateChangedMessage(LoginState.Authenticating));

            AuthResult result = LoginFormType switch
            {
                LoginFormType.SRP => await HandleSrpLoginAsync(),
                LoginFormType.SSO => await HandleSsoLoginAsync(),
                _ => throw new NotSupportedException($"{LoginFormType} login is not supported.")
            };

            if (result.Success)
            {
                await HandleSuccessAsync();
            }
            else
            {
                HandleError(result);
            }
        }
        catch (Exception e)
        {
            _eventMessageSender.Send(new LoginStateChangedMessage(LoginState.Error, AuthError.Unknown, e.Message));
        }
        finally
        {
            IsLoggingIn = false;
        }
    }

    private bool CanLogIn()
    {
        return !IsLoggingIn && LoginFormType switch
        {
            LoginFormType.SRP => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrEmpty(Password),
            LoginFormType.SSO => !string.IsNullOrWhiteSpace(Username) && Username.IsValidEmailAddress() && _featureFlagsObserver.IsSsoEnabled,
            _ => false
        };
    }

    private async Task<AuthResult> HandleSrpLoginAsync()
    {
        SecureString securePassword = Password.ToSecureString();
        Password = string.Empty;

        return await _userAuthenticator.LoginUserAsync(Username, securePassword);
    }

    private async Task<AuthResult> HandleSsoLoginAsync()
    {
        SsoAuthResult result = await _userAuthenticator.StartSsoAuthAsync(Username);

        return result.Success
            ? await _ssoLoginOverlayViewModel.AuthenticateAsync(result.SsoChallengeToken)
            : result;
    }

    private async Task HandleSuccessAsync()
    {
        if (_guestHoleActionExecutor.IsActive())
        {
            await _guestHoleActionExecutor.DisconnectAsync();
        }

        _eventMessageSender.Send(new LoginStateChangedMessage(LoginState.Success));
    }

    private void HandleError(AuthResult result)
    {
        switch (result.Value)
        {
            case AuthError.TwoFactorRequired:
                _eventMessageSender.Send(new LoginStateChangedMessage(LoginState.TwoFactorRequired));
                break;

            case AuthError.SwitchToSSO:
            case AuthError.SwitchToSRP:
                if (CanSwitchLoginForm())
                {
                    SwitchLoginForm();
                }
                goto default;

            default:
                _eventMessageSender.Send(new LoginStateChangedMessage(LoginState.Error, result.Value, result.Error));
                break;
        }
    }

    private bool CanSwitchLoginForm()
    {
        return LoginFormType switch
        {
            LoginFormType.SRP => _featureFlagsObserver.IsSsoEnabled,
            LoginFormType.SSO => true,
            _ => false,
        };
    }

    [RelayCommand(CanExecute = nameof(CanSwitchLoginForm))]
    public void SwitchLoginForm()
    {
        LoginFormType = LoginFormType switch
        {
            LoginFormType.SRP => LoginFormType.SSO,
            LoginFormType.SSO => LoginFormType.SRP,
            _ => LoginFormType.SRP,
        };
    }

    public void Receive(LoggedInMessage message)
    {
        ClearInputs();
    }

    private void ClearInputs()
    {
        Username = string.Empty;
        Password = string.Empty;
    }

    [RelayCommand]
    public void ResetPassword()
    {
        _urls.NavigateTo(_urls.ResetPassword);
    }

    [RelayCommand]
    public void ForgotUsername()
    {
        _urls.NavigateTo(_urls.ForgotUsername);
    }

    [RelayCommand]
    public void TroubleSigningIn()
    {
        _urls.NavigateTo(_urls.TroubleSigningIn);
    }

    [RelayCommand]
    public async Task ReportAnIssueAsync()
    {
        _dialogActivator.ShowDialog<ReportIssueShellViewModel>();

        await _reportIssueViewNavigator.NavigateToCategorySelectionAsync();
    }

    [RelayCommand(CanExecute = nameof(CanCreateAccount))]
    public async Task CreateAccountAsync()
    {
        try
        {
            IsToShowCreateAccountSpinner = true;
            bool isSignUpPageAccessible = await _apiAvailabilityVerifier.IsSignUpPageAccessibleAsync();
            if (isSignUpPageAccessible)
            {
                await OpenCreateAccountPageAsync();
            }
            else
            {
                await _guestHoleActionExecutor.ExecuteAsync(OpenCreateAccountPageAsync);
            }
        }
        finally
        {
            IsToShowCreateAccountSpinner = false;
        }
    }

    private async Task OpenCreateAccountPageAsync()
    {
        await Launcher.LaunchUriAsync(new Uri(_urls.CreateAccount));
    }

    public void Receive(FeatureFlagsChangedMessage message)
    {
        _uiThreadDispatcher.TryEnqueue(() =>
        {
            // If currently on SSO login page but SSO feature flag is not enabled, switch back to SRP login form
            if (!_featureFlagsObserver.IsSsoEnabled && LoginFormType == LoginFormType.SSO)
            {
                SwitchLoginForm();
            }

            LoginCommand.NotifyCanExecuteChanged();
            SwitchLoginFormCommand.NotifyCanExecuteChanged();
        });
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(UsernameFieldLabel));
        OnPropertyChanged(nameof(SwitchPageLabel));
    }
}