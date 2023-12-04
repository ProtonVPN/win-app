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

using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.VpnConfig;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Observers;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.SettingsLogs;

namespace ProtonVPN.Client.Settings.Observers;

public class ClientConfigObserver : ObserverBase, IClientConfigObserver, IEventMessageReceiver<LoggedInMessage>, IEventMessageReceiver<LoggedOutMessage>
{
    private readonly List<int> _unsupportedWireGuardPorts = new() { 53 };

    protected override TimeSpan PollingInterval => Config.ClientConfigUpdateInterval;

    public ClientConfigObserver(
        ISettings settings,
        IApiClient apiClient,
        IConfiguration config,
        ILogger logger)
        : base(settings, apiClient, config, logger)
    {
    }

    public void Receive(LoggedInMessage message)
    {
        StartTimer();
    }

    public void Receive(LoggedOutMessage message)
    {
        StopTimer();
    }

    protected override async Task UpdateAsync()
    {
        try
        {
            Logger.Info<SettingsLog>("Retrieving Client Config");

            ApiResponseResult<VpnConfigResponse> response = await ApiClient.GetVpnConfig();
            if (response.Success)
            {
                Settings.OpenVpnTcpPorts = response.Value.DefaultPorts.OpenVpn.Tcp;
                Settings.OpenVpnUdpPorts = response.Value.DefaultPorts.OpenVpn.Udp;
                Settings.WireGuardPorts = response.Value.DefaultPorts.WireGuard.Udp.Where(IsWireGuardPortSupported).ToArray();

                // TODO: Retrieve legacy feature flags here?
            }
        }
        catch (Exception e)
        {
            Logger.Error<SettingsLog>("Failed to retrieve Client Config", e);
        }
    }

    private bool IsWireGuardPortSupported(int port)
    {
        return !_unsupportedWireGuardPorts.Contains(port);
    }
}
