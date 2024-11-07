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
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.ServerValidation;

namespace ProtonVPN.Vpn.Connection
{
    public class ServerAuthenticatorWrapper : ISingleVpnConnection
    {
        private readonly IServerValidator _serverValidator;
        private readonly ISingleVpnConnection _origin;

        public ServerAuthenticatorWrapper(IServerValidator serverValidator,
            ISingleVpnConnection origin)
        {
            _serverValidator = serverValidator;
            _origin = origin;
            
            _origin.StateChanged += Origin_StateChanged;
        }

        public NetworkTraffic NetworkTraffic => _origin.NetworkTraffic;

        public event EventHandler<EventArgs<VpnState>> StateChanged;

        public event EventHandler<ConnectionDetails> ConnectionDetailsChanged
        {
            add => _origin.ConnectionDetailsChanged += value;
            remove => _origin.ConnectionDetailsChanged -= value;
        }

        private void Origin_StateChanged(object sender, EventArgs<VpnState> e)
        {
            StateChanged?.Invoke(this, e);
        }

        public void Connect(VpnEndpoint endpoint, VpnCredentials credentials, VpnConfig config)
        {
            VpnError error = _serverValidator.Validate(endpoint.Server);
            if (error == VpnError.None)
            {
                _origin.Connect(endpoint, credentials, config);
            }
            else
            {
                DisconnectWithError(error, config);
            }
        }

        private void DisconnectWithError(VpnError error, VpnConfig config)
        {
            StateChanged?.Invoke(this, new EventArgs<VpnState>(
                new VpnState(VpnStatus.Disconnected, error, config.VpnProtocol)));
        }

        public void Disconnect(VpnError error)
        {
            _origin.Disconnect(error);
        }

        public void RequestNetShieldStats()
        {
            _origin.RequestNetShieldStats();
        }

        public void RequestConnectionDetails()
        {
            _origin.RequestConnectionDetails();
        }

        public void SetFeatures(VpnFeatures vpnFeatures)
        {
            _origin.SetFeatures(vpnFeatures);
        }
    }
}