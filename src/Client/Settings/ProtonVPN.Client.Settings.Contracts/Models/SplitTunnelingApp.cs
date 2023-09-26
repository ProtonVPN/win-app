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

using System.Diagnostics.CodeAnalysis;

namespace ProtonVPN.Client.Settings.Contracts.Models;

public struct SplitTunnelingApp : IEquatable<SplitTunnelingApp>
{
    public string AppFilePath { get; set; }

    public List<string> AlternateAppFilePaths { get; set; }

    public bool IsActive { get; set; }

    public SplitTunnelingApp(string appFilePath, bool isActive)
    {
        AppFilePath = appFilePath;
        IsActive = isActive;
        AlternateAppFilePaths = new List<string>();
    }

    public bool Equals(SplitTunnelingApp other)
    {
        return string.Equals(AppFilePath, other.AppFilePath, StringComparison.OrdinalIgnoreCase)
            && IsActive == other.IsActive;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj?.GetType() != GetType())
        {
            return false;
        }
        return Equals((SplitTunnelingApp)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(AppFilePath?.ToUpperInvariant(), IsActive);
    }

    public List<string> GetAllAppFilePaths()
    {
        List<string> paths = new()
        {
            AppFilePath
        };

        if (AlternateAppFilePaths.Any())
        {
            paths.AddRange(AlternateAppFilePaths);
        }

        return paths;
    }
}