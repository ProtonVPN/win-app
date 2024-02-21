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
    public static TimeSpan StartupDelay => TimeSpan.FromMilliseconds(500);
    public static TimeSpan DefaultAnimationDelay => TimeSpan.FromMilliseconds(200);
    public static TimeSpan DefaultNavigationDelay => TimeSpan.FromSeconds(1);
    public static TimeSpan InitializationDelay => TimeSpan.FromSeconds(2);
    public static TimeSpan ConnectionDelay => TimeSpan.Zero;
    public static TimeSpan DisconnectionDelay => TimeSpan.Zero;
    public static TimeSpan VeryShortTimeout => TimeSpan.FromSeconds(5);
    public static TimeSpan ShortTimeout => TimeSpan.FromSeconds(10);
    public static TimeSpan MediumTimeout => TimeSpan.FromSeconds(30);
    public static TimeSpan LongTimeout => TimeSpan.FromSeconds(60);
    public static TimeSpan RetryInterval => TimeSpan.FromMilliseconds(500);
    public static string AppFolderPath = @"C:\Program Files\Proton\VPN";
    public static string MapCountry = "CA";
    public static string PathToRecorder = @"C:\TestRecorder\ffmpeg.exe";
    public static string ClientLogsPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Proton\Proton VPN\Logs\client-logs.txt");
    public static string UserStoragePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Proton\Proton VPN");
    public enum Protocol
    {
        Wireguard,
        OpenVpnUdp,
        OpenVpnTcp
    }
}