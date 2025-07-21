/*
 * Copyright (c) 2025 Proton AG
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
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Core.Messages;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Auth.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.GuestHole;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Login.Bases;
using ProtonVPN.Client.UI.Login.Enums;
using ProtonVPN.Client.UI.Login.Overlays;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Legacy.Abstract;
using Windows.System;

namespace ProtonVPN.Client.UI.Login.Pages;

public partial class SignInPageViewModel : LoginPageViewModelBase
{
    private readonly IUrlsBrowser _urlsBrowser;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IApiAvailabilityVerifier _apiAvailabilityVerifier;
    private readonly IGuestHoleManager _guestHoleManager;
    private readonly ISessionSettings _sessionSettings;
    private readonly SsoLoginOverlayViewModel _ssoLoginOverlayViewModel;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SignInCommand))]
    [NotifyPropertyChangedFor(nameof(IsSignInFormEnabled))]
    private bool _isSigningIn;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SignInCommand))]
    [NotifyPropertyChangedFor(nameof(UsernameFieldLabel))]
    [NotifyPropertyChangedFor(nameof(SwitchPageLabel))]
    [NotifyPropertyChangedFor(nameof(IsSrpFormType))]
    [NotifyPropertyChangedFor(nameof(IsSsoFormType))]
    [NotifyPropertyChangedFor(nameof(IsSwitchFormButtonVisible))]
    private SignInFormType _signInFormType;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SignInCommand))]
    private string _username = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SignInCommand))]
    private string _password = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateAccountCommand))]
    private bool _isToShowCreateAccountSpinner;

    [ObservableProperty]
    private bool _isToShowUsernameError;

    [ObservableProperty]
    private bool _isToShowPasswordError;

    private AuthError _authError;

    public bool IsSignInFormEnabled => !IsSigningIn;

    public bool IsSrpFormType => SignInFormType == SignInFormType.SRP;

    public bool IsSsoFormType => SignInFormType == SignInFormType.SSO;

    public bool IsSwitchFormButtonVisible => SignInFormType == SignInFormType.SRP;

    public string UsernameFieldLabel => SignInFormType switch
    {
        SignInFormType.SRP => Localizer.Get("SignIn_Form_Username"),
        SignInFormType.SSO => Localizer.Get("SignIn_Form_Email"),
        _ => Localizer.Get("SignIn_Form_Username")
    };

    public string SwitchPageLabel => SignInFormType switch
    {
        SignInFormType.SRP => Localizer.Get("SignIn_Form_SignInWithSso"),
        _ => string.Empty
    };

    public string CreateAccountUrl => _urlsBrowser.CreateAccount;

    public AuthError AuthError => _authError;

    private bool CanCreateAccount => !IsToShowCreateAccountSpinner;

    public SignInPageViewModel(
        ILoginViewNavigator parentViewNavigator,
        IUrlsBrowser urlsBrowser,
        IUserAuthenticator userAuthenticator,
        IEventMessageSender eventMessageSender,
        IApiAvailabilityVerifier apiAvailabilityVerifier,
        IGuestHoleManager guestHoleManager,
        ISessionSettings sessionSettings,
        SsoLoginOverlayViewModel ssoLoginOverlayViewModel,
        IViewModelHelper viewModelHelper)
        : base(parentViewNavigator, viewModelHelper)
    {
        _urlsBrowser = urlsBrowser;
        _userAuthenticator = userAuthenticator;
        _eventMessageSender = eventMessageSender;
        _apiAvailabilityVerifier = apiAvailabilityVerifier;
        _guestHoleManager = guestHoleManager;
        _sessionSettings = sessionSettings;
        _ssoLoginOverlayViewModel = ssoLoginOverlayViewModel;
    }

    [RelayCommand(CanExecute = nameof(CanSignIn))]
    public async Task SignInAsync()
    {
        if (!ValidateForm())
        {
            return;
        }

        try
        {
            IsSigningIn = true;

            _eventMessageSender.Send(new LoginStateChangedMessage(LoginState.Authenticating));

            AuthResult result = SignInFormType switch
            {
                SignInFormType.SRP => await HandleSrpLoginAsync(),
                SignInFormType.SSO => await HandleSsoLoginAsync(),
                _ => throw new NotSupportedException($"{SignInFormType} login is not supported.")
            };

            if (result.Success)
            {
                HandleSuccess();
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
            IsSigningIn = false;
        }
    }

    private bool ValidateForm()
    {
        switch (SignInFormType)
        {
            case SignInFormType.SRP:
                IsToShowUsernameError = string.IsNullOrWhiteSpace(Username);
                IsToShowPasswordError = string.IsNullOrWhiteSpace(Password);
                break;
            case SignInFormType.SSO:
                Username = Username.Trim();
                IsToShowUsernameError = string.IsNullOrWhiteSpace(Username) || !Username.IsValidEmailAddress();
                break;
        }

        return !IsToShowUsernameError && !IsToShowPasswordError;
    }

    private bool CanSignIn()
    {
        return !IsSigningIn;
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

    private void HandleSuccess()
    {
        _authError = AuthError.None;
        _eventMessageSender.Send(new LoginStateChangedMessage(LoginState.Success));
    }

    private void HandleError(AuthResult result)
    {
        _authError = result.Value;

        switch (result.Value)
        {
            case AuthError.TwoFactorRequired:
                _eventMessageSender.Send(new LoginStateChangedMessage(LoginState.TwoFactorRequired));
                break;

            case AuthError.SwitchToSSO:
            case AuthError.SwitchToSRP:
                SwitchLoginForm();
                goto default;

            default:
                _eventMessageSender.Send(new LoginStateChangedMessage(LoginState.Error, result.Value, result.Error));
                break;
        }
    }

    [RelayCommand]
    public void SwitchLoginForm()
    {
        SignInFormType = SignInFormType switch
        {
            SignInFormType.SRP => SignInFormType.SSO,
            SignInFormType.SSO => SignInFormType.SRP,
            _ => SignInFormType.SRP,
        };
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
                await _guestHoleManager.ExecuteAsync<Result>(OpenCreateAccountPageAsync);
            }
        }
        finally
        {
            IsToShowCreateAccountSpinner = false;
        }
    }

    private async Task<Result> OpenCreateAccountPageAsync()
    {
        await Launcher.LaunchUriAsync(new Uri(_urlsBrowser.CreateAccount));
        return Result.Ok();
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        if (_userAuthenticator.IsAutoLogin != true && SignInCommand.CanExecute(null))
        {
            if (!string.IsNullOrEmpty(_sessionSettings.Username))
            {
                Username = _sessionSettings.Username.Trim();

                if (!string.IsNullOrEmpty(_sessionSettings.Password)) 
                {
                    Password = _sessionSettings.Password.Trim();

                    SignInCommand.Execute(null);
                }
            }
        }
    }

    partial void OnSignInFormTypeChanged(SignInFormType oldValue, SignInFormType newValue)
    {
        IsToShowUsernameError = false;
        IsToShowPasswordError = false;
    }
}