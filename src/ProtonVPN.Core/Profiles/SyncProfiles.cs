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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Profiles.Cached;
using ProtonVPN.Core.Profiles.Comparers;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Core.Profiles
{
    public class SyncProfiles : IProfileStorageAsync, ISyncProfileStorage, ILoggedInAware, ILogoutAware
    {
        private const int NumberOfSyncRetries = 3; 

        private static readonly ProfileByExternalIdEqualityComparer ProfileByExternalIdEqualityComparer =
            new ProfileByExternalIdEqualityComparer();
        private static readonly ProfileByEssentialPropertiesEqualityComparer ProfileByEssentialPropertiesEqualityComparer =
            new ProfileByEssentialPropertiesEqualityComparer();
        private static readonly ProfileByPropertiesEqualityComparer ProfileByPropertiesEqualityComparer =
            new ProfileByPropertiesEqualityComparer();

        private readonly ILogger _logger;
        private readonly IAppSettings _appSettings;
        private readonly IProfileStorageAsync _profiles;
        private readonly CachedProfiles _cachedProfiles;
        private readonly IProfileStorageAsync _apiProfiles;
        private readonly SyncProfile _syncProfile;

        private readonly System.Timers.Timer _timer;
        private readonly CoalescingAction _syncAction;

        private volatile bool _loggedIn;
        private CancellationToken _cancellationToken;

        private bool _syncFailed;
        private string _syncErrorMessage;
        private bool _changesSynced;
        private DateTime _lastSyncAt = DateTime.MinValue;

        private ProfileSyncStatus _syncStatus = ProfileSyncStatus.Succeeded;

        public SyncProfiles(
            Common.Configuration.Config appConfig,
            ILogger logger,
            IAppSettings appSettings,
            Profiles profiles,
            CachedProfiles cachedProfiles,
            ApiProfiles apiProfiles,
            SyncProfile syncProfile)
        {
            _appConfig = appConfig;
            _logger = logger;
            _appSettings = appSettings;
            _profiles = profiles;
            _cachedProfiles = cachedProfiles;
            _apiProfiles = apiProfiles;
            _syncProfile = syncProfile;

            _syncAction = new CoalescingAction(SyncAction);
            _syncAction.Completed += OnSyncCompleted;

            _timer = new System.Timers.Timer
            {
                Interval = _appConfig.ProfileSyncTimerPeriod.RandomizedWithDeviation(0.2).TotalMilliseconds,
                AutoReset = true
            };
            _timer.Elapsed += (s, e) => OnTimerElapsed();
        }

        public Task<IReadOnlyList<Profile>> GetAll()
        {
            return _profiles.GetAll();
        }

        public async Task Create(Profile profile)
        {
            // Toggle profile on QuickConnectViewModel is not checking for profile name duplicates.
            // Ensure new profile name shown to the user is initially adjusted for uniqueness.
            Profile p = _syncProfile.WithUniqueName(profile);

            await _profiles.Create(p);
            Sync();
        }

        public async Task Update(Profile profile)
        {
            await _profiles.Update(profile);
            Sync();
        }

        public async Task Delete(Profile profile)
        {
            await _profiles.Delete(profile);
            Sync();
        }

        public event EventHandler<ProfileSyncStatusChangedEventArgs> SyncStatusChanged;

        public void OnUserLoggedIn()
        {
            _loggedIn = true;
            _syncAction.Cancel();
            _timer.Start();
        }

        public void OnUserLoggedOut()
        {
            _loggedIn = false;
            _timer.Stop();
            _syncAction.Cancel();
        }

        public void Sync()
        {
            if (!_loggedIn)
                return;

            OnSyncStatusChanged(ProfileSyncStatus.InProgress);
            _syncAction.Run();
        }

        private async Task SyncAction(CancellationToken cancellationToken)
        {
            _logger.Info("Sync profiles requested");
            _cancellationToken = cancellationToken;

            await Retry(async () =>
                {
                    _syncFailed = false;
                    _syncErrorMessage = null;
                    _changesSynced = false;

                    await MergeApiToExternal();
                    await MergeLocalToSync();
                    await MergeSyncToApi();
                },
                () => !_syncFailed && ContainsNotSyncedData(),
                NumberOfSyncRetries);

            _lastSyncAt = DateTime.UtcNow;

            if (!_syncFailed && _changesSynced)
                ChangesSyncedAt = _lastSyncAt;
        }

        private bool ContainsNotSyncedData()
        {
            using (CachedProfileData cached = _cachedProfiles.ProfileData())
            {
                return cached.Sync.Any();
            }
        }

        private void OnSyncCompleted(object sender, TaskCompletedEventArgs e)
        {
            if (e.Task.IsFaulted)
            {
                OnSyncStatusChanged(ProfileSyncStatus.Failed);
                _logger.Error("Task exception after syncing profiles.", e.Task.Exception);
            }
            else
            {
                ProfileSyncStatus status =_syncAction.Running
                    ? ProfileSyncStatus.InProgress
                    : _syncFailed
                        ? ProfileSyncStatus.Failed
                        : ProfileSyncStatus.Succeeded;

                OnSyncStatusChanged(status, status == ProfileSyncStatus.Failed ? _syncErrorMessage : null);
            }
        }

        private async Task MergeApiToExternal()
        {
            IReadOnlyList<Profile> profiles;
            try
            {
                profiles = await _apiProfiles.GetAll();
                _cancellationToken.ThrowIfCancellationRequested();
            }
            catch (ProfileException e)
            {
                _syncFailed = true;
                _syncErrorMessage = e.Message;
                return;
            }

            await MergeApiToExternal(profiles);
        }

        private async Task MergeApiToExternal(IReadOnlyList<Profile> profiles)
        {
            using (CachedProfileData cached = await _cachedProfiles.LockedProfileData())
            {
                CachedProfileList external = cached.External;

                foreach (Profile profile in profiles)
                {
                    Profile candidate = profile.WithStatus(ProfileStatus.Synced);
                    candidate.ModifiedAt = DateTime.UtcNow;

                    Profile existing = external.FirstOrDefault(p => ProfileByExternalIdEqualityComparer.Equals(p, profile));
                    if (existing == null)
                    {
                        Profile notSynced = 
                            cached.Local.FirstOrDefault(p =>
                                p.Status == ProfileStatus.Created &&
                                ProfileByEssentialPropertiesEqualityComparer.Equals(p, profile)) ??
                                cached.Sync.FirstOrDefault(p =>
                                    p.Status == ProfileStatus.Created &&
                                    ProfileByEssentialPropertiesEqualityComparer.Equals(p, profile));

                        if (notSynced != null)
                        {
                            candidate = candidate.WithIdFrom(notSynced);
                            cached.Local.Remove(notSynced);
                            cached.Sync.Remove(notSynced);
                        }

                        external.Add(candidate.WithSyncStatus(ProfileSyncStatus.Succeeded));
                        continue;
                    }

                    if (ProfileByPropertiesEqualityComparer.Equals(profile, existing))
                        continue;

                    candidate = candidate
                        .WithIdFrom(existing)
                        .WithSyncStatus(existing.SyncStatus);
                    external.AddOrReplace(candidate);
                }

                external
                    .Except(profiles, ProfileByExternalIdEqualityComparer)
                    .ToList()
                    .ForEach(external.Remove);

                _changesSynced = _changesSynced || cached.HasChanges;
            }
        }

        private async Task MergeLocalToSync()
        {
            if (_syncFailed)
                return;

            // First checking existence of local to avoid unnecessary locking of profile data
            using (CachedProfileData cached = _cachedProfiles.ProfileData())
            {
                if (!cached.Local.Any())
                    return;
            }

            using (CachedProfileData cached = await _cachedProfiles.LockedProfileData())
            {
                CachedProfileList local = cached.Local;
                CachedProfileList sync = cached.Sync;
                local.ForEach(p => sync.AddOrReplace(p.WithStatusMergedFrom(sync.Get(p))));
                local.Clear();

                _changesSynced = _changesSynced || cached.HasChanges;
            }
        }

        private async Task MergeSyncToApi()
        {
            if (_syncFailed)
                return;

            using (CachedProfileData cached = _cachedProfiles.ProfileData())
            {
                var profiles = cached.Sync.OrderBy(p => p.ModifiedAt).ToList();
                foreach (Profile profile in profiles)
                {
                    await Sync(profile);

                    if (_syncFailed)
                        return;
                }
            }
        }

        private async Task Sync(Profile profile)
        {
            await _syncProfile.Sync(profile, _cancellationToken);

            if (!_syncProfile.Succeeded)
            {
                _syncFailed = true;
                _syncErrorMessage = _syncProfile.ErrorMessage;
            }
        }

        private async Task Retry(Func<Task> action, Func<bool> retryRequired, int numberOfRetries)
        {
            if (numberOfRetries < 1) throw new ArgumentOutOfRangeException(nameof(numberOfRetries));

            int i = numberOfRetries;
            do
            {
                await action();
                i--;
            } while (i > 0 && retryRequired());
        }

        private DateTime? _changesSyncedAt;
        private readonly Common.Configuration.Config _appConfig;

        private DateTime ChangesSyncedAt
        {
            get => _changesSyncedAt ?? (_changesSyncedAt = _appSettings.ProfileChangesSyncedAt).Value;
            set
            {
                if (value == _changesSyncedAt)
                    return;

                _changesSyncedAt = value;
                _appSettings.ProfileChangesSyncedAt = value;
            }
        }

        private void OnSyncStatusChanged(ProfileSyncStatus status, string errorMessage = null)
        {
            if (_syncStatus == status)
                return;

            _syncStatus = status;
            SyncStatusChanged?.Invoke(this, new ProfileSyncStatusChangedEventArgs(status, errorMessage, ChangesSyncedAt));
        }

        private void OnTimerElapsed()
        {
            if (_syncAction.Running)
                return;

            if (DateTime.UtcNow - _lastSyncAt > _appConfig.ProfileSyncPeriod)
                Sync();
            else
            {
                if (_syncStatus == ProfileSyncStatus.Succeeded)
                    SyncStatusChanged?.Invoke(this, new ProfileSyncStatusChangedEventArgs(_syncStatus, "", ChangesSyncedAt));
            }
        }
    }
}
