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

namespace ProtonVPN.UI.Tests.TestsHelper
{
    public static class TestConstants
    {
        public static TimeSpan VeryShortTimeout => TimeSpan.FromSeconds(5);
        public static TimeSpan ShortTimeout => TimeSpan.FromSeconds(10);
        public static TimeSpan MediumTimeout => TimeSpan.FromSeconds(30);
        public static TimeSpan LongTimeout => TimeSpan.FromSeconds(60);
        public static string ProfileName => "@AutomationProfile";
        public static string AppFolderPath => @"C:\Program Files (x86)\Proton Technologies\ProtonVPN";
        public static TimeSpan RetryInterval => TimeSpan.FromMilliseconds(500);
        public static string MapCountry => "US";
        public static string PathToRecorder => @"C:\TestRecorder\ffmpeg.exe";
        public static string AppLogsPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"ProtonVPN\Logs\app-logs.txt");
        public static string ServiceLogsPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"ProtonVPN\Logs\service-logs.txt");
    }
}
