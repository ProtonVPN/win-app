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

using System.Management;

namespace ProtonVPN.Vpn.Networks.Adapters
{
    public class NetworkAdapter : INetworkAdapter
    {
        public const string NET_CONNECTION_ID_KEY = "NetConnectionID";
        public const string NAME_KEY = "Name";
        public const string DESCRIPTION_KEY = "Description";
        public const string PRODUCT_NAME_KEY = "ProductName";
        public const string NET_CONNECTION_STATUS_KEY = "NetConnectionStatus";

        public string NetConnectionId { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string ProductName { get; private set; }
        public NetConnectionStatus? NetConnectionStatus { get; private set; }

        private readonly ManagementObject _managementObject;

        public NetworkAdapter(ManagementObject managementObject)
        {
            _managementObject = managementObject;
            MapProperties();
        }

        private void MapProperties()
        {
            NetConnectionId = GetNetworkAdapterPropertyValueOrDefault<string>(_managementObject, NET_CONNECTION_ID_KEY);
            Name = GetNetworkAdapterPropertyValueOrDefault<string>(_managementObject, NAME_KEY);
            Description = GetNetworkAdapterPropertyValueOrDefault<string>(_managementObject, DESCRIPTION_KEY);
            ProductName = GetNetworkAdapterPropertyValueOrDefault<string>(_managementObject, PRODUCT_NAME_KEY);
            NetConnectionStatus = GetNetConnectionStatusOrNull(_managementObject);
        }

        private T GetNetworkAdapterPropertyValueOrDefault<T>(ManagementObject networkAdapter, string propertyKey)
        {
            T value;
            try
            {
                value = (T)networkAdapter[propertyKey];
            }
            catch
            {
                value = default(T);
            }

            return value;
        }

        private NetConnectionStatus? GetNetConnectionStatusOrNull(ManagementObject networkAdapter)
        {
            ushort netConnectionStatusInt = GetNetworkAdapterPropertyValueOrDefault<ushort>(
                networkAdapter, NET_CONNECTION_STATUS_KEY);
            NetConnectionStatus? netConnectionStatus;
            try
            {
                netConnectionStatus = (NetConnectionStatus)netConnectionStatusInt;
            }
            catch
            {
                netConnectionStatus = null;
            }

            return netConnectionStatus;
        }

        public void Enable()
        {
            _managementObject.InvokeMethod("Enable", null);
        }

        public void Disable()
        {
            _managementObject.InvokeMethod("Disable", null);
        }

        public string GenerateLoggingDescription()
        {
            return
                $"{NET_CONNECTION_ID_KEY} '{NetConnectionId}', " +
                $"{NAME_KEY} '{Name}', " +
                $"{DESCRIPTION_KEY} '{Description}', " +
                $"{PRODUCT_NAME_KEY} '{ProductName}', " +
                $"{NET_CONNECTION_STATUS_KEY} '{NetConnectionStatus}'";
        }
    }
}