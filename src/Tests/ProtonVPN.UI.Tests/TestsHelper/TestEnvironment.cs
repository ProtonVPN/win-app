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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Win32;
using ProtonVPN.Common.Core.Helpers;

namespace ProtonVPN.UI.Tests.TestsHelper;

public class TestEnvironment : TestSession
{
    public static string GetAppVersion()
    {
        string registryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Proton VPN_is1";
        RegistryKey localMachineRegistry = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        RegistryKey key = localMachineRegistry.OpenSubKey(registryKeyPath);

        object displayVersionObject = key?.GetValue("DisplayVersion");
        return displayVersionObject?.ToString();
    }

    public static bool AreTestsRunningLocally()
    {
        bool isLocalEnvironment = false;
        string ciCommitHash = Environment.GetEnvironmentVariable("CI_COMMIT_SHA");
        if (string.IsNullOrEmpty(ciCommitHash))
        {
            isLocalEnvironment = true;
        }
        return isLocalEnvironment;
    }

    public static string GetCommitHash()
    {
        FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(TestConstants.LauncherPath);
        string version = fileVersionInfo.ProductVersion;
        return version.Split("-").Last();
    }

    public static bool IsVideoRecorderPresent()
    {
        return File.Exists(TestConstants.PathToRecorder);
    }

    public static int GetOperatingSystemMajorVersion()
    {
        return Environment.OSVersion.Version.Major;
    }

    public static string GetProtonClientFolder()
    {
        string versionFolder = $"v{GetAppVersion()}";
        return Path.Combine(TestConstants.AppFolderPath, versionFolder);
    }

    public static string GetServiceLogsPath()
    {
        return Path.Combine(GetProtonClientFolder(), "ServiceData", "Logs", "service-logs.txt");
    }
}