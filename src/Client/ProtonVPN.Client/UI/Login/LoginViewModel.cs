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
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.UI.Assets.Icons.PathIcons;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Messages;
using ProtonVPN.Client.Models;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Home;
using ProtonVPN.Client.UI.Login.Forms;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.UI.Login;

public partial class LoginViewModel : NavigationPageViewModelBase, IEventMessageReceiver<LoginStateChangedMessage>
{
    private readonly ILogger _logger;
    private readonly IUserAuthenticator _userAuthenticator;

    [ObservableProperty]
    private bool _isToShowError;

    [ObservableProperty]
    private string _errorMessage;

    public LoginViewModel(ILogger logger, ILocalizationProvider localizationProvider, IPageNavigator pageNavigator,
        IUserAuthenticator userAuthenticator) : base(pageNavigator, localizationProvider)
    {
        _logger = logger;
        _userAuthenticator = userAuthenticator;
    }

    public Frame Frame { get; set; }

    public override IconElement Icon { get; } = new User();

    public void Receive(LoginStateChangedMessage message)
    {
        switch (message.Value)
        {
            case LoginState.Success:
                ClearErrorMessage();
                PageNavigator.NavigateTo(typeof(HomeViewModel).FullName!);
                break;
            case LoginState.TwoFactorRequired:
                ClearErrorMessage();
                Frame.Navigate(typeof(TwoFactorForm));
                break;
            case LoginState.TwoFactorFailed:
                switch (message.AuthError)
                {
                    case AuthError.IncorrectTwoFactorCode:
                        ShowErrorMessage(Localizer.Get("Login_Error_IncorrectTwoFactorCode"));
                        break;
                    case AuthError.TwoFactorAuthFailed:
                        ShowErrorMessage(Localizer.Get("Login_Error_TwoFactorFailed"));
                        Frame.Navigate(typeof(LoginForm));
                        break;
                    case AuthError.Unknown:
                        ShowErrorMessage(message.ErrorMessage);
                        break;
                }

                break;
            case LoginState.Error:
                if (message.AuthError == AuthError.MissingGoSrpDll)
                {
                    _logger.Fatal<AppCrashLog>("The app is missing GoSrp.dll");
                    //TODO: add modal about missing file
                    App.MainWindow.Close();
                }
                ShowErrorMessage(message.ErrorMessage);
                break;
        }
    }

    public override async void OnNavigatedTo(object parameter)
    {
        base.OnNavigatedTo(parameter);

        await _userAuthenticator.LogoutAsync();
    }

    private void ShowErrorMessage(string message)
    {
        ErrorMessage = message;
        IsToShowError = true;
    }

    private void ClearErrorMessage()
    {
        ErrorMessage = string.Empty;
        IsToShowError = false;
    }
}