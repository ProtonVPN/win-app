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

using ProtonVPN.Core.Profiles.Comparers;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Specs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProtonVPN.Core.Profiles
{
    public class ProfileManager
    {
        private static readonly ProfileByIdEqualityComparer ProfileByIdEqualityComparer =
            new ProfileByIdEqualityComparer();
        private static readonly ProfileByNameEqualityComparer ProfileByNameEqualityComparer =
            new ProfileByNameEqualityComparer();

        private readonly ServerManager _serverManager;
        private readonly IProfileStorageAsync _profiles;

        public ProfileManager(ServerManager serverManager, IProfileStorageAsync profiles)
        {
            _serverManager = serverManager;
            _profiles = profiles;
        }

        public Task AddProfile(Profile profile)
        {
            AddCountryCode(profile);
            return _profiles.Create(profile);
        }

        public async Task<Profile> GetProfileById(string id)
        {
            var profiles = await GetProfiles();
            return profiles.FirstOrDefault(profile => profile.Id == id);
        }

        public async Task<IReadOnlyList<Profile>> GetProfiles()
        {
            var profiles = await _profiles.GetAll();
            foreach (var profile in profiles)
            {
                profile.Server = _serverManager.GetServer(new ServerById(profile.ServerId));
                profile.CountryCode = profile.CountryCode?.ToUpper() ?? profile.Server?.ExitCountry;
            }

            return profiles;
        }

        public async Task<Profile> GetFastestProfile()
        {
            var profiles = await GetProfiles();
            return profiles.FirstOrDefault(p => p.IsPredefined && p.Id == "Fastest");
        }

        public Task RemoveProfile(Profile profile)
        {
            return _profiles.Delete(profile);
        }

        public Task UpdateProfile(Profile profile)
        {
            AddCountryCode(profile);
            return _profiles.Update(profile);
        }

        public async Task<bool> ProfileWithNameExists(Profile profile)
        {
            var profiles = await GetProfiles();
            return profiles.Any(p => ProfileByNameEqualityComparer.Equals(p, profile));
        }

        public async Task<bool> OtherProfileWithNameExists(Profile profile)
        {
            var profiles = await GetProfiles();
            return profiles.Any(p => !ProfileByIdEqualityComparer.Equals(p, profile) && ProfileByNameEqualityComparer.Equals(p, profile));
        }

        private void AddCountryCode(Profile profile)
        {
            if (!string.IsNullOrEmpty(profile.CountryCode) || string.IsNullOrEmpty(profile.ServerId))
                return;

            var server = _serverManager.GetServer(new ServerById(profile.ServerId));
            profile.CountryCode = server?.ExitCountry;
        }
    }
}
