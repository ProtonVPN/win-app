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
using ProtonVPN.Client.Common.Observers;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Observers;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.SettingsLogs;

namespace ProtonVPN.Client.Settings.Observers;

public class ClientConfigObserver : 
    PollingObserverBase, 
    IClientConfigObserver, 
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<LoggedOutMessage>
{
    private readonly List<int> _unsupportedWireGuardPorts = new() { 53 };

    private readonly ISettings _settings;
    private readonly IApiClient _apiClient;
    private readonly IConfiguration _config;
    private readonly ILogger _logger;

    protected override TimeSpan PollingInterval => _config.ClientConfigUpdateInterval;

    public ClientConfigObserver(
        ISettings settings,
        IApiClient apiClient,
        IConfiguration config,
        ILogger logger)
        : base()
    {
        _settings = settings;
        _apiClient = apiClient;
        _config = config;
        _logger = logger;
    }

    public void Receive(LoggedInMessage message)
    {
        StartTimerAndTriggerOnStart();
    }

    public void Receive(LoggedOutMessage message)
    {
        StopTimer();
    }

    protected override async Task OnTriggerAsync()
    {
        try
        {
            _logger.Info<SettingsLog>("Retrieving Client Config");

            ApiResponseResult<VpnConfigResponse> response = await _apiClient.GetVpnConfig();
            if (response.Success)
            {
                HandleVpnConfigResponse(response.Value);
            }
        }
        catch (Exception e)
        {
            _logger.Error<SettingsLog>("Failed to retrieve Client Config", e);
        }
    }

    private void HandleVpnConfigResponse(VpnConfigResponse value)
    {
        _settings.OpenVpnTcpPorts = value.DefaultPorts.OpenVpn.Tcp;
        _settings.OpenVpnUdpPorts = value.DefaultPorts.OpenVpn.Udp;
        _settings.WireGuardPorts = value.DefaultPorts.WireGuard.Udp.Where(IsWireGuardPortSupported).ToArray();

        if (value.FeatureFlags.ServerRefresh.HasValue)
        {
            _settings.IsFeatureConnectedServerCheckEnabled = value.FeatureFlags.ServerRefresh.Value;
        }

        if (value.ServerRefreshInterval.HasValue)
        {
            _settings.ConnectedServerCheckInterval = TimeSpan.FromMinutes(value.ServerRefreshInterval.Value);
        }

        // TODO: Retrieve legacy feature flags here?
    }

    private bool IsWireGuardPortSupported(int port)
    {
        return !_unsupportedWireGuardPorts.Contains(port);
    }
}
