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

using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Handlers.Bases;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Handlers;

public class ProfileConnectAndGoHandler : IHandler, 
    IEventMessageReceiver<ConnectionStatusChangedMessage>
{
    private readonly IConnectionManager _connectionManager;
    private readonly IUrlsBrowser _urlsBrowser;
    private readonly ILogger _logger;

    private IConnectionProfile? _lastProfile;

    public ProfileConnectAndGoHandler(
        IConnectionManager connectionManager,
        IUrlsBrowser urlsBrowser,
        ILogger logger)
    {
        _connectionManager = connectionManager;
        _urlsBrowser = urlsBrowser;
        _logger = logger;
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        if (_connectionManager.IsDisconnected)
        {
            _lastProfile = null;
            return;
        }

        if (_connectionManager.IsConnected &&
            _connectionManager.CurrentConnectionIntent is IConnectionProfile profile &&
            profile.Options.IsConnectAndGoEnabled)
        {
            if (_lastProfile != null && _lastProfile.IsSameAs(profile))
            {
                return;
            }

            _lastProfile = profile;

            string url = profile.Options.ConnectAndGoUrl.ToFormattedUrl();

            if (url.IsValidUrl())
            {
                _urlsBrowser.BrowseTo(url);
            }
            else
            {
                _logger.Info<AppLog>($"Skip browsing to profile's connect and go url due to invalid format: {url}");
            }
        }
    }
}