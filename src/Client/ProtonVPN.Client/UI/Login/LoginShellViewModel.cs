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
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.Logging.Contracts.Events.GuestHoleLogs;

namespace ProtonVPN.Client.UI.Login;

public partial class LoginShellViewModel : ShellViewModelBase<ILoginViewNavigator>, IEventMessageReceiver<LoginStateChangedMessage>, IEventMessageReceiver<LoggedOutMessage>
{
    private readonly ILogger _logger;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public override bool IsBackEnabled => CurrentPage?.IsBackEnabled ?? false;

    public LoginShellViewModel(
        ILoginViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        ILogger logger)
        : base(viewNavigator, localizationProvider)
    {
        _logger = logger;

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
                break;
            case LogoutReason.SessionExpired:
                ExecuteOnUIThread(() => ShowErrorMessage(Localizer.Get("Login_Error_SessionExpired")));
                break;
        }
    }

    protected override void OnNavigated(object sender, NavigationEventArgs e)
    {
        base.OnNavigated(sender, e);

        OnPropertyChanged(nameof(IsBackEnabled));
    }

    private void HandleAuthError(LoginStateChangedMessage message)
    {
        switch (message.AuthError)
        {
            case AuthError.MissingGoSrpDll:
                _logger.Fatal<AppCrashLog>("The app is missing GoSrp.dll");
                //TODO: add modal about missing file
                ViewNavigator.CloseCurrentWindow();
                break;

            case AuthError.GuestHoleFailed:
                //TODO: show troubleshooting dialog
                _logger.Error<GuestHoleLog>("Failed to authenticate using guest hole.");
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