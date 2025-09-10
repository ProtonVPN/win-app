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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
using ProtonVPN.Client.UI.Login.Bases;

namespace ProtonVPN.Client.UI.Login.Pages;

public partial class TwoFactorPageViewModel : LoginPageViewModelBase
{
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IGuestHoleManager _guestHoleManager;
    private readonly IUrlsBrowser _urlsBrowser;

    [ObservableProperty]
    private bool _isToShowError;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AuthenticateWithCodeCommand))]
    [NotifyCanExecuteChangedFor(nameof(AuthenticateWithSecurityKeyCommand))]
    private bool _isAuthenticating;

    [ObservableProperty]
    private bool _isAuthenticatorModeSelected;

    [ObservableProperty]
    private bool _isSecurityKeyModeSelected;

    public bool CanSwitchTwoFactorMode => _userAuthenticator.IsTwoFactorAuthenticatorModeEnabled
                                       && _userAuthenticator.IsTwoFactorSecurityKeyModeEnabled;

    public string LearnMoreUrl => _urlsBrowser.TwoFactorAuthLearnMore;

    public TwoFactorPageViewModel(
        ILoginViewNavigator parentViewNavigator,
        IEventMessageSender eventMessageSender,
        IUserAuthenticator userAuthenticator,
        IGuestHoleManager guestHoleManager,
        IUrlsBrowser urlsBrowser,
        IViewModelHelper viewModelHelper)
        : base(parentViewNavigator, viewModelHelper)
    {
        _eventMessageSender = eventMessageSender;
        _userAuthenticator = userAuthenticator;
        _guestHoleManager = guestHoleManager;
        _urlsBrowser = urlsBrowser;
    }

    public event EventHandler? OnTwoFactorFailure;

    public event EventHandler? OnTwoFactorSuccess;

    [RelayCommand(CanExecute = nameof(CanAuthenticate))]
    public async Task AuthenticateWithCodeAsync(string twoFactorCode)
    {
        if (twoFactorCode is not { Length: 6 })
        {
            IsToShowError = true;
            return;
        }

        IsToShowError = false;

        try
        {
            IsAuthenticating = true;

            _eventMessageSender.Send(new LoginStateChangedMessage(LoginState.Authenticating));

            AuthResult result = await _userAuthenticator.SendTwoFactorCodeAsync(twoFactorCode);

            await HandleAuthResultAsync(result);
        }
        catch (Exception ex)
        {
            _eventMessageSender.Send(new LoginStateChangedMessage(LoginState.TwoFactorFailed, AuthError.Unknown, ex.Message));
        }
        finally
        {
            IsAuthenticating = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanAuthenticate))]
    public async Task AuthenticateWithSecurityKeyAsync()
    {
        try
        {
            IsAuthenticating = true;

            _eventMessageSender.Send(new LoginStateChangedMessage(LoginState.Authenticating));

            AuthResult result = await _userAuthenticator.AuthenticateWithSecurityKeyAsync();

            await HandleAuthResultAsync(result);
        }
        catch (Exception ex)
        {
            _eventMessageSender.Send(new LoginStateChangedMessage(LoginState.TwoFactorFailed, AuthError.Unknown, ex.Message));
        }
        finally
        {
            IsAuthenticating = false;
        }
    }

    public void Receive(LoginStateChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            if (message.Value == LoginState.TwoFactorFailed)
            {
                OnTwoFactorFailure?.Invoke(this, EventArgs.Empty);
            }
        });
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        InvalidateTwoFactorMode();
    }

    protected override void OnDeactivated()
    {
        IsToShowError = false;
    }

    private async Task HandleAuthResultAsync(AuthResult result)
    {
        if (result.Success)
        {
            if (_guestHoleManager.IsActive)
            {
                await _guestHoleManager.DisconnectAsync();
            }

            _eventMessageSender.Send(new LoginStateChangedMessage(LoginState.Success));
            OnTwoFactorSuccess?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            _eventMessageSender.Send(result.Value == AuthError.TwoFactorCancelled
                ? new LoginStateChangedMessage(LoginState.TwoFactorCancelled)
                : new LoginStateChangedMessage(LoginState.TwoFactorFailed, result.Value));
        }
    }

    private bool CanAuthenticate()
    {
        return !IsAuthenticating;
    }

    [RelayCommand]
    private async Task<bool> GoBackAsync()
    {
        if (_guestHoleManager.IsActive)
        {
            await _guestHoleManager.DisconnectAsync();
        }

        return await ParentViewNavigator.NavigateToSignInViewAsync();
    }

    private void InvalidateTwoFactorMode()
    {
        OnPropertyChanged(nameof(CanSwitchTwoFactorMode));

        if (CanSwitchTwoFactorMode && (IsAuthenticatorModeSelected || IsSecurityKeyModeSelected))
        {
            // If both modes are available and one of them is already selected, do nothing.
            return;
        }

        if (_userAuthenticator.IsTwoFactorSecurityKeyModeEnabled)
        {
            IsSecurityKeyModeSelected = true;
            IsAuthenticatorModeSelected = false;
        }
        else if (_userAuthenticator.IsTwoFactorAuthenticatorModeEnabled)
        {
            IsAuthenticatorModeSelected = true;
            IsSecurityKeyModeSelected = false;
        }
        else
        {
            _eventMessageSender.Send(new LoginStateChangedMessage(LoginState.TwoFactorFailed, AuthError.Unknown, Localizer.Get("Login_TwoFactor_NoMethodAvailable")));
            GoBackCommand.Execute(null);
        }
    }
}