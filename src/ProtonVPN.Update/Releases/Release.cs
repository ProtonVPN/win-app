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
using System.Collections.Generic;
using ProtonVPN.Update.Responses;

namespace ProtonVPN.Update.Releases
{
    /// <summary>
    /// A release of the app in the release history.
    /// </summary>
    public class Release : IRelease, IComparable, IComparable<IRelease>
    {
        private readonly ReleaseResponse _release;

        public Release(ReleaseResponse release, bool earlyAccess, Version currentVersion)
        {
            _release = release;
            Version = Version.Parse(_release.Version);
            EarlyAccess = earlyAccess;
            New = Version > currentVersion;
            DisableAutoUpdate = release.DisableAutoUpdate;
        }

        public Version Version { get; }

        public IReadOnlyList<string> ChangeLog => _release.ChangeLog;

        public bool EarlyAccess { get; }

        public bool New { get; }

        public bool DisableAutoUpdate { get; }

        public FileResponse File => _release.File;

        public bool Empty()
        {
            return _release.Version == "0.0.0" ||
                   _release.File == null ||
                   string.IsNullOrEmpty(_release.File.Url) || 
                   string.IsNullOrEmpty(_release.File.Sha512CheckSum);
        }

        public static Release EmptyRelease()
        {
            return new Release(new ReleaseResponse { Version = "0.0.0" }, false, new Version(0, 0, 0));
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
}
