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

using System.Collections;
using System.Collections.Generic;
using ProtonVPN.Common.Configuration;

namespace ProtonVPN.Vpn.OpenVpn.Arguments
{
    public class ManagementArguments : IEnumerable<string>
    {
        private readonly OpenVpnConfig _config;
        private readonly int _managementPort;

        public ManagementArguments(OpenVpnConfig config, int managementPort)
        {
            _config = config;
            _managementPort = managementPort;
        }

        public IEnumerator<string> GetEnumerator()
        {
            yield return $"--management {_config.ManagementHost} {_managementPort} stdin";
            yield return "--management-query-passwords";
            yield return "--management-hold";
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
