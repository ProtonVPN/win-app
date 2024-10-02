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
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Auth.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Contracts.Services.Navigation;
using ProtonVPN.Client.UI.Login.Bases;
using ProtonVPN.Client.UI.Login.Enums;
using Windows.System;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.GuestHole;
using ProtonVPN.Common.Legacy.Abstract;
using ProtonVPN.Client.Settings.Contracts.Observers;
using ProtonVPN.Client.Contracts.Messages;
using ProtonVPN.Client.UI.Login.Overlays;
using ProtonVPN.Client.Services.Browsing;
using ProtonVPN.Client.Contracts.Enums;

namespace ProtonVPN.Client.UI.Login.Pages;

public partial class SignInPageViewModel : LoginPageViewModelBase
{
    private readonly IUrls _urls;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IApiAvailabilityVerifier _apiAvailabilityVerifier;
    private readonly IGuestHoleManager _guestHoleManager;
    private readonly IFeatureFlagsObserver _featureFlagsObserver;
    private readonly SsoLoginOverlayViewModel _ssoLoginOverlayViewModel;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SignInCommand))]
    [NotifyPropertyChangedFor(nameof(IsSignInFormEnabled))]
    private bool _isSigningIn;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SignInCommand))]
    [NotifyPropertyChangedFor(nameof(UsernameFieldLabel))]
    [NotifyPropertyChangedFor(nameof(SwitchPageLabel))]
    [NotifyPropertyChangedFor(nameof(IsPasswordFieldVisible))]
    [NotifyPropertyChangedFor(nameof(IsCreateAccountButtonVisible))]
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

    public bool IsSignInFormEnabled => !IsSigningIn;

    public bool IsPasswordFieldVisible => SignInFormType == SignInFormType.SRP;

    public bool IsCreateAccountButtonVisible => SignInFormType == SignInFormType.SRP;

    public string UsernameFieldLabel => SignInFormType switch
    {
        SignInFormType.SRP => Localizer.Get("SignIn_Form_Username"),
        SignInFormType.SSO => Localizer.Get("SignIn_Form_Email"),
        _ => Localizer.Get("SignIn_Form_Username")
    };

    public string SwitchPageLabel => SignInFormType switch
    {
        SignInFormType.SRP => Localizer.Get("SignIn_Form_SignInWithSso"),
        SignInFormType.SSO => Localizer.Get("SignIn_Form_SignInWithPassword"),
        _ => string.Empty
    };

    public string CreateAccountUrl => _urls.CreateAccount;

    private bool CanCreateAccount => !IsToShowCreateAccountSpinner;

    public SignInPageViewModel(
        ILoginViewNavigator parentViewNavigator,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        IUrls urls,
        IUserAuthenticator userAuthenticator,
        IEventMessageSender eventMessageSender,
        IApiAvailabilityVerifier apiAvailabilityVerifier,
        IGuestHoleManager guestHoleManager,
        IFeatureFlagsObserver featureFlagsObserver,
        SsoLoginOverlayViewModel ssoLoginOverlayViewModel)
        : base(parentViewNavigator, localizer, logger, issueReporter)
    {
        _urls = urls;
        _userAuthenticator = userAuthenticator;
        _eventMessageSender = eventMessageSender;
        _apiAvailabilityVerifier = apiAvailabilityVerifier;
        _guestHoleManager = guestHoleManager;
        _featureFlagsObserver = featureFlagsObserver;
        _ssoLoginOverlayViewModel = ssoLoginOverlayViewModel;
    }

    [RelayCommand(CanExecute = nameof(CanSignIn))]
    public async Task SignInAsync()
    {
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

    private bool CanSignIn()
    {
        return !IsSigningIn && SignInFormType switch
        {
            SignInFormType.SRP => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrEmpty(Password),
            SignInFormType.SSO => !string.IsNullOrWhiteSpace(Username) && Username.IsValidEmailAddress() && _featureFlagsObserver.IsSsoEnabled,
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

    private void HandleSuccess()
    {
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
        return SignInFormType switch
        {
            SignInFormType.SRP => _featureFlagsObserver.IsSsoEnabled,
            SignInFormType.SSO => true,
            _ => false,
        };
    }

    [RelayCommand(CanExecute = nameof(CanSwitchLoginForm))]
    public void SwitchLoginForm()
    {
        SignInFormType = SignInFormType switch
        {
            SignInFormType.SRP => SignInFormType.SSO,
            SignInFormType.SSO => SignInFormType.SRP,
            _ => SignInFormType.SRP,
        };
    }

    public void Receive(LoggedInMessage message)
    {
        ExecuteOnUIThread(ClearInputs);
    }

    private void ClearInputs()
    {
        Username = string.Empty;
        Password = string.Empty;
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
        await Launcher.LaunchUriAsync(new Uri(_urls.CreateAccount));
        return Result.Ok();
    }

    public void Receive(FeatureFlagsChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {            
            // If currently on SSO login page but SSO feature flag is not enabled, switch back to SRP login form
            if (!_featureFlagsObserver.IsSsoEnabled && SignInFormType == SignInFormType.SSO)
            {
                SwitchLoginForm();
            }

            SignInCommand.NotifyCanExecuteChanged();
            SwitchLoginFormCommand.NotifyCanExecuteChanged();
        });
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        _userAuthenticator.ClearUnauthSessionDetails();
    }
}