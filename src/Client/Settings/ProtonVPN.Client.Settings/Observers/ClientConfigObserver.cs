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

using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.VpnConfig;
using ProtonVPN.Client.Common.Observers;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Observers;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.SettingsLogs;

namespace ProtonVPN.Client.Settings.Observers;

public class ClientConfigObserver :
    PollingObserverBase,
    IClientConfigObserver,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<LoggedOutMessage>,
    IEventMessageReceiver<DeviceLocationChangedMessage>
{
    private readonly List<int> _unsupportedWireGuardUdpPorts = [53];

    private readonly ISettings _settings;
    private readonly IApiClient _apiClient;
    private readonly IConfiguration _config;

    protected override TimeSpan PollingInterval => _config.ClientConfigUpdateInterval;

    public ClientConfigObserver(
        ILogger logger,
        IIssueReporter issueReporter,
        ISettings settings,
        IApiClient apiClient,
        IConfiguration config)
        : base(logger, issueReporter)
    {
        _settings = settings;
        _apiClient = apiClient;
        _config = config;
    }

    public void Receive(LoggedInMessage message)
    {
        StartTimer();
    }

    public void Receive(LoggedOutMessage message)
    {
        StopTimer();
    }

    public void Receive(DeviceLocationChangedMessage message)
    {
        if (message.HasCountryChangedAndHasValue && message.IsUserLoggedIn)
        {
            TriggerAction.Run();
        }
    }

    public Task UpdateAsync(CancellationToken cancellationToken)
    {
        return MakeClientConfigRequestAsync(cancellationToken);
    }

    protected override async Task OnTriggerAsync()
    {
        await MakeClientConfigRequestAsync(CancellationToken.None);
    }

    private async Task MakeClientConfigRequestAsync(CancellationToken cancellationToken)
    {
        try
        {
            Logger.Info<SettingsLog>("Retrieving Client Config");
            ApiResponseResult<VpnConfigResponse> response = await _apiClient.GetVpnConfigAsync(_settings.DeviceLocation, cancellationToken);
            if (response.Success)
            {
                HandleVpnConfigResponse(response.Value);
            }
        }
        catch (Exception e)
        {
            Logger.Error<SettingsLog>("Failed to retrieve Client Config", e);
        }
    }

    private void HandleVpnConfigResponse(VpnConfigResponse value)
    {
        _settings.OpenVpnTcpPorts = value.DefaultPorts.OpenVpn.Tcp;
        _settings.OpenVpnUdpPorts = value.DefaultPorts.OpenVpn.Udp;
        _settings.WireGuardUdpPorts = value.DefaultPorts.WireGuard.Udp.Where(IsWireGuardUdpPortSupported).ToArray();
        _settings.WireGuardTcpPorts = value.DefaultPorts.WireGuard.Tcp;
        _settings.WireGuardTlsPorts = value.DefaultPorts.WireGuard.Tls;

        if (value.FeatureFlags.ServerRefresh.HasValue)
        {
            _settings.IsFeatureConnectedServerCheckEnabled = value.FeatureFlags.ServerRefresh.Value;
        }

        if (value.ServerRefreshInterval.HasValue)
        {
            _settings.ConnectedServerCheckInterval = TimeSpan.FromMinutes(value.ServerRefreshInterval.Value);
        }

        _settings.ChangeServerSettings = new()
        {
            AttemptsLimit = value.ChangeServerAttemptLimit,
            ShortDelay = TimeSpan.FromSeconds(value.ChangeServerShortDelayInSeconds),
            LongDelay = TimeSpan.FromSeconds(value.ChangeServerLongDelayInSeconds)
        };

        if (value.SmartProtocol is not null)
        {
            List<VpnProtocol> disabledVpnProtocols = [];
            if (!value.SmartProtocol.WireGuardUdp)
            {
                disabledVpnProtocols.Add(VpnProtocol.WireGuardUdp);
            }
            if (!value.SmartProtocol.WireGuardTcp)
            {
                disabledVpnProtocols.Add(VpnProtocol.WireGuardTcp);
            }
            if (!value.SmartProtocol.WireGuardTls)
            {
                disabledVpnProtocols.Add(VpnProtocol.WireGuardTls);
            }
            if (!value.SmartProtocol.OpenVpnUdp)
            {
                disabledVpnProtocols.Add(VpnProtocol.OpenVpnUdp);
            }
            if (!value.SmartProtocol.OpenVpnTcp)
            {
                disabledVpnProtocols.Add(VpnProtocol.OpenVpnTcp);
            }
            _settings.DisabledSmartProtocols = disabledVpnProtocols.ToArray();
        }
    }

    private bool IsWireGuardUdpPortSupported(int port)
    {
        return !_unsupportedWireGuardUdpPorts.Contains(port);
    }
}