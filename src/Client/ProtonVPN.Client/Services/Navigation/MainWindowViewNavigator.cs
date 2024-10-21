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

using ProtonVPN.Client.Contracts.Messages;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Contracts.Services.Mapping;
using ProtonVPN.Client.Contracts.Services.Navigation;
using ProtonVPN.Client.Contracts.Services.Navigation.Bases;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.UI.Login;
using ProtonVPN.Client.UI.Main;

namespace ProtonVPN.Client.Services.Navigation;

public class MainWindowViewNavigator : ViewNavigatorBase, IMainWindowViewNavigator,
    IEventMessageReceiver<AuthenticationStatusChanged>
{
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IUserAuthenticator _userAuthenticator;

    public MainWindowViewNavigator(
        ILogger logger,
        IEventMessageSender eventMessageSender,
        IPageViewMapper pageViewMapper,
        IUserAuthenticator userAuthenticator)
        : base(logger, pageViewMapper)
    {
        _eventMessageSender = eventMessageSender;
        _userAuthenticator = userAuthenticator;
    }

    public Task<bool> NavigateToLoginViewAsync()
    {
        return NavigateToAsync<LoginPageViewModel>();
    }

    public Task<bool> NavigateToMainViewAsync()
    {
        return NavigateToAsync<MainPageViewModel>();
    }

    public void Receive(AuthenticationStatusChanged message)
    {
        NavigateToDefaultAsync();

        if (message.AuthenticationStatus == AuthenticationStatus.LoggedIn)
        {
            _eventMessageSender.Send(new HomePageDisplayedAfterLoginMessage());
        }
    }

    public override Task<bool> NavigateToDefaultAsync()
    {
        return _userAuthenticator.IsLoggedIn
            ? NavigateToMainViewAsync()
            : NavigateToLoginViewAsync();
    }
}