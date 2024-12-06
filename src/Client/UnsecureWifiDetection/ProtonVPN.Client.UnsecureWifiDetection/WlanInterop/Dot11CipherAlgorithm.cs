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

namespace ProtonVPN.Client.UnsecureWifiDetection.WlanInterop;

public enum Dot11CipherAlgorithm : uint
{
    CCMP = 4,
    IHV_End = 0xffffffff,
    IHV_Start = 0x80000000,
    None = 0,
    RSN_UseGroup = 0x100,
    TKIP = 2,
    WEP = 0x101,
    WEP104 = 5,
    WEP40 = 1,
    WPA_UseGroup = 0x100
}