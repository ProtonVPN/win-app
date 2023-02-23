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

using System.Runtime.InteropServices;

namespace ProtonVPN.Common.OS.Net.Routing
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MibIPForwardRow
    {
        internal uint DwForwardDest;
        internal uint DwForwardMask;
        internal uint DwForwardPolicy;
        internal uint DwForwardNextHop;
        internal uint DwForwardIfIndex;
        internal uint DwForwardType;
        internal uint DwForwardProto;
        internal uint DwForwardAge;
        internal uint DwForwardNextHopAS;
        internal uint DwForwardMetric1;
        internal uint DwForwardMetric2;
        internal uint DwForwardMetric3;
        internal uint DwForwardMetric4;
        internal uint DwForwardMetric5;
    }
}