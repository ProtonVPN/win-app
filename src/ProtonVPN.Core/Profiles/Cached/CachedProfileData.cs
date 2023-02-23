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

using ProtonVPN.Common.Helpers;
using ProtonVPN.Core.Settings;
using System;
using System.Linq;

namespace ProtonVPN.Core.Profiles.Cached
{
    public class CachedProfileData : IDisposable
    {
        private IAppSettings _appSettings;
        private IDisposable _disposableLock;
        private CachedProfileDataContract _cached;

        public CachedProfileData(IAppSettings appSettings) : this(appSettings, null)
        { }

        public CachedProfileData(IAppSettings appSettings, IDisposable disposableLock)
        {
            Ensure.NotNull(appSettings, nameof(appSettings));

            _appSettings = appSettings;
            _disposableLock = disposableLock;

            _cached = appSettings.Profiles;
        }

        private CachedProfileList _local;
        public CachedProfileList Local => _local ?? (_local = new CachedProfileList(_cached.Local));

        private CachedProfileList _sync;
        public CachedProfileList Sync => _sync ?? (_sync = new CachedProfileList(_cached.Sync));

        private CachedProfileList _external;
        public CachedProfileList External => _external ?? (_external = new CachedProfileList(_cached.External));

        public bool HasChanges => _local?.HasChanges == true || _sync?.HasChanges == true || _external?.HasChanges == true;

        private void Save()
        {
            _appSettings.Profiles = new CachedProfileDataContract
            {
                Local = _local?.HasChanges == true ? _local.ToList() : _cached.Local,
                Sync = _sync?.HasChanges == true ? _sync.ToList() : _cached.Sync,
                External = _external?.HasChanges == true ? _external.ToList() : _cached.External
            };

            _local = null;
            _sync = null;
            _external = null;
        }

        public void Dispose()
        {
            if (HasChanges)
            {
                if (_disposableLock == null) throw new InvalidOperationException("CachedProfileData mut be locked to support data changes");
                Save();
            }

            _appSettings = null;
            _disposableLock?.Dispose();
            _disposableLock = null;
            _cached = null;
        }
    }
}
