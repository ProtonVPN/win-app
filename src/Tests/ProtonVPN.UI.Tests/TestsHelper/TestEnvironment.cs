/*
 * Copyright (c) 2025 Proton AG
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
using ProtonVPN.UI.Tests.TestBase;

namespace ProtonVPN.UI.Tests.TestsHelper;

public class TestEnvironment : BaseTest
{
    private const string REGISTRY_KEY_PATH = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Proton VPN_is1";

    public static string GetAppVersion()
    {
        using (RegistryKey? key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(REGISTRY_KEY_PATH))
        {
            object? displayVersionObject = key?.GetValue("DisplayVersion");
            return displayVersionObject?.ToString() ?? string.Empty;
        }
    }

    public static bool AreTestsRunningLocally()
    {
        bool isLocalEnvironment = false;
        string ciCommitHash = Environment.GetEnvironmentVariable("CI_COMMIT_SHA") ?? throw new Exception("Missing CI_COMMIT_SHA env var.");
        if (string.IsNullOrEmpty(ciCommitHash))
        {
            isLocalEnvironment = true;
        }
        return isLocalEnvironment;
    }

    public static string GetCommitHash()
    {
        FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(TestConstants.LauncherPath);
        string? version = fileVersionInfo.ProductVersion;
        return version?.Split("-").Last() ?? throw new Exception("Failed to get commit hash.");
    }

    public static string GetOperatingSystem()
    {
        return $"Windows {Environment.OSVersion.Version.Major}";
    }

    public static string GetProtonClientFolder()
    {
        string versionFolder = $"v{GetAppVersion()}";
        return Path.Combine(TestConstants.AppFolderPath, versionFolder);
    }

    public static string GetDevProtonClientFolder()
    {
        string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            ?? throw new Exception("Failed to get executing assembly location.");
        return Directory.GetParent(directoryName)?.ToString()
            ?? throw new Exception("Failed to get client folder.");
    }

    public static string GetServiceLogsPath()
    {
        return Path.Combine(GetProtonClientFolder(), "ServiceData", "Logs", "service-logs.txt");
    }
}