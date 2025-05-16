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

using System;
using System.IO;

namespace ProtonVPN.UI.Tests.TestsHelper;

public static class TestConstants
{
    public static TimeSpan? DefaultElementWaitingTime => TimeSpan.FromSeconds(10);
    public static TimeSpan ApiRetryInterval => TimeSpan.FromSeconds(3);
    public static TimeSpan OneSecondTimeout => TimeSpan.FromSeconds(1);
    public static TimeSpan FiveSecondsTimeout => TimeSpan.FromSeconds(5);
    public static TimeSpan TenSecondsTimeout => TimeSpan.FromSeconds(10);
    public static TimeSpan ThirtySecondsTimeout => TimeSpan.FromSeconds(30);
    public static TimeSpan OneMinuteTimeout => TimeSpan.FromSeconds(60);
    public static TimeSpan TwoMinutesTimeout => TimeSpan.FromMinutes(2);
    public static TimeSpan RetryInterval => TimeSpan.FromMilliseconds(200);
    public static TimeSpan AnimationDelay => TimeSpan.FromMilliseconds(500);

    public static string AppFolderPath = @"C:\Program Files\Proton\VPN";
    public static string LauncherPath = @"C:\Program Files\Proton\VPN\ProtonVPN.Launcher.exe";
    public static string MapCountry = "CA";
    public static string ClientLogsPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Proton\Proton VPN\Logs\client-logs.txt");
    public static string UserStoragePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Proton\Proton VPN");
    public enum Protocol
    {
        WireGuardUdp,
        OpenVpnUdp,
        WireGuardTcp,
        OpenVpnTcp,
        WireGuardTls,
    }

    public enum VpnConnectionOptions
    {
        Fast,
        Random,
        Last,
        Customized
    }
}