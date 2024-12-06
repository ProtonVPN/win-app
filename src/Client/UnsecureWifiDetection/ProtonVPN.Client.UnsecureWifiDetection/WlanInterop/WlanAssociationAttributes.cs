/*
 * Copyright (c) 2024 Proton AG
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

using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace ProtonVPN.Client.UnsecureWifiDetection.WlanInterop;

[StructLayout(LayoutKind.Sequential)]
public struct WlanAssociationAttributes
{
    private readonly Dot11Ssid dot11Ssid;
    private readonly Dot11BssType dot11BssType;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
    private readonly byte[] dot11Bssid;
    private readonly Dot11PhyType dot11PhyType;
    private readonly uint dot11PhyIndex;
    private readonly uint wlanSignalQuality;
    private readonly uint rxRate;
    private readonly uint txRate;

    public PhysicalAddress Dot11Bssid
    {
        get => dot11Bssid != null
            ? new PhysicalAddress(dot11Bssid)
            : new PhysicalAddress([0, 0, 0, 0, 0, 0]);
    }
}