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
using ProtonVPN.Common.Networking;

namespace ProtonVPN.Vpn.OpenVpn.Arguments
{
    internal class NetworkDriverArgument : IEnumerable<string>
    {
        private readonly string _interfaceGuid;
        private readonly OpenVpnAdapter _openVpnAdapter;

        public NetworkDriverArgument(string interfaceGuid, OpenVpnAdapter openVpnAdapter)
        {
            _openVpnAdapter = openVpnAdapter;
            _interfaceGuid = interfaceGuid;
        }

        public IEnumerator<string> GetEnumerator()
        {
            yield return $"--dev-node \"{_interfaceGuid}\"";

            if (_openVpnAdapter == OpenVpnAdapter.Tun)
            {
                yield return "--windows-driver wintun";
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}