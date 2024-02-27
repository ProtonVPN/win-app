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
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Auth.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.GuestHole;
using ProtonVPN.Client.Messages;
using ProtonVPN.Client.Models;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Login.Forms;

public partial class TwoFactorFormViewModel : PageViewModelBase<ILoginViewNavigator>, IEventMessageReceiver<LoginStateChangedMessage>
{
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IGuestHoleActionExecutor _guestHoleActionExecutor;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AuthenticateCommand))]
    private bool _isAuthenticating;

    public IRelayCommand<string> AuthenticateCommand { get; }

    public override bool IsBackEnabled => true;

    public event EventHandler OnTwoFactorFailure;

    public TwoFactorFormViewModel(
        ILoginViewNavigator loginViewNavigator, 
        ILocalizationProvider localizationProvider, 
        IEventMessageSender eventMessageSender,
        IUserAuthenticator userAuthenticator, 
        IGuestHoleActionExecutor guestHoleActionExecutor,
        ILogger logger,
        IIssueReporter issueReporter) 
        : base(loginViewNavigator, localizationProvider, logger, issueReporter)
    {
        _eventMessageSender = eventMessageSender;
        _userAuthenticator = userAuthenticator;
        _guestHoleActionExecutor = guestHoleActionExecutor;

        AuthenticateCommand = new RelayCommand<string>(twoFactorCode => AuthenticateAsync(twoFactorCode), CanAuthenticate);
    }

    public async Task AuthenticateAsync(string twoFactorCode)
    {
        try
        {
            IsAuthenticating = true;

            _eventMessageSender.Send(new LoginStateChangedMessage(LoginState.Authenticating));

            AuthResult result = await _userAuthenticator.SendTwoFactorCodeAsync(twoFactorCode);
            if (result.Success)
            {
                if (_guestHoleActionExecutor.IsActive())
                {
                    await _guestHoleActionExecutor.DisconnectAsync();
                }

                _eventMessageSender.Send(new LoginStateChangedMessage(LoginState.Success));
            }
            else
            {
                _eventMessageSender.Send(new LoginStateChangedMessage(LoginState.TwoFactorFailed, result.Value));
            }
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

    private bool CanAuthenticate(string twoFactorCode)
    {
        return twoFactorCode is { Length: 6 } && !IsAuthenticating;
    }

    public void Receive(LoginStateChangedMessage message)
    {
        if (message.Value == LoginState.TwoFactorFailed)
        {
            OnTwoFactorFailure?.Invoke(this, new EventArgs());
        }
    }
}