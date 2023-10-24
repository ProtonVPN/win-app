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
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Threading;
using ProtonVPN.Configurations.Contracts;
using Timer = System.Timers.Timer;

namespace ProtonVPN.Client.Settings;

public class ClientConfigObserver : IClientConfigObserver, IEventMessageReceiver<SettingChangedMessage>
{
    private readonly ISettings _settings;
    private readonly IApiClient _apiClient;
    private readonly IConfiguration _config;

    private readonly SingleAction _updateAction;

    private readonly List<int> _unsupportedWireGuardPorts = new() { 53 };

    private Timer _timer;

    public ClientConfigObserver(
        ISettings settings,
        IApiClient apiClient,
        IConfiguration config)
    {
        _settings = settings;
        _apiClient = apiClient;
        _config = config;

        _updateAction = new SingleAction(UpdateAsync);

        _timer = new Timer
        {
            Interval = _config.ClientConfigUpdateInterval.RandomizedWithDeviation(0.2).TotalMilliseconds
        };
        _timer.Elapsed += OnTimerElapsed;

        InvalidateTimer();
    }

    public async Task UpdateAsync()
    {
        try
        {
            ApiResponseResult<VpnConfigResponse> response = await _apiClient.GetVpnConfig();
            if (response.Success)
            {
                _settings.OpenVpnTcpPorts = response.Value.DefaultPorts.OpenVpn.Tcp;
                _settings.OpenVpnUdpPorts = response.Value.DefaultPorts.OpenVpn.Udp;
                _settings.WireGuardPorts = response.Value.DefaultPorts.WireGuard.Udp.Where(IsWireGuardPortSupported).ToArray();

                // TODO: Retrieve feature flags here
            }
        }
        catch { }
    }

    public void Receive(SettingChangedMessage message)
    {
        switch (message.PropertyName)
        {
            case nameof(ISettings.Username):
                InvalidateTimer();
                break;
        }
    }

    private void OnTimerElapsed(object? sender, EventArgs e)
    {
        _updateAction.Run();
    }

    private bool IsWireGuardPortSupported(int port)
    {
        return !_unsupportedWireGuardPorts.Contains(port);
    }

    private void InvalidateTimer()
    {
        if (string.IsNullOrEmpty(_settings.Username))
        {
            _timer.Stop();
        }
        else
        {
            _timer.Start();
            _updateAction.Run();
        }
    }
}