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

using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.NetworkLogs;
using ProtonVPN.Common.OS.Net;
using ProtonVPN.Common.OS.Net.NetworkInterface;

namespace ProtonVPN.Core.Service.Vpn
{
    public class NetworkAdapterValidator : INetworkAdapterValidator
    {
        private readonly INetworkInterfaceLoader _networkInterfaceLoader;
        private readonly ILogger _logger;

        public NetworkAdapterValidator(INetworkInterfaceLoader networkInterfaceLoader, ILogger logger)
        {
            _networkInterfaceLoader = networkInterfaceLoader;
            _logger = logger;
        }

        public bool IsOpenVpnAdapterAvailable()
        {
            INetworkInterface openVpnTapInterface = _networkInterfaceLoader.GetOpenVpnTapInterface();
            INetworkInterface openVpnTunInterface = _networkInterfaceLoader.GetOpenVpnTunInterface();
            bool isOpenVpnAdapterAvailable = openVpnTapInterface != null || openVpnTunInterface != null;

            LogIsOpenVpnAdapterAvailable(isOpenVpnAdapterAvailable,
                CreateInterfaceLogMessage("TAP", openVpnTapInterface),
                CreateInterfaceLogMessage("TUN", openVpnTunInterface));

            return isOpenVpnAdapterAvailable;
        }

        private string CreateInterfaceLogMessage(string interfaceType, INetworkInterface networkInterface)
        {
            if (networkInterface == null)
            {
                return $"The {interfaceType} adapter is unavailable.";
            }

            return $"The {interfaceType} adapter is available (Index: {networkInterface.Index}, " +
                   $"Name: '{networkInterface.Name}', Description: '{networkInterface.Description}').";
        }

        private void LogIsOpenVpnAdapterAvailable(bool isOpenVpnAdapterAvailable, params string[] openVpnInterfaces)
        {
            string logMessage = "Checking which OpenVPN adapters are available. " + string.Join(" ", openVpnInterfaces);
            if (isOpenVpnAdapterAvailable)
            {
                _logger.Info<NetworkLog>(logMessage);
            }
            else
            {
                _logger.Warn<NetworkLog>(logMessage);
            }
        }
    }
}