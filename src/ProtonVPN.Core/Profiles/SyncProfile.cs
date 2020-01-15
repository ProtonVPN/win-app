/*
 * Copyright (c) 2020 Proton Technologies AG
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

using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Core.Profiles.Cached;
using ProtonVPN.Core.Profiles.Comparers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Configuration;

namespace ProtonVPN.Core.Profiles
{
    public class SyncProfile
    {
        private static readonly ProfileByIdEqualityComparer ProfileByIdEqualityComparer =
            new ProfileByIdEqualityComparer();
        private static readonly ProfileByNameEqualityComparer ProfileByNameEqualityComparer =
            new ProfileByNameEqualityComparer();

        private readonly ILogger _logger;
        private readonly CachedProfiles _cachedProfiles;
        private readonly IProfileStorageAsync _apiProfiles;
        private readonly Config _appConfig;

        private CancellationToken _cancellationToken;
        private bool _failed;

        public SyncProfile(Config appConfig, ILogger logger, CachedProfiles cachedProfiles, ApiProfiles apiProfiles) :
            this(appConfig, logger, cachedProfiles, new NullSafeProfileStorage(apiProfiles))
        { }

        private SyncProfile(Config appConfig, ILogger logger, CachedProfiles cachedProfiles, IProfileStorageAsync apiProfiles)
        {
            _appConfig = appConfig;
            _logger = logger;
            _cachedProfiles = cachedProfiles;
            _apiProfiles = apiProfiles;
        }

        public bool Succeeded => !_failed;

        public string ErrorMessage { get; private set; }

        public async Task Sync(Profile profile, CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            _failed = false;
            ErrorMessage = null;

            switch (profile.Status)
            {
                case ProfileStatus.Created:
                    await SyncCreated(profile);
                    break;
                case ProfileStatus.Updated:
                    await SyncUpdated(profile);
                    break;
                case ProfileStatus.Deleted:
                    await SyncDeleted(profile);
                    break;
                default:
                    await Skip(profile);
                    break;
            }
        }

        public Profile WithUniqueName(Profile profile)
        {
            if (profile == null) return null;

            using (var cached = _cachedProfiles.ProfileData())
            {
                var local = cached.Local.Where(x => x.Status != ProfileStatus.Deleted).ToList();
                var sync = cached.Local.Where(x => x.Status != ProfileStatus.Deleted).ToList();
                var p = profile.WithUniqueNameCandidate(_appConfig.MaxProfileNameLength);
                while (ContainsOtherWithSameName(local, p) ||
                       ContainsOtherWithSameName(sync, p) ||
                       ContainsOtherWithSameName(cached.External, p)) 
                {
                    p = p.WithNextUniqueNameCandidate(_appConfig.MaxProfileNameLength); 
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

        private async Task SyncCreated(Profile profile)
        {
            if (External(profile).Exists())
            {
                // This should not happen
                await Skip(profile);
                return;
            }

            if (!profile.IsValid())
            {
                _logger.Warn($"Profile \"{profile.Name}\" is not valid! Removing.");
                await Skip(profile);
                return;
            }

            profile = WithUniqueName(profile);
            profile = await CreatedInApi(profile);
            await FinishSyncCreated(profile);
        }

        private async Task SyncUpdated(Profile profile)
        {
            var external = External(profile);
            if (!external.Exists())
            {
                if (profile.HasElapsed(_appConfig.ForcedProfileSyncInterval))
                    // First wins: Profile was deleted while editing or syncing
                    await Skip(profile);
                else
                    // Last wins: Creating new profile
                    await SyncCreated(profile);

                return;
            }

            var p = profile.WithExternalIdFrom(external);

            if (p.HasElapsed(_appConfig.ForcedProfileSyncInterval) && external.ModifiedLaterThan(p))
            {
                // First wins: Profile was updated while syncing
                await FinishSyncOverridden(p);

                return;
            }

            // Last wins
            p = WithUniqueName(p);
            p = await UpdatedInApi(p);

            await FinishSyncUpdated(p);
        }

        private async Task SyncDeleted(Profile profile)
        {
            var external = External(profile);

            if (profile.HasElapsed(_appConfig.ForcedProfileSyncInterval) && external.ModifiedLaterThan(profile))
            {
                // First wins: Profile was updated while syncing
                await FinishSyncOverridden(profile);
                return;
            }

            // Last wins
            await DeleteInApi(external);
            await FinishSyncDeleted(profile);
        }

        private Profile External(Profile profile)
        {
            using (var cached = _cachedProfiles.ProfileData())
            {
                return cached.External.Get(profile);
            }
        }

        private async Task<Profile> CreatedInApi(Profile profile)
        {
            if (profile == null) return null;

            try
            {
                await _apiProfiles.Create(profile);
                _cancellationToken.ThrowIfCancellationRequested();
            }
            catch (ProfileException ex)
            {
                await HandleException(ex, profile);

                return null;
            }

            return profile;
        }

        private async Task<Profile> UpdatedInApi(Profile profile)
        {
            if (profile == null) return null;

            try
            {
                await _apiProfiles.Update(profile);
                _cancellationToken.ThrowIfCancellationRequested();
            }
            catch (ProfileException ex)
            {
                await HandleException(ex, profile);

                return profile;
            }

            return profile;
        }

        private async Task DeleteInApi(Profile profile)
        {
            if (profile == null) return;

            try
            {
                await _apiProfiles.Delete(profile);
                _cancellationToken.ThrowIfCancellationRequested();
            }
            catch (ProfileException ex)
            {
                _logger.Error(ex.CombinedMessage());

                if (ex.Error != ProfileError.NotFound)
                {
                    _failed = true;
                    ErrorMessage = ex.Message;
                }
            }
        }

        private async Task Skip(Profile profile)
        {
            using (var cached = await _cachedProfiles.LockedProfileData())
            {
                cached.Sync.Remove(profile);
            }
        }

        private async Task FinishSyncCreated(Profile profile)
        {
            if (profile == null) return;

            using (var cached = await _cachedProfiles.LockedProfileData())
            {
                var p = profile
                    .WithStatus(ProfileStatus.Synced)
                    .WithSyncStatus(ProfileSyncStatus.Succeeded);

                cached.External.AddOrReplace(p);
                cached.Sync.Remove(profile);
            }
        }

        private async Task FinishSyncUpdated(Profile profile)
        {
            if (profile == null) return;

            using (var cached = await _cachedProfiles.LockedProfileData())
            {
                var p = profile
                    .WithStatus(ProfileStatus.Synced)
                    .WithSyncStatus(ProfileSyncStatus.Succeeded);

                cached.External.AddOrReplace(p);
                cached.Sync.Remove(profile);
            }
        }

        private async Task FinishSyncDeleted(Profile profile)
        {
            if (profile == null) return;

            using (var cached = await _cachedProfiles.LockedProfileData())
            {
                cached.External.Remove(profile);
                cached.Sync.Remove(profile);
            }
        }

        private async Task FinishSyncOverridden(Profile profile)
        {
            if (profile == null) return;

            using (var cached = await _cachedProfiles.LockedProfileData())
            {
                var external = cached.External.Get(profile);
                if (external == null)
                    return;

                var p = external
                    .WithStatus(ProfileStatus.Synced)
                    .WithSyncStatus(ProfileSyncStatus.Overridden);

                cached.External.AddOrReplace(p);
                cached.Sync.Remove(profile);
            }
        }

        private async Task HandleException(ProfileException ex, Profile profile)
        {
            _logger.Error(ex.CombinedMessage());

            if (ex.Error == ProfileError.NameConflict || ex.Error == ProfileError.Other)
            {
                using (var cached = await _cachedProfiles.LockedProfileData())
                {
                    var p = profile;

                    if (ex.Error == ProfileError.Other)
                    {
                        p = p.WithSyncStatus(ProfileSyncStatus.Failed);
                    }

                    if (ex.Error == ProfileError.NameConflict)
                    {
                        p = p.WithNextUniqueNameCandidate(_appConfig.MaxProfileNameLength);
                    }

                    cached.Sync.AddOrReplace(p);
                }
            }
            else
            {
                _failed = true;
                ErrorMessage = ex.Message;
            }
        }
    }
}
