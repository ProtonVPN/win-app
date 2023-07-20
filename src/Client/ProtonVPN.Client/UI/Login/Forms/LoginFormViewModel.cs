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
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Messages;
using ProtonVPN.Client.Models;
using ProtonVPN.Client.Models.Urls;
using Windows.System;

namespace ProtonVPN.Client.UI.Login.Forms;

public partial class LoginFormViewModel : ViewModelBase, IEventMessageReceiver<LoginSuccessMessage>
{
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IUrls _urls;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private bool _isLoggingIn;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _username = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _password = string.Empty;

    public LoginFormViewModel(ILocalizationProvider localizationProvider, IEventMessageSender eventMessageSender,
        IUserAuthenticator userAuthenticator, IUrls urls) : base(localizationProvider)
    {
        _eventMessageSender = eventMessageSender;
        _userAuthenticator = userAuthenticator;
        _urls = urls;
    }

    public string CreateAccountUrl => _urls.CreateAccount;

    [RelayCommand(CanExecute = nameof(CanLogIn))]
    public async Task LoginAsync()
    {
        try
        {
            IsLoggingIn = true;

            SecureString secureString = new();
            foreach (char c in Password)
            {
                secureString.AppendChar(c);
            }

            secureString.MakeReadOnly();

            AuthResult result = await _userAuthenticator.LoginUserAsync(Username, secureString);
            if (result.Success)
            {
                Password = string.Empty;
                _eventMessageSender.Send(new LoginStateChangedMessage(LoginState.Success));
                _eventMessageSender.Send(new LoginSuccessMessage());
            }
            else
            {
                if (result.Value == AuthError.TwoFactorRequired)
                {
                    Password = string.Empty;
                    _eventMessageSender.Send(new LoginStateChangedMessage(LoginState.TwoFactorRequired));
                }
                else
                {
                    _eventMessageSender.Send(new LoginStateChangedMessage(LoginState.Error, result.Value,
                        result.Error));
                }
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
        return Username.Length > 0 && Password.Length > 0 && !IsLoggingIn;
    }

    public void Receive(LoginSuccessMessage message)
    {
        ClearInputs();
    }

    [RelayCommand]
    public async Task ResetPasswordAsync()
    {
        await _urls.NavigateToAsync(_urls.ResetPassword);
    }

    [RelayCommand]
    public async Task ForgotUsernameAsync()
    {
        await _urls.NavigateToAsync(_urls.ForgotUsername);
    }

    [RelayCommand]
    public async Task TroubleSigningInAsync()
    {
        await _urls.NavigateToAsync(_urls.TroubleSigningIn);
    }

    [RelayCommand]
    public async Task ReportAnIssueAsync()
    {
        //TODO: add report a bug feature
    }

    private void ClearInputs()
    {
        Username = string.Empty;
        Password = string.Empty;
    }
}