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

public enum WlanIntfOpcode
{
    AutoconfEnabled = 1,
    BackgroundScanEnabled = 2,
    BssType = 5,
    ChannelNumber = 8,
    CurrentConnection = 7,
    CurrentOperationMode = 12,
    IhvEnd = 0x3fffffff,
    IhvStart = 0x30000000,
    InterfaceState = 6,
    MediaStreamingMode = 3,
    RadioState = 4,
    Rssi = 0x10000102,
    SecurityEnd = 0x2fffffff,
    SecurityStart = 0x20010000,
    Statistics = 0x10000101,
    SupportedAdhocAuthCipherPairs = 10,
    SupportedCountryOrRegionStringList = 11,
    SupportedInfrastructureAuthCipherPairs = 9
}