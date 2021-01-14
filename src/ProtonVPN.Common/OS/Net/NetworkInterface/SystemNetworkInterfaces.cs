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
using System.Net.NetworkInformation;
using System.Security;
using Microsoft.Win32;

namespace ProtonVPN.Common.OS.Net.NetworkInterface
{
    /// <summary>
    /// Provides access to network interfaces on the system.
    /// </summary>
    public class SystemNetworkInterfaces : INetworkInterfaces
    {
        public SystemNetworkInterfaces()
        {
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

        public uint InterfaceIndex(string hardwareId)
        {
            string guid = GetInterfaceGuid(hardwareId);
            if (!string.IsNullOrEmpty(guid))
            {
                INetworkInterface netInterface = Interfaces().FirstOrDefault(i => i.Id == guid);
                return netInterface?.Index ?? 0;
            }

            return 0;
        }

        private string GetInterfaceGuid(string hardwareId)
        {
            //This key is consistent across windows and does not change.
            string keyStr = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}";
            using RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyStr);
            if (key != null)
            {
                foreach (string adapterKey in key.GetSubKeyNames())
                {
                    try
                    {
                        RegistryKey adapter = key.OpenSubKey(adapterKey);
                        if (adapter == null)
                        {
                            continue;
                        }

                        object componentId = adapter.GetValue("ComponentId");
                        object adapterGuid = adapter.GetValue("NetCfgInstanceId");
                        if (componentId?.ToString() == hardwareId)
                        {
                            return adapterGuid?.ToString();
                        }
                    }
                    catch (SecurityException)
                    {
                        //ignore
                    }
                }
            }

            return string.Empty;
        }
    }
}