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

public enum Dot11PhyType : uint
{
    Any = 0,
    Dsss = 2,
    Erp = 6,
    Fhss = 1,
    Hrdsss = 5,
    Ht = 7,
    IhvEnd = 0xffffffff,
    IhvStart = 0x80000000,
    IrBaseband = 3,
    Ofdm = 4,
    Unknown = 0
}