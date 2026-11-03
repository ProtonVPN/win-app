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

using Microsoft.Win32;

namespace ProtonVPN.Common.Core.Helpers;

public static class OSVersion
{
    private const string CURRENT_VERSION_REGISTRY_PATH = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";

    private static readonly Lazy<Version> _version = new(CreateVersion);
    private static readonly Lazy<string> _versionString = new(_version.Value.ToString);
    private static readonly Lazy<string> _platformVersionString = new(CreatePlatformVersionString);

    public static readonly Version Windows11Version = new(10, 0, 22000); // Windows 11 21H2
    public static readonly Version TaskbarBadgeMinimumWindowsVersion = new(6, 1, 7600); // Windows 7 RTM
    public static readonly Version EfficiencyModeMinimumWindowsVersion = new(10, 0, 16299); // Windows 10 1709 (Fall Creators)

    private static Version CreateVersion()
    {
        // Environment.OSVersion is affected by compatibility mode (e.g. reports Windows Vista when the app
        // is set to run in Vista compatibility mode), while the registry values used here are not.
        return GetVersionFromRegistry() ?? Environment.OSVersion.Version;
    }

    private static Version? GetVersionFromRegistry()
    {
        try
        {
            using RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using RegistryKey? key = baseKey.OpenSubKey(CURRENT_VERSION_REGISTRY_PATH);
            if (key?.GetValue("CurrentMajorVersionNumber") is int major &&
                key.GetValue("CurrentMinorVersionNumber") is int minor &&
                // CurrentBuildNumber is not used because compatibility mode also changes it (e.g. 6002 for Vista SP2)
                int.TryParse(key.GetValue("CurrentBuild") as string, out int build))
            {
                return new Version(major, minor, build, 0);
            }
        }
        catch
        {
        }

        return null;
    }

    private static string CreatePlatformVersionString()
    {
        return $"Microsoft Windows NT {_version.Value}";
    }

    /// <summary>Returns the OS version in the format: 10.0.19045.0</summary>
    public static Version Get()
    {
        return _version.Value;
    }

    /// <summary>Returns the OS version in the format: 10.0.19045.0</summary>
    public static string GetString()
    {
        return _versionString.Value;
    }

    /// <summary>Returns the OS version in the format: Microsoft Windows NT 10.0.19045.0</summary>
    public static string GetPlatformString()
    {
        return _platformVersionString.Value;
    }

    public static bool IsWindows11OrHigher()
    {
        return IsOrHigherThan(Windows11Version);
    }

    public static bool IsOrHigherThan(Version version)
    {
        return _version.Value >= version;
    }
}