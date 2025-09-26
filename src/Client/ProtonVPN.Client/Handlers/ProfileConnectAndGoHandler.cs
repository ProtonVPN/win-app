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

using ProtonVPN.Client.Common.Enums;
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
    private const int DELAY_AFTER_CONNECTION_IN_MS = 1000;

    private readonly IConnectionManager _connectionManager;
    private readonly IUrlsBrowser _urlsBrowser;
    private readonly IFilesBrowser _appsBrowser;
    private readonly ILogger _logger;

    private IConnectionProfile? _lastProfile;

    public ProfileConnectAndGoHandler(
        IConnectionManager connectionManager,
        IUrlsBrowser urlsBrowser,
        IFilesBrowser appsBrowser,
        ILogger logger)
    {
        _connectionManager = connectionManager;
        _urlsBrowser = urlsBrowser;
        _appsBrowser = appsBrowser;
        _logger = logger;
    }

    public async void Receive(ConnectionStatusChangedMessage message)
    {
        if (_connectionManager.IsDisconnected)
        {
            _lastProfile = null;
            return;
        }

        if (_connectionManager.IsConnected &&
            _connectionManager.CurrentConnectionIntent is IConnectionProfile profile &&
            profile.Options.ConnectAndGo.IsEnabled)
        {
            // Extra delay to ensure the connection is fully established before triggering Connect and go.
            await Task.Delay(DELAY_AFTER_CONNECTION_IN_MS);

            if (_lastProfile != null && _lastProfile.IsSameAs(profile))
            {
                return;
            }

            _lastProfile = profile;

            IConnectAndGoOption connectAndGo = profile.Options.ConnectAndGo;

            switch (connectAndGo.Mode)
            {
                case ConnectAndGoMode.Website:
                    string url = connectAndGo.Url.ToFormattedUrl();
                    _logger.Info<AppLog>($"Connect and go - Open a website: {url}");
                    _urlsBrowser.BrowseTo(url, connectAndGo.UsePrivateBrowsingMode);
                    break;

                case ConnectAndGoMode.Application:
                    string appPath = connectAndGo.AppPath ?? string.Empty;
                    _logger.Info<AppLog>($"Connect and go - Open an app: {appPath}");
                    _appsBrowser.OpenApp(appPath);
                    break;
            }
        }
    }
}