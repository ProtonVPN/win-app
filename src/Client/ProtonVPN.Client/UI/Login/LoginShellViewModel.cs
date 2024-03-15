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

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Navigation;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Messages;
using ProtonVPN.Client.Models;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.Logging.Contracts.Events.GuestHoleLogs;

namespace ProtonVPN.Client.UI.Login;

public partial class LoginShellViewModel : ShellViewModelBase<ILoginViewNavigator>, IEventMessageReceiver<LoginStateChangedMessage>, IEventMessageReceiver<LoggedOutMessage>
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public override bool IsBackEnabled => CurrentPage?.IsBackEnabled ?? false;

    public LoginShellViewModel(
        ILoginViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(viewNavigator, localizationProvider, logger, issueReporter)
    {
        _errorMessage = string.Empty;
    }

    public async void Receive(LoginStateChangedMessage message)
    {
        switch (message.Value)
        {
            case LoginState.Authenticating:
                ClearErrorMessage();
                break;

            case LoginState.Success:
                ClearErrorMessage();
                await ViewNavigator.NavigateToLoginAsync();
                break;

            case LoginState.TwoFactorRequired:
                ClearErrorMessage();
                await ViewNavigator.NavigateToTwoFactorAsync();
                break;

            case LoginState.TwoFactorFailed:
                switch (message.AuthError)
                {
                    case AuthError.IncorrectTwoFactorCode:
                        ShowErrorMessage(Localizer.Get("Login_Error_IncorrectTwoFactorCode"));
                        break;

                    case AuthError.TwoFactorAuthFailed:
                        ShowErrorMessage(Localizer.Get("Login_Error_TwoFactorFailed"));
                        await ViewNavigator.NavigateToLoginAsync();
                        break;

                    case AuthError.Unknown:
                        ShowErrorMessage(message.ErrorMessage);
                        break;
                }
                break;

            case LoginState.Error:
                HandleAuthError(message);
                break;
        }
    }

    public void Receive(LoggedOutMessage message)
    {
        switch (message.Reason)
        {
            case LogoutReason.UserAction:
            case LogoutReason.NoVpnConnectionsAssigned:
                break;
            case LogoutReason.SessionExpired:
                ExecuteOnUIThread(() => ShowErrorMessage(Localizer.Get("Login_Error_SessionExpired")));
                break;
        }
    }

    protected override void OnNavigated()
    {
        base.OnNavigated();

        OnPropertyChanged(nameof(IsBackEnabled));
    }

    private void HandleAuthError(LoginStateChangedMessage message)
    {
        switch (message.AuthError)
        {
            case AuthError.MissingGoSrpDll:
                Logger.Fatal<AppCrashLog>("The app is missing GoSrp.dll");
                // VPNWIN-2109 - Add modal about missing file
                ViewNavigator.CloseCurrentWindow();
                break;

            case AuthError.GuestHoleFailed:
                // VPNWIN-1982 - Show troubleshooting dialog
                Logger.Error<GuestHoleLog>("Failed to authenticate using guest hole.");
                break;

            case AuthError.SsoAuthFailed:
                ShowErrorMessage(Localizer.Get("Login_Error_SsoAuthFailed"));
                break;

            case AuthError.GetSessionDetailsFailed:
                ShowErrorMessage(Localizer.Get("Login_Error_GetSessionDetailsFailed"));
                break;

            default:
                ShowErrorMessage(message.ErrorMessage);
                break;
        }
    }

    private void ShowErrorMessage(string message)
    {
        ErrorMessage = message;
    }

    private void ClearErrorMessage()
    {
        ErrorMessage = string.Empty;
    }
}