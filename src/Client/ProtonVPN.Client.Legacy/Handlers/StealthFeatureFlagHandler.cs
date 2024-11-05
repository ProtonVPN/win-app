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

using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts.Observers;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Legacy.Handlers;

public class StealthFeatureFlagHandler : IHandler, IEventMessageReceiver<FeatureFlagsChangedMessage>
{
    private readonly ILogger _logger;
    private readonly ISettings _settings;
    private readonly IConnectionManager _connectionManager;
    private readonly IFeatureFlagsObserver _featureFlagsObserver;

    public StealthFeatureFlagHandler(
        ILogger logger,
        ISettings settings,
        IConnectionManager connectionManager,
        IFeatureFlagsObserver featureFlagsObserver)
    {
        _logger = logger;
        _settings = settings;
        _connectionManager = connectionManager;
        _featureFlagsObserver = featureFlagsObserver;
    }

    public async void Receive(FeatureFlagsChangedMessage message)
    {
        if (_featureFlagsObserver.IsStealthEnabled)
        {
            return;
        }

        if (_settings.VpnProtocol is VpnProtocol.WireGuardTls or VpnProtocol.WireGuardTcp)
        {
            _logger.Info<AppLog>("Switching to smart protocol because Stealth was disabled by the feature flag.");
            _settings.VpnProtocol = VpnProtocol.Smart;
        }

        if (_connectionManager.IsConnected &&
            (_connectionManager.CurrentConnectionDetails?.Protocol is VpnProtocol.WireGuardTls or VpnProtocol.WireGuardTcp))
        {
            await _connectionManager.ReconnectAsync();
        }
    }
}