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

using ProtonVPN.Client.Contracts.Enums;
using ProtonVPN.Client.Contracts.Services.Mapping;
using ProtonVPN.Client.Contracts.Services.Navigation;
using ProtonVPN.Client.Contracts.Services.Navigation.Bases;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.TEMP;
using ProtonVPN.Client.UI.Main.Home;
using ProtonVPN.Client.UI.Main.Settings;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Services.Navigation;

public class MainViewNavigator : ViewNavigatorBase, IMainViewNavigator,
    IEventMessageReceiver<ConnectionStatusChangedMessage>
{
    private ConnectionStatus _connectionStatus = ConnectionStatus.Disconnected;

    public MainViewNavigator(
        ILogger logger,
        IPageViewMapper pageViewMapper) 
        : base(logger, pageViewMapper)
    {
    }

    public Task<bool> NavigateToHomeViewAsync()
    {
        return NavigateToAsync<HomePageViewModel>();
    }

    public Task<bool> NavigateToSettingsViewAsync()
    {
        return NavigateToAsync<SettingsPageViewModel>();
    }

    public override Task<bool> NavigateToDefaultAsync()
    {
        return NavigateToHomeViewAsync();
    }

    public Task<bool> NavigateToGalleryViewAsync()
    {
        return NavigateToAsync<GalleryPageViewModel>();
    }

    // TODO: prevent NavigateToDefaultAsync from being called multiple times due out of order messages
    // Disconnected
    // Connecting
    // Disconnected
    // Connecting
    // Connected
    public void Receive(ConnectionStatusChangedMessage message)
    {
        if (_connectionStatus == message.ConnectionStatus)
        {
            return;
        }

        _connectionStatus = message.ConnectionStatus;

        if (message.ConnectionStatus == ConnectionStatus.Connecting)
        {
            NavigateToDefaultAsync();
        }
    }
}