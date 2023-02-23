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
using ProtonVPN.Common;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Service.Firewall;
using ProtonVPN.Service.Settings;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Service.Vpn
{
    internal class Ipv6HandlingWrapper : IVpnConnection
    {
        private readonly IServiceSettings _serviceSettings;
        private readonly IFirewall _firewall;
        private readonly Ipv6 _ipv6;
        private readonly ITaskQueue _taskQueue;
        private readonly IVpnConnection _origin;

        private IReadOnlyList<VpnHost> _servers;
        private VpnConfig _config;
        private VpnCredentials _credentials;

        private Task _ipv6Task = Task.CompletedTask;
        private bool _connectRequested;
        private bool _disconnectedReceived;
        private volatile bool _networkChanged;

        private VpnStatus _vpnStatus;

        public Ipv6HandlingWrapper(
            IServiceSettings serviceSettings,
            IFirewall firewall,
            ObservableNetworkInterfaces networkInterfaces,
            Ipv6 ipv6,
            ITaskQueue taskQueue,
            IVpnConnection origin)
        {
            _serviceSettings = serviceSettings;
            _firewall = firewall;
            _ipv6 = ipv6;
            _taskQueue = taskQueue;
            _origin = origin;

            _origin.StateChanged += Origin_StateChanged;
            _serviceSettings.SettingsChanged += OnServiceSettingsChanged;
            networkInterfaces.NetworkInterfacesAdded += NetworkInterfaces_NetworkInterfacesAdded;
        }

        public event EventHandler<EventArgs<VpnState>> StateChanged;

        public event EventHandler<ConnectionDetails> ConnectionDetailsChanged
        {
            add => _origin.ConnectionDetailsChanged += value;
            remove => _origin.ConnectionDetailsChanged -= value;
        }

        public InOutBytes Total => _origin.Total;

        public void Connect(IReadOnlyList<VpnHost> servers, VpnConfig config, VpnCredentials credentials)
        {
            _servers = servers;
            _config = config;
            _credentials = credentials;

            _connectRequested = true;
            _disconnectedReceived = false;

            Queued(Connect);
        }

        public void Disconnect(VpnError error)
        {
            _connectRequested = false;
            _disconnectedReceived = false;

            _origin.Disconnect(error);
        }

        public void UpdateAuthCertificate(string certificate)
        {
            _origin.UpdateAuthCertificate(certificate);
        }

        public void SetFeatures(VpnFeatures vpnFeatures)
        {
            _origin.SetFeatures(vpnFeatures);
        }

        private async void Connect()
        {
            if (!_connectRequested)
            {
                return;
            }

            InvokeConnecting();

            await _ipv6.EnableOnVPNInterfaceAsync();

            if (!_serviceSettings.Ipv6LeakProtection)
            {
                _connectRequested = false;
                _origin.Connect(_servers, _config, _credentials);
                return;
            }

            if (_ipv6Task.IsCompleted)
            {
                if (_ipv6.Enabled)
                {
                    _networkChanged = false;
                    _ipv6Task = _ipv6.DisableAsync();
                }
                else
                {
                    _connectRequested = false;
                    _origin.Connect(_servers, _config, _credentials);
                    return;
                }
            }

            await _ipv6Task;
            Queued(Connect);
        }

        private void Origin_StateChanged(object sender, EventArgs<VpnState> e)
        {
            VpnState state = e.Data;
            _vpnStatus = e.Data.Status;

            if (_connectRequested)
            {
                InvokeConnecting();
                return;
            }

            InvokeStateChanged(state);

            _disconnectedReceived = state.Status == VpnStatus.Disconnected;

            if (_disconnectedReceived)
            {
                Disconnected();
            }
        }

        private async void Disconnected()
        {
            if (!_disconnectedReceived)
            {
                return;
            }

            if (_ipv6Task.IsCompleted)
            {
                if (!_firewall.LeakProtectionEnabled && !_ipv6.Enabled)
                {
                    _networkChanged = false;
                    _ipv6Task = _ipv6.EnableAsync();
                }
                else
                {
                    _disconnectedReceived = false;

                    return;
                }
            }

            await _ipv6Task;
            Queued(Disconnected);
        }

        private void NetworkInterfaces_NetworkInterfacesAdded(object sender, EventArgs e)
        {
            if (_networkChanged || !_connectRequested)
            {
                return;
            }

            _networkChanged = true;
            Queued(NetworkChanged);
        }

        private async void NetworkChanged()
        {
            if (!_networkChanged || _disconnectedReceived)
            {
                return;
            }

            if (_ipv6Task.IsCompleted)
            {
                _networkChanged = false;

                if (!_ipv6.Enabled)
                {
                    _ipv6Task = _ipv6.DisableAsync();
                }
                else
                {
                    return;
                }
            }

            await _ipv6Task;
            Queued(NetworkChanged);
        }

        private void InvokeConnecting()
        {
            VpnHost server = _servers.FirstOrDefault();
            if (!server.IsEmpty())
            {
                InvokeStateChanged(new VpnState(VpnStatus.Pinging, VpnError.None, string.Empty, server.Ip,
                    _config.VpnProtocol, openVpnAdapter: _config.OpenVpnAdapter, label: server.Label));
            }
        }

        private void InvokeStateChanged(VpnState state)
        {
            StateChanged?.Invoke(this, new EventArgs<VpnState>(state));
        }

        private void Queued(Action action)
        {
            _taskQueue.Enqueue(action);
        }

        private void OnServiceSettingsChanged(object sender, Contract.Settings.SettingsContract e)
        {
            Queued(ApplyIpv6Settings);
        }

        private async void ApplyIpv6Settings()
        {
            if (_ipv6.Enabled == !_serviceSettings.Ipv6LeakProtection)
            {
                return;
            }

            if (_vpnStatus != VpnStatus.Connected)
            {
                return;
            }

            if (_ipv6Task.IsCompleted)
            {
                _ipv6Task = _serviceSettings.Ipv6LeakProtection ? _ipv6.DisableAsync() : _ipv6.EnableAsync();
            }

            await _ipv6Task;

            Queued(ApplyIpv6Settings);
        }
    }
}