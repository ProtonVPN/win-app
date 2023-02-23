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
using ProtonVPN.Common;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.ConnectLogs;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.PortMapping;

namespace ProtonVPN.Vpn.Connection
{
    public class PortForwardingWrapper : ISingleVpnConnection
    {
        private readonly ILogger _logger;
        private readonly IPortMappingProtocolClient _portMappingProtocolClient;
        private readonly ISingleVpnConnection _origin;

        public PortForwardingWrapper(
            ILogger logger,
            IPortMappingProtocolClient portMappingProtocolClient,
            ISingleVpnConnection origin)
        {
            _logger = logger;
            _portMappingProtocolClient = portMappingProtocolClient;
            _origin = origin;

            _origin.StateChanged += Origin_StateChanged;
        }

        public event EventHandler<EventArgs<VpnState>> StateChanged;

        public event EventHandler<ConnectionDetails> ConnectionDetailsChanged
        {
            add => _origin.ConnectionDetailsChanged += value;
            remove => _origin.ConnectionDetailsChanged -= value;
        }

        public InOutBytes Total => _origin.Total;

        public void Connect(VpnEndpoint endpoint, VpnCredentials credentials, VpnConfig config)
        {
            _origin.Connect(endpoint, credentials, config);
        }

        public async void Disconnect(VpnError error)
        {
            await StopPortMappingProtocolClientAsync();
            _origin.Disconnect(error);
        }

        public void UpdateAuthCertificate(string certificate)
        {
            _origin.UpdateAuthCertificate(certificate);
        }

        public async void SetFeatures(VpnFeatures vpnFeatures)
        {
            if (IsToStopPortMappingProtocolClient(vpnFeatures))
            {
                await StopPortMappingProtocolClientAsync();
            }
            _origin.SetFeatures(vpnFeatures);
        }

        private bool IsToStopPortMappingProtocolClient(VpnFeatures vpnFeatures)
        {
            return !vpnFeatures.PortForwarding;
        }

        private void Origin_StateChanged(object sender, EventArgs<VpnState> e)
        {
            VpnState state = e.Data;
            OnStateChanged(state);
            if (IsToStartPortMappingProtocolClient(state))
            {
                _logger.Debug<ConnectLog>("Requesting NAT-PMP client to start.");
                _portMappingProtocolClient.StartAsync();
            }
            else if (IsToStopPortMappingProtocolClient(state))
            {
                StopPortMappingProtocolClientAsync();
            }
        }

        private async Task StopPortMappingProtocolClientAsync()
        {
            _logger.Debug<ConnectLog>("Requesting NAT-PMP client to stop.");
            await _portMappingProtocolClient.StopAsync();
        }

        private bool IsToStartPortMappingProtocolClient(VpnState state)
        {
            return state.Status == VpnStatus.Connected && state.PortForwarding;
        }

        private bool IsToStopPortMappingProtocolClient(VpnState state)
        {
            return state.Status != VpnStatus.Connected || !state.PortForwarding;
        }

        private void OnStateChanged(VpnState state)
        {
            StateChanged?.Invoke(this, new EventArgs<VpnState>(state));
        }
    }
}