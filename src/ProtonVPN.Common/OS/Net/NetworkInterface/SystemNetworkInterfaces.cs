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

using System;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;

namespace ProtonVPN.Common.OS.Net.NetworkInterface
{
    /// <summary>
    /// Provides access to network interfaces on the system.
    /// </summary>
    public class SystemNetworkInterfaces : INetworkInterfaces
    {
        private readonly ILogger _logger;

        public SystemNetworkInterfaces(ILogger logger)
        {
            _logger = logger;

            NetworkChange.NetworkAddressChanged += (s, e) => this.NetworkAddressChanged?.Invoke(s, e);
        }

        public event EventHandler NetworkAddressChanged;

        public INetworkInterface[] Interfaces()
        {
            var interfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();

            return interfaces
                .Select(i => new SystemNetworkInterface(i))
                .Cast<INetworkInterface>()
                .ToArray();
        }

        public INetworkInterface Interface(string interfaceDescription)
        {
            var networkInterface = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                .FirstOrDefault(c => c.Description.StartsWithIgnoringCase(interfaceDescription));

            if (networkInterface != null)
                return new SystemNetworkInterface(networkInterface);

            _logger.Warn($"Unable to find network interface \"{interfaceDescription}\"");
            return new NullNetworkInterface();
        }

        public uint InterfaceIndex(string description, string hardwareId)
        {
            //First try to find TAP interface index by description using System.Net.NetworkInformation
            //as it's faster than querying WMI.
            var index = Interface(description).Index;
            if (index == 0)
            {
                index = InterfaceIndexByHardwareId(hardwareId);
            }

            return index;
        }

        private uint InterfaceIndexByHardwareId(string hardwareId)
        {
            var adapterSearch = new ManagementObjectSearcher("root\\CIMV2",
                "SELECT PNPDeviceID, ConfigManagerErrorCode, InterfaceIndex FROM Win32_NetworkAdapter");
            foreach (var networkAdapter in adapterSearch.Get())
            {
                var pnpDeviceId = (string)networkAdapter["PNPDeviceID"];
                if (string.IsNullOrEmpty(pnpDeviceId))
                {
                    continue;
                }

                var txt = "SELECT HardWareID FROM win32_PNPEntity where DeviceID='" +
                          pnpDeviceId.Replace("\\", "\\\\") + "'";
                var deviceSearch = new ManagementObjectSearcher("root\\CIMV2", txt);
                foreach (var device in deviceSearch.Get())
                {
                    var hardwareIds = (string[])device["HardWareID"];
                    if (hardwareIds != null && hardwareIds.Length > 0)
                    {
                        if (hardwareIds[0] == hardwareId)
                        {
                            var error = Convert.ToUInt32(networkAdapter["ConfigManagerErrorCode"]);
                            if (error != 0)
                            {
                                _logger.Error("TAP adapter error code: " + error);
                                return 0;
                            }

                            return Convert.ToUInt32(networkAdapter["InterfaceIndex"]);
                        }
                    }
                }
            }

            return 0;
        }
    }
}