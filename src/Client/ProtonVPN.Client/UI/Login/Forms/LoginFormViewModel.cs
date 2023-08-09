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
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.GuestHole;
using ProtonVPN.Client.Messages;
using ProtonVPN.Client.Models;
using ProtonVPN.Client.Models.Urls;
using ProtonVPN.Client.UI.ReportIssue.Steps;
using ProtonVPN.Client.UI.ReportIssue;
using Windows.System;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Models.Navigation;

namespace ProtonVPN.Client.UI.Login.Forms;

public partial class LoginFormViewModel : ViewModelBase, IEventMessageReceiver<LoginSuccessMessage>
{
    private readonly IUrls _urls;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IDialogActivator _dialogActivator;
    private readonly IReportIssueViewNavigator _reportIssueViewNavigator;
    private readonly IApiAvailabilityVerifier _apiAvailabilityVerifier;
    private readonly IGuestHoleActionExecutor _guestHoleActionExecutor;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private bool _isLoggingIn;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _username = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _password = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateAccountCommand))]
    private bool _isToShowCreateAccountSpinner;

    public LoginFormViewModel(IUrls urls,
        ILocalizationProvider localizationProvider,
        IEventMessageSender eventMessageSender,
        IUserAuthenticator userAuthenticator,
        IApiAvailabilityVerifier apiAvailabilityVerifier,
        IGuestHoleActionExecutor guestHoleActionExecutor,
        IDialogActivator dialogActivator,
        IReportIssueViewNavigator reportIssueViewNavigator) : base(localizationProvider)
    {
        _urls = urls;
        _eventMessageSender = eventMessageSender;
        _userAuthenticator = userAuthenticator;
        _apiAvailabilityVerifier = apiAvailabilityVerifier;
        _guestHoleActionExecutor = guestHoleActionExecutor;
        _dialogActivator = dialogActivator;
        _reportIssueViewNavigator = reportIssueViewNavigator;
    }

    public string CreateAccountUrl => _urls.CreateAccount;

    [RelayCommand(CanExecute = nameof(CanLogIn))]
    public async Task LoginAsync()
    {
        try
        {
            IsLoggingIn = true;

            AuthResult result = await _userAuthenticator.LoginUserAsync(Username, GetSecurePassword());
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

    private void HandleError(AuthResult result)
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

    private async Task HandleSuccessAsync()
    {
        Password = string.Empty;

        if (_guestHoleActionExecutor.IsActive())
        {
            await _guestHoleActionExecutor.DisconnectAsync();
        }

        _eventMessageSender.Send(new LoginStateChangedMessage(LoginState.Success));
        _eventMessageSender.Send(new LoginSuccessMessage());
    }

    private SecureString GetSecurePassword()
    {
        SecureString secureString = new();
        foreach (char c in Password)
        {
            secureString.AppendChar(c);
        }

        secureString.MakeReadOnly();

        return secureString;
    }

    private bool CanLogIn()
    {
        return Username.Length > 0 && Password.Length > 0 && !IsLoggingIn;
    }

    private bool CanCreateAccount => !IsToShowCreateAccountSpinner;

    public void Receive(LoginSuccessMessage message)
    {
        ClearInputs();
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
    public void ReportAnIssue()
    {
        _dialogActivator.ShowDialog<ReportIssueShellViewModel>();

        _reportIssueViewNavigator.NavigateTo<CategorySelectionViewModel>();
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

    private void ClearInputs()
    {
        Username = string.Empty;
        Password = string.Empty;
    }
}