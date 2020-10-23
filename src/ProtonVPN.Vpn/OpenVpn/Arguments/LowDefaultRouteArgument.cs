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

using System.Collections;
using System.Collections.Generic;

namespace ProtonVPN.Vpn.OpenVpn.Arguments
{
    internal class LowDefaultRouteArgument : IEnumerable<string>
    {
        public IEnumerator<string> GetEnumerator()
        {
            yield return "--pull-filter ignore \"redirect-gateway\"";
            yield return "--route 0.0.0.0 0.0.0.0 vpn_gateway 32000";
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
