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

using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Core.Services.Mapping;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Core.Services.Navigation.Bases;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.UI.Main.Settings;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Services.Navigation;

public class MainViewNavigator : ViewNavigatorBase, IMainViewNavigator,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<LoggedOutMessage>
{
    private ConnectionStatus _connectionStatus = ConnectionStatus.Disconnected;

    public MainViewNavigator(
        ILogger logger,
        IPageViewMapper pageViewMapper,
        IUIThreadDispatcher uiThreadDispatcher) 
        : base(logger, pageViewMapper, uiThreadDispatcher)
    { }

    public Task<bool> NavigateToHomeViewAsync(bool forceNavigation = false)
    {
        return ClearFrameAsync(forceNavigation);
    }

    public Task<bool> NavigateToSettingsViewAsync()
    {
        return NavigateToAsync<SettingsPageViewModel>();
    }

    public override Task<bool> NavigateToDefaultAsync()
    {
        return NavigateToHomeViewAsync();
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        if (_connectionStatus == message.ConnectionStatus)
        {
            return;
        }

        _connectionStatus = message.ConnectionStatus;

        if (message.ConnectionStatus == ConnectionStatus.Connecting)
        {
            // Force navigation to automatically discard any unsaved changes
            UIThreadDispatcher.TryEnqueue(async () => await NavigateToHomeViewAsync(forceNavigation: true));
        }
    }

    public void Receive(LoggedOutMessage message)
    {
        // Force navigation to automatically discard any unsaved changes
        UIThreadDispatcher.TryEnqueue(async () => await NavigateToHomeViewAsync(forceNavigation: true));
    }
}