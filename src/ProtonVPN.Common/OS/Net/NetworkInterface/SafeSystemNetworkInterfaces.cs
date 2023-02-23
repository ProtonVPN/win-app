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
using System.Net;
using System.Net.NetworkInformation;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.NetworkLogs;

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

        public INetworkInterface[] GetInterfaces()
        {
            try
            {
                return _origin.GetInterfaces();
            }
            catch (NetworkInformationException ex)
            {
                _logger.Error<NetworkUnavailableLog>($"Failed to retrieve a list of system network interfaces: {ex.CombinedMessage()}");
                return new INetworkInterface[0];
            }
        }

        public INetworkInterface GetByDescription(string description)
        {
            return TryGet(() => _origin.GetByDescription(description));
        }

        public INetworkInterface GetByLocalAddress(IPAddress localAddress)
        {
            return TryGet(() => _origin.GetByLocalAddress(localAddress));
        }

        public INetworkInterface GetBestInterface(string hardwareIdToExclude)
        {
            return TryGet(() => _origin.GetBestInterface(hardwareIdToExclude));
        }

        private INetworkInterface TryGet(Func<INetworkInterface> func)
        {
            try
            {
                return func();
            }
            catch (NetworkInformationException ex)
            {
                _logger.Error<NetworkLog>($"Failed to retrieve a system network interface: {ex.CombinedMessage()}");
                return new NullNetworkInterface();
            }
        }

        public INetworkInterface GetByName(string name)
        {
            return TryGet(() => _origin.GetByName(name));
        }

        public INetworkInterface GetById(Guid id)
        {
            return TryGet(() => _origin.GetById(id));
        }
    }
}