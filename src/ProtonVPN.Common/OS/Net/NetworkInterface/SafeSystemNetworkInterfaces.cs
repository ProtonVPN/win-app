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
using System.Net.NetworkInformation;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;

namespace ProtonVPN.Common.OS.Net.NetworkInterface
{
    public class SafeSystemNetworkInterfaces : INetworkInterfaces
    {
        private readonly ILogger _logger;
        private readonly INetworkInterfaces _origin;

        public SafeSystemNetworkInterfaces(ILogger logger, INetworkInterfaces origin)
        {
            _logger = logger;
            _origin = origin;
        }

        public event EventHandler NetworkAddressChanged
        {
            add => _origin.NetworkAddressChanged += value;
            remove => _origin.NetworkAddressChanged -= value;
        }

        public INetworkInterface[] Interfaces()
        {
            try
            {
                return _origin.Interfaces();
            }
            catch (NetworkInformationException ex)
            {
                _logger.Error($"Failed to retrieve a list of system network interfaces: {ex.CombinedMessage()}");
                return new INetworkInterface[0];
            }
        }

        public INetworkInterface Interface(string interfaceDescription)
        {
            try
            {
                return _origin.Interface(interfaceDescription);
            }
            catch (NetworkInformationException ex)
            {
                _logger.Error($"Failed to retrieve a system network interface: {ex.CombinedMessage()}");
                return new NullNetworkInterface();
            }
        }

        public uint InterfaceIndex(string description, string hardwareId)
        {
            try
            {
                return _origin.InterfaceIndex(description, hardwareId);
            }
            catch (NetworkInformationException ex)
            {
                _logger.Error($"Failed to retrieve a system network interface: {ex.CombinedMessage()}");
                return 0;
            }
        }
    }
}