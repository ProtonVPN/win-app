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
using System.Threading.Tasks;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.Logging.Contracts.Events.ConnectLogs;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Core.Network;

namespace ProtonVPN.Core
{
    internal class AutoConnect : IVpnStateAware
    {
        private readonly IAppSettings _appSettings;
        private readonly INetworkClient _networkClient;
        private readonly IVpnManager _vpnManager;
        private readonly ILogger _logger;
        private VpnStatus _vpnStatus;
        private bool _connectedToInsecureWifi;

        public AutoConnect(
            IAppSettings appSettings,
            INetworkClient networkClient,
            IVpnManager vpnManager,
            ILogger logger)
        {
            _appSettings = appSettings;
            _networkClient = networkClient;
            _vpnManager = vpnManager;
            _logger = logger;

            _networkClient.WifiChangeDetected += OnWifiChangeDetected;
        }

        public async Task LoadAsync(bool autoLogin)
        {
            if (!AutoConnectRequired(autoLogin))
            {
                return;
            }

            try
            {
                _logger.Info<ConnectTriggerLog>("Automatically connecting on app start");
                await _vpnManager.QuickConnectAsync();
            }
            catch (OperationCanceledException ex)
            {
                _logger.Error<AppLog>("An error occurred when connecting automatically on app start.", ex);
            }
        }

        private bool AutoConnectRequired(bool autoLogin)
        {
            return autoLogin && _vpnStatus.Equals(VpnStatus.Disconnected) && _appSettings.ConnectOnAppStart;
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _vpnStatus = e.State.Status;

            return Task.CompletedTask;
        }

        private bool InsecureWifiAutoConnectionRequired(bool isSecure)
        {
            return !isSecure && _vpnStatus.Equals(VpnStatus.Disconnected) && _appSettings.ConnectOnInsecureWifi;
        }
        private bool InsecureWifiSecureDisconnectRequired(bool isSecure)
        {
            return isSecure && _vpnStatus.Equals(VpnStatus.Connected) && _connectedToInsecureWifi && _appSettings.SecureDisconnect;
        }

        private void OnWifiChangeDetected(object sender, WifiChangeEventArgs e)
        {
            if (_appSettings.ConnectOnInsecureWifi)
            {
                if (InsecureWifiSecureDisconnectRequired(e.Secure))
                {
                    Task.Factory.StartNew(async () =>
                    {
                        try
                        {
                            _logger.Info<ConnectTriggerLog>("Automatically disconnecting on secure wifi");
                            await _vpnManager.DisconnectAsync();
                        }
                        catch (OperationCanceledException ex)
                        {
                            _logger.Error<AppLog>("An error occurred when disconnecting automatically on secure wifi.", ex);
                        }
                    });
                }
                else if (InsecureWifiAutoConnectionRequired(e.Secure))
                {
                    Task.Factory.StartNew(async () =>
                    {
                        try
                        {
                            _logger.Info<ConnectTriggerLog>("Automatically connecting on insecure wifi");
                            await _vpnManager.QuickConnectAsync();
                        }
                        catch (OperationCanceledException ex)
                        {
                            _logger.Error<AppLog>("An error occurred when connecting automatically on insecure wifi.", ex);
                        }
                    });
                }
            }

            _connectedToInsecureWifi = !e.Secure;
        }
    }
}