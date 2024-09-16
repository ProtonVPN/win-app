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
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.VpnConfig;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Networking;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Users;
using ProtonVPN.Core.Windows;

namespace ProtonVPN.Core.Config
{
    public class ClientConfig : IClientConfig, ILoggedInAware, ILogoutAware, IVpnPlanAware, IHandle<WindowStateMessage>
    {
        private readonly IAppSettings _appSettings;
        private readonly IApiClient _apiClient;
        private readonly IConfiguration _config;
        private readonly IUserAuthenticator _userAuthenticator;
        private readonly IUserStorage _userStorage;
        private readonly IUserLocationService _userLocationService;
        private readonly ISchedulerTimer _timer;
        private readonly SingleAction _updateAction;

        private readonly List<int> _unsupportedWireGuardPorts = new() { 53 };

        private DateTime _lastUpdateCallTime = DateTime.MinValue;
        private DateTime _lastSuccessfulUpdateTime = DateTime.MinValue;

        public ClientConfig(
            IAppSettings appSettings,
            IScheduler scheduler,
            IApiClient apiClient,
            IConfiguration config,
            IUserAuthenticator userAuthenticator,
            IUserStorage userStorage,
            IUserLocationService userLocationService,
            IEventAggregator eventAggregator)
        {
            _appSettings = appSettings;
            _apiClient = apiClient;
            _config = config;
            _userAuthenticator = userAuthenticator;
            _userStorage = userStorage;
            _userLocationService = userLocationService;

            _timer = scheduler.Timer();
            _timer.Interval = config.ClientConfigUpdateInterval.RandomizedWithDeviation(0.2);
            _timer.Tick += Timer_OnTick;
            _updateAction = new SingleAction(UpdateAction);

            eventAggregator.Subscribe(this);
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
                _lastUpdateCallTime = DateTime.UtcNow;
                string country = _userAuthenticator.IsLoggedIn ? _userStorage.GetLocation().Country : null;
                string ip = _userAuthenticator.IsLoggedIn ? await _userLocationService.GetTruncatedIpAddressAsync() : null;
                ApiResponseResult<VpnConfigResponse> response = await _apiClient.GetVpnConfig(country, ip);
                if (response.Success)
                {
                    _lastSuccessfulUpdateTime = DateTime.UtcNow;
                    _appSettings.OpenVpnTcpPorts = response.Value.DefaultPorts.OpenVpn.Tcp;
                    _appSettings.OpenVpnUdpPorts = response.Value.DefaultPorts.OpenVpn.Udp;
                    _appSettings.WireGuardPorts = response.Value.DefaultPorts.WireGuard.Udp.Where(IsWireGuardPortSupported).ToArray();
                    _appSettings.WireGuardTcpPorts = response.Value.DefaultPorts.WireGuard.Tcp.Where(IsWireGuardPortSupported).ToArray();
                    _appSettings.WireGuardTlsPorts = response.Value.DefaultPorts.WireGuard.Tls.Where(IsWireGuardPortSupported).ToArray();
                    _appSettings.FeatureNetShieldEnabled = response.Value.FeatureFlags.NetShield;
                    _appSettings.FeatureNetShieldStatsEnabled = response?.Value?.FeatureFlags?.NetShieldStats ?? false;

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

                    bool vpnAcceleratorFeatureFlag = response.Value.FeatureFlags.VpnAccelerator ?? true;
                    _appSettings.FeatureVpnAcceleratorEnabled = vpnAcceleratorFeatureFlag;

                    bool smartReconnectFeatureFlag = response.Value.FeatureFlags.SmartReconnect ?? true;
                    _appSettings.FeatureSmartReconnectEnabled = vpnAcceleratorFeatureFlag && smartReconnectFeatureFlag;

                    _appSettings.ShowNonStandardPortsToFreeUsers = response.Value.FeatureFlags.SafeMode ?? false;
                    _appSettings.FeatureStreamingServicesLogosEnabled = response.Value.FeatureFlags.StreamingServicesLogos ?? true;
                    _appSettings.FeaturePromoCodeEnabled = response.Value.FeatureFlags.PromoCode ?? false;
                    _appSettings.FeatureFreeRescopeEnabled = response.Value.FeatureFlags.ShowNewFreePlan ?? false;

                    if (response.Value.ChangeServerAttemptLimit.HasValue)
                    {
                        _appSettings.ChangeServerAttemptLimit = response.Value.ChangeServerAttemptLimit.Value;
                    }

                    if (response.Value.ChangeServerShortDelayInSeconds.HasValue)
                    {
                        _appSettings.ChangeServerShortDelayInSeconds =
                            response.Value.ChangeServerShortDelayInSeconds.Value;
                    }

                    if (response.Value.ChangeServerLongDelayInSeconds.HasValue)
                    {
                        _appSettings.ChangeServerLongDelayInSeconds =
                            response.Value.ChangeServerLongDelayInSeconds.Value;
                    }

                    if (response.Value.SmartProtocol is not null)
                    {
                        List<VpnProtocol> disabledVpnProtocols = [];
                        if (!response.Value.SmartProtocol.WireGuardUdp)
                        {
                            disabledVpnProtocols.Add(VpnProtocol.WireGuardUdp);
                        }
                        if (!response.Value.SmartProtocol.WireGuardTcp)
                        {
                            disabledVpnProtocols.Add(VpnProtocol.WireGuardTcp);
                        }
                        if (!response.Value.SmartProtocol.WireGuardTls)
                        {
                            disabledVpnProtocols.Add(VpnProtocol.WireGuardTls);
                        }
                        if (!response.Value.SmartProtocol.OpenVpnUdp)
                        {
                            disabledVpnProtocols.Add(VpnProtocol.OpenVpnUdp);
                        }
                        if (!response.Value.SmartProtocol.OpenVpnTcp)
                        {
                            disabledVpnProtocols.Add(VpnProtocol.OpenVpnTcp);
                        }
                        _appSettings.DisabledSmartProtocols = disabledVpnProtocols.ToArray();
                    }

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

        public async Task HandleAsync(WindowStateMessage message, CancellationToken cancellationToken)
        {
            DateTime currentDate = DateTime.UtcNow;
            if (message.IsActive
                && _lastUpdateCallTime + TimeSpan.FromMinutes(1) <= currentDate
                && _lastSuccessfulUpdateTime + _config.ClientConfigMinimumUpdateInterval <= currentDate)
            {
                _lastUpdateCallTime = DateTime.UtcNow;
                _updateAction.Run();
            }
        }

        public async Task OnVpnPlanChangedAsync(VpnPlanChangedEventArgs e)
        {
            _updateAction.Run();
        }
    }
}