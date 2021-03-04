/*
 * Copyright (c) 2020 Proton Technologies AG
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
using ProtonVPN.Common.Os.Net;
using ProtonVPN.Service.Network;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Service.Vpn
{
    internal class NetworkSettings : IVpnStateAware
    {
        private readonly ICurrentNetworkInterface _currentNetworkInterface;
        private readonly ILogger _logger;

        public NetworkSettings(ILogger logger, ICurrentNetworkInterface currentNetworkInterface)
        {
            _logger = logger;
            _currentNetworkInterface = currentNetworkInterface;
        }

        public bool ApplyNetworkSettings()
        {
            uint interfaceIndex = _currentNetworkInterface.Index;
            if (interfaceIndex == 0)
            {
                return false;
            }

            try
            {
                NetworkUtil.SetLowestTapMetric(interfaceIndex);
            }
            catch (NetworkUtilException e)
            {
                _logger.Error("Failed to apply network settings. Error code: " + e.Code);
                return false;
            }

            return true;
        }

        private void RestoreNetworkSettings()
        {
            uint interfaceIndex = _currentNetworkInterface.Index;
            if (interfaceIndex == 0)
            {
                return;
            }

            try
            {
                NetworkUtil.RestoreDefaultTapMetric(interfaceIndex);
            }
            catch (NetworkUtilException e)
            {
                _logger.Error("Failed restore network settings. Error code: " + e.Code);
            }
        }

        public void OnVpnDisconnected(VpnState state)
        {
            RestoreNetworkSettings();
        }

        public void OnVpnConnected(VpnState state)
        {
        }

        public void OnVpnConnecting(VpnState state)
        {
        }
    }
}