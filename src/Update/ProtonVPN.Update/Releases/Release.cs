/*
 * Copyright (c) 2024 Proton AG
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
using System.Collections.Generic;
using ProtonVPN.Update.Responses;

namespace ProtonVPN.Update.Releases;

/// <summary>
/// A release of the app in the release history.
/// </summary>
public class Release : IRelease, IComparable, IComparable<IRelease>
{
    public static Release Empty => new Release();

    public Version Version { get; set; } = new(0, 0, 0);

    public IReadOnlyList<string> ChangeLog { get; set; } = new List<string>();

    public bool IsEarlyAccess { get; set; } = false;

    public bool IsNew { get; set; } = false;

    public FileResponse File { get; set; } = null;

    public DateTime? ReleaseDate { get; set; }

    public Release()
    { }

    public bool IsEmpty()
    {
        return Version.ToString() == "0.0.0" ||
               File == null ||
               string.IsNullOrEmpty(File.Url) ||
               string.IsNullOrEmpty(File.Sha512CheckSum);
    }

    public int CompareTo(IRelease other)
    {
        return Version.CompareTo(other.Version);
    }

    public int CompareTo(object obj)
    {
        return CompareTo((IRelease)obj);
    }
}