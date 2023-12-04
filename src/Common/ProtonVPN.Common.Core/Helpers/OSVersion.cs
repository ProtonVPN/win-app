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
}
