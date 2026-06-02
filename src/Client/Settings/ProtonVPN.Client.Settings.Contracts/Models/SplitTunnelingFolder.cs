/*
 * Copyright (c) 2026 Proton AG
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

public struct SplitTunnelingFolder : IEquatable<SplitTunnelingFolder>
{
    public string FolderPath { get; set; }

    public bool IsActive { get; set; }

    public SplitTunnelingFolder(string folderPath, bool isActive)
    {
        FolderPath = folderPath;
        IsActive = isActive;
    }

    public bool Equals(SplitTunnelingFolder other)
    {
        return string.Equals(FolderPath, other.FolderPath, StringComparison.OrdinalIgnoreCase)
            && IsActive == other.IsActive;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj?.GetType() != GetType())
        {
            return false;
        }
        return Equals((SplitTunnelingFolder)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(FolderPath?.ToUpperInvariant(), IsActive);
    }
}
