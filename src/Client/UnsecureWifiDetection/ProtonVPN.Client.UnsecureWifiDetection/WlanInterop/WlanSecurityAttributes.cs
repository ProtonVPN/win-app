﻿/*
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

using System.Runtime.InteropServices;

namespace ProtonVPN.Client.UnsecureWifiDetection.WlanInterop;

[StructLayout(LayoutKind.Sequential)]
public struct WlanSecurityAttributes
{
    [MarshalAs(UnmanagedType.Bool)]
    public readonly bool securityEnabled;
    [MarshalAs(UnmanagedType.Bool)]
    private readonly bool oneXEnabled;
    private readonly Dot11AuthAlgorithm dot11AuthAlgorithm;
    private readonly Dot11CipherAlgorithm dot11CipherAlgorithm;
}