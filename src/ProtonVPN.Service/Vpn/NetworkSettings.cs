/*
 * Copyright (c) 2021 Proton Technologies AG
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

using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Networking;
using ProtonVPN.Common.Os.Net;
using ProtonVPN.Common.OS.Net;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Service.Vpn
{
    public class NetworkSettings : IVpnStateAware
    {
        private readonly ILogger _logger;
        private readonly INetworkInterfaceLoader _networkInterfaceLoader;

        public NetworkSettings(ILogger logger, INetworkInterfaceLoader networkInterfaceLoader)
        {
            _logger = logger;
            _networkInterfaceLoader = networkInterfaceLoader;
        }

        public bool IsNetworkAdapterAvailable(VpnProtocol vpnProtocol, OpenVpnAdapter? openVpnAdapter)
        {
            return _networkInterfaceLoader.GetByVpnProtocol(vpnProtocol, openVpnAdapter).Index != 0;
        }

        public void OnVpnDisconnected(VpnState state)
        {
            if (state.VpnProtocol != VpnProtocol.WireGuard)
            {
                RestoreNetworkSettings(state.VpnProtocol, state.OpenVpnAdapter);
            }
        }

        public void OnVpnConnected(VpnState state)
        {
        }

        public void OnVpnConnecting(VpnState state)
        {
            if (state.VpnProtocol != VpnProtocol.WireGuard)
            {
                ApplyNetworkSettings(state.VpnProtocol, state.OpenVpnAdapter);
            }
        }

        private void ApplyNetworkSettings(VpnProtocol vpnProtocol, OpenVpnAdapter? openVpnAdapter)
        {
            uint interfaceIndex = _networkInterfaceLoader.GetByVpnProtocol(vpnProtocol, openVpnAdapter).Index;

            try
            {
                _logger.Info("Setting interface metric...");
                NetworkUtil.SetLowestTapMetric(interfaceIndex);
                _logger.Info("Interface metric set.");
            }
            catch (NetworkUtilException e)
            {
                _logger.Error("Failed to apply network settings. Error code: " + e.Code);
            }
        }

        private void RestoreNetworkSettings(VpnProtocol vpnProtocol, OpenVpnAdapter? openVpnAdapter)
        {

            uint interfaceIndex = _networkInterfaceLoader.GetByVpnProtocol(vpnProtocol, openVpnAdapter).Index;
            if (interfaceIndex == 0)
            {
                return;
            }

            try
            {
                _logger.Info("Restoring interface metric...");
                NetworkUtil.RestoreDefaultTapMetric(interfaceIndex);
                _logger.Info("Interface metric restored.");
            }
            catch (NetworkUtilException e)
            {
                _logger.Error("Failed restore network settings. Error code: " + e.Code);
            }
        }
    }
}