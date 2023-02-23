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

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProtonVPN.Core.Profiles.Cached
{
    public class CachedProfileList : IEnumerable<Profile>
    {
        private readonly Dictionary<string, Profile> _profiles;

        public CachedProfileList(IEnumerable<Profile> profiles) : this(profiles.ToDictionary(p => p.Id, p => p))
        { }

        private CachedProfileList(Dictionary<string, Profile> profiles)
        {
            _profiles = profiles;
        }

        #region IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<Profile> GetEnumerator()
        {
            return _profiles.Values.GetEnumerator();
        }

        #endregion

        public Profile Get(Profile profile)
        {
            if (profile == null)
                return null;

            return _profiles.TryGetValue(profile.Id, out var result) ? result : null;
        }

        public bool Any()
        {
            return _profiles.Any();
        }

        public void Add(Profile profile)
        {
            if (profile == null)
                return;

            _profiles.Add(profile.Id, profile);
            HasChanges = true;
        }

        public void AddOrReplace(Profile profile)
        {
            if (profile == null)
                return;

            _profiles[profile.Id] = profile;
            HasChanges = true;
        }

        public void Remove(Profile profile)
        {
            if (profile == null)
                return;

            if (_profiles.Remove(profile.Id))
                HasChanges = true;
        }

        public void Clear()
        {
            if (!_profiles.Any())
                return;

            _profiles.Clear();
            HasChanges = true;
        }

        public bool HasChanges { get; private set; }
    }
}
