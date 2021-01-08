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
using ProtonVPN.Common.OS.Net.NetworkInterface;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Service.Vpn
{
    public class NetworkSettings : IVpnStateAware
    {
        private readonly INetworkInterfaces _networkInterfaces;
        private readonly ILogger _logger;
        private readonly Common.Configuration.Config _config;

        public NetworkSettings(
            ILogger logger,
            INetworkInterfaces networkInterfaces,
            Common.Configuration.Config config)
        {
            _config = config;
            _logger = logger;
            _networkInterfaces = networkInterfaces;
        }

        public bool ApplyNetworkSettings(NetworkSettingsConfig settingsConfig)
        {
            uint tapInterfaceIndex = GetTapInterfaceIndex();
            if (tapInterfaceIndex == 0)
            {
                return false;
            }

            try
            {
                string localInterfaceIp = NetworkUtil.GetBestInterfaceIp(_config.OpenVpn.TapAdapterId).ToString();
                NetworkUtil.SetLowestTapMetric(tapInterfaceIndex);
                NetworkUtil.DeleteDefaultGatewayForIface(tapInterfaceIndex, localInterfaceIp);

                if (settingsConfig.AddDefaultGatewayForTap)
                {
                    NetworkUtil.AddDefaultGatewayForIface(tapInterfaceIndex, localInterfaceIp);
                }
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
            uint tapInterfaceIndex = GetTapInterfaceIndex();
            if (tapInterfaceIndex == 0)
            {
                return;
            }

            try
            {
                NetworkUtil.RestoreDefaultTapMetric(tapInterfaceIndex);
            }
            catch (NetworkUtilException e)
            {
                _logger.Error("Failed restore network settings. Error code: " + e.Code);
            }
        }

        private uint GetTapInterfaceIndex()
        {
            return _networkInterfaces.InterfaceIndex(
                _config.OpenVpn.TapAdapterDescription,
                _config.OpenVpn.TapAdapterId);
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