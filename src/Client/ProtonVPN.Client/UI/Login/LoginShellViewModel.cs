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
using ProtonVPN.Client.Logic.Auth.Contracts;
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
    private readonly IUserAuthenticator _userAuthenticator;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public override bool IsBackEnabled => CurrentPage?.IsBackEnabled ?? false;

    public LoginShellViewModel(
        ILoginViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IUserAuthenticator userAuthenticator)
        : base(viewNavigator, localizationProvider)
    {
        _logger = logger;
        _userAuthenticator = userAuthenticator;

        _errorMessage = string.Empty;
    }

    public async void Receive(LoginStateChangedMessage message)
    {
        switch (message.Value)
        {
            case LoginState.Success:
                ClearErrorMessage();
                await ViewNavigator.NavigateToSrpLoginAsync();
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
                        await ViewNavigator.NavigateToSrpLoginAsync();
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
                ShowErrorMessage(Localizer.Get("Login_Error_SessionExpired"));
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