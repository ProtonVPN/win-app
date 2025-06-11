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

namespace ProtonVPN.Common.Core.Helpers;

public static class OSVersion
{
    private static readonly Lazy<Version> _version = new(CreateVersion);
    private static readonly Lazy<string> _versionString = new(_version.Value.ToString);
    private static readonly Lazy<string> _platformVersionString = new(CreatePlatformVersionString);

    public static readonly Version Windows11Version = new(10, 0, 22000); // Windows 11 21H2
    public static readonly Version TaskbarBadgeMinimumWindowsVersion = new(6, 1, 7600); // Windows 7 RTM
    public static readonly Version EfficiencyModeMinimumWindowsVersion = new(10, 0, 16299); // Windows 10 1709 (Fall Creators)

    private static Version CreateVersion()
    {
        return Environment.OSVersion.Version;
    }

    private static string CreatePlatformVersionString()
    {
        return Environment.OSVersion.VersionString;
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
        return Environment.OSVersion.Version >= version;
    }
}