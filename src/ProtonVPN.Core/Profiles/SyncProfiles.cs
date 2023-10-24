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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProtonVPN.Core.Profiles.Cached;
using ProtonVPN.Core.Profiles.Comparers;

namespace ProtonVPN.Core.Profiles;

public class SyncProfiles : IProfileStorageAsync
{
    private const int MAX_PROFILE_NAME_LENGTH = 25;

    private static readonly ProfileByIdEqualityComparer ProfileByIdEqualityComparer = new();
    private static readonly ProfileByNameEqualityComparer ProfileByNameEqualityComparer = new();

    private readonly IProfileStorageAsync _profiles;
    private readonly CachedProfiles _cachedProfiles;

    public SyncProfiles(
        Profiles profiles,
        CachedProfiles cachedProfiles)
    {
        _profiles = profiles;
        _cachedProfiles = cachedProfiles;
    }

    public Task<IReadOnlyList<Profile>> GetAll()
    {
        return _profiles.GetAll();
    }

    public async Task Create(Profile profile)
    {
        // Toggle profile on QuickConnectViewModel is not checking for profile name duplicates.
        // Ensure new profile name shown to the user is initially adjusted for uniqueness.
        Profile p = EnsureUniqueName(profile);

        await _profiles.Create(p);
    }

    public Profile EnsureUniqueName(Profile profile)
    {
        if (profile == null)
        {
            return null;
        }

        using (CachedProfileData cached = _cachedProfiles.ProfileData())
        {
            List<Profile> local = cached.Local.Where(x => x.Status != ProfileStatus.Deleted).ToList();
            Profile p = profile.WithUniqueNameCandidate(MAX_PROFILE_NAME_LENGTH);
            while (ContainsOtherWithSameName(local, p) ||
                   ContainsOtherWithSameName(cached.External, p))
            {
                p = p.WithNextUniqueNameCandidate(MAX_PROFILE_NAME_LENGTH);
            }

            return p;
        }
    }

    private bool ContainsOtherWithSameName(IEnumerable<Profile> profiles, Profile profile)
    {
        return profiles.Any(x =>
            !ProfileByIdEqualityComparer.Equals(x, profile) &&
            ProfileByNameEqualityComparer.Equals(x, profile));
    }

    public async Task Update(Profile profile)
    {
        await _profiles.Update(profile);
    }

    public async Task Delete(Profile profile)
    {
        await _profiles.Delete(profile);
    }
}