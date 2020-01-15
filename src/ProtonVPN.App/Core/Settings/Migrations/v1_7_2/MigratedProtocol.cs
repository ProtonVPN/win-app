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

using ProtonVPN.Common.Extensions;
using ProtonVPN.Core.Profiles;

namespace ProtonVPN.Core.Settings.Migrations.v1_7_2
{
    internal class MigratedProtocol
    {
        private readonly string _protocol;

        public MigratedProtocol(string protocol)
        {
            _protocol = protocol;
        }

        public static implicit operator Protocol(MigratedProtocol item) => item.Value();

        public Protocol Value()
        {
            return _protocol?.EqualsIgnoringCase("udp") == true ? Protocol.OpenVpnUdp :
                   _protocol?.EqualsIgnoringCase("tcp") == true ? Protocol.OpenVpnTcp :
                   Protocol.Auto;
        }
    }
}
