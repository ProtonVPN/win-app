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

using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Core.Services.Mapping;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Core.Services.Navigation.Bases;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Servers.Cache;
using ProtonVPN.Client.UI.Login;
using ProtonVPN.Client.UI.Main;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Services.Navigation;

public class MainWindowViewNavigator : ViewNavigatorBase, IMainWindowViewNavigator,
    IEventMessageReceiver<AuthenticationStatusChanged>
{
    private readonly IServersCache _serversCache;
    private readonly IUserAuthenticator _userAuthenticator;

    public MainWindowViewNavigator(
        ILogger logger,
        IPageViewMapper pageViewMapper,
        IUIThreadDispatcher uiThreadDispatcher,
        IServersCache serversCache,
        IUserAuthenticator userAuthenticator)
        : base(logger, pageViewMapper, uiThreadDispatcher)
    {
        _serversCache = serversCache;
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

    public Task<bool> NavigateToNoServersViewAsync()
    {
        return NavigateToAsync<NoServersPageViewModel>();
    }

    public override Task<bool> NavigateToDefaultAsync()
    {
        return _userAuthenticator.IsLoggedIn
            ? _serversCache.IsEmpty()
                ? NavigateToNoServersViewAsync()
                : NavigateToMainViewAsync()
            : NavigateToLoginViewAsync();
    }

    public void Receive(AuthenticationStatusChanged message)
    {
        UIThreadDispatcher.TryEnqueue(async () =>
        {
            await NavigateToDefaultAsync();
        });
    }
}