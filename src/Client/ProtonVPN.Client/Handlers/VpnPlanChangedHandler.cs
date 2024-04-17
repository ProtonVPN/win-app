/*
 * Copyright (c) 2024 Proton AG
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

using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Handlers;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Home;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

public class VpnPlanChangedHandler : IHandler, IEventMessageReceiver<VpnPlanChangedMessage>
{
    private readonly ILogger _logger;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IMainViewNavigator _mainViewNavigator;
    private readonly IUIThreadDispatcher _uiThreadDispatcher;

    public VpnPlanChangedHandler(ILogger logger,
        IUserAuthenticator userAuthenticator,
        IMainViewNavigator mainViewNavigator,
        IUIThreadDispatcher uiThreadDispatcher)
    {
        _logger = logger;
        _userAuthenticator = userAuthenticator;
        _mainViewNavigator = mainViewNavigator;
        _uiThreadDispatcher = uiThreadDispatcher;
    }

    public async void Receive(VpnPlanChangedMessage message)
    {
        if (_userAuthenticator.IsLoggedIn && message.HasMaxTierChanged())
        {
            _logger.Info<AppLog>("Navigating to Home page due to max tier change.");
            _uiThreadDispatcher.TryEnqueue(() => _mainViewNavigator.NavigateToAsync<HomeViewModel>());
        }
    }
}