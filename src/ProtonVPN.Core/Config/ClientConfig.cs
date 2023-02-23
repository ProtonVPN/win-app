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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.VpnConfig;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Core.Config
{
    public class ClientConfig : IClientConfig, ILoggedInAware, ILogoutAware
    {
        private readonly IApiClient _apiClient;
        private readonly ISchedulerTimer _timer;
        private readonly SingleAction _updateAction;
        private readonly IAppSettings _appSettings;

        private readonly List<int> _unsupportedWireGuardPorts = new() { 53 };

        public ClientConfig(
            IAppSettings appSettings,
            IScheduler scheduler,
            IApiClient apiClient,
            IConfiguration config)
        {
            _appSettings = appSettings;
            _apiClient = apiClient;
            _timer = scheduler.Timer();
            _timer.Interval = config.ClientConfigUpdateInterval.RandomizedWithDeviation(0.2);
            _timer.Tick += Timer_OnTick;
            _updateAction = new SingleAction(UpdateAction);
        }

        public async Task Update()
        {
            await _updateAction.Run();
        }

        public void OnUserLoggedIn()
        {
            _timer.Start();
        }

        public void OnUserLoggedOut()
        {
            _timer.Stop();
        }

        private void Timer_OnTick(object sender, EventArgs eventArgs)
        {
            _updateAction.Run();
        }

        private async Task UpdateAction()
        {
            try
            {
                ApiResponseResult<VpnConfigResponse> response = await _apiClient.GetVpnConfig();
                if (response.Success)
                {
                    _appSettings.OpenVpnTcpPorts = response.Value.DefaultPorts.OpenVpn.Tcp;
                    _appSettings.OpenVpnUdpPorts = response.Value.DefaultPorts.OpenVpn.Udp;
                    _appSettings.WireGuardPorts = response.Value.DefaultPorts.WireGuard.Udp.Where(IsWireGuardPortSupported).ToArray();
                    _appSettings.FeatureNetShieldEnabled = response.Value.FeatureFlags.NetShield;

                    if (response.Value.FeatureFlags.ServerRefresh.HasValue)
                    {
                        _appSettings.FeatureMaintenanceTrackerEnabled = response.Value.FeatureFlags.ServerRefresh.Value;
                    }

                    if (response.Value.FeatureFlags.PollNotificationApi.HasValue)
                    {
                        _appSettings.FeaturePollNotificationApiEnabled = response.Value.FeatureFlags.PollNotificationApi.Value;
                    }

                    if (response.Value.ServerRefreshInterval.HasValue)
                    {
                        _appSettings.MaintenanceCheckInterval = TimeSpan.FromMinutes(response.Value.ServerRefreshInterval.Value);
                    }

                    if (response.Value.FeatureFlags.PortForwarding.HasValue)
                    {
                        _appSettings.FeaturePortForwardingEnabled = response.Value.FeatureFlags.PortForwarding.Value;
                    }

                    _appSettings.FeatureSmartProtocolWireGuardEnabled = response.Value.SmartProtocol.WireGuard;

                    bool vpnAcceleratorFeatureFlag = response.Value.FeatureFlags.VpnAccelerator ?? true;
                    _appSettings.FeatureVpnAcceleratorEnabled = vpnAcceleratorFeatureFlag;

                    bool smartReconnectFeatureFlag = response.Value.FeatureFlags.SmartReconnect ?? true;
                    _appSettings.FeatureSmartReconnectEnabled = vpnAcceleratorFeatureFlag && smartReconnectFeatureFlag;

                    _appSettings.ShowNonStandardPortsToFreeUsers = response.Value.FeatureFlags.SafeMode ?? false;
                    _appSettings.FeatureStreamingServicesLogosEnabled = response.Value.FeatureFlags.StreamingServicesLogos ?? true;
                    _appSettings.FeaturePromoCodeEnabled = response.Value.FeatureFlags.PromoCode ?? false;
                }
            }
            catch
            {
            }
        }

        private bool IsWireGuardPortSupported(int port)
        {
            return !_unsupportedWireGuardPorts.Contains(port);
        }
    }
}