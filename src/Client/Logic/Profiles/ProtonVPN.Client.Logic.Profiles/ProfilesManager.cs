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

using System.Collections.Specialized;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Profiles.Contracts;
using ProtonVPN.Client.Logic.Profiles.Contracts.Messages;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Logic.Profiles.Files;

namespace ProtonVPN.Client.Logic.Profiles;

public class ProfilesManager : IProfilesManager,
    IEventMessageReceiver<LoggedInMessage>
{
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IProfilesFileReaderWriter _profilesFileReaderWriter;
    private readonly IDefaultProfilesProvider _defaultProfilesProvider;

    private readonly object _lock = new();

    private List<IConnectionProfile> _profiles = new();

    public ProfilesManager(
        IEventMessageSender eventMessageSender,
        IProfilesFileReaderWriter profilesFileReaderWriter,
        IDefaultProfilesProvider defaultProfilesProvider)
    {
        _eventMessageSender = eventMessageSender;
        _profilesFileReaderWriter = profilesFileReaderWriter;
        _defaultProfilesProvider = defaultProfilesProvider;
    }

    public IOrderedEnumerable<IConnectionProfile> GetAll()
    {
        return _profiles.OrderBy(p => p.CreationDateTimeUtc);
    }

    public IConnectionProfile? GetById(Guid profileId)
    {
        return _profiles.FirstOrDefault(p => p.Id == profileId);
    }

    public void OverrideProfiles(IEnumerable<IConnectionProfile> profiles)
    {
        lock (_lock)
        {
            _profiles.Clear();

            _profiles.AddRange(_defaultProfilesProvider.GetDefaultProfiles());
            _profiles.AddRange(profiles.DistinctBy(p => p.Id));

            SaveAndBroadcastProfileChanges(NotifyCollectionChangedAction.Reset);
        }
    }

    public void AddOrEditProfile(IConnectionProfile profile)
    {
        lock (_lock)
        {
            NotifyCollectionChangedAction action = NotifyCollectionChangedAction.Add;

            List<IConnectionProfile> existingProfiles = _profiles.Where(p => p.Id == profile.Id).ToList();
            foreach (IConnectionProfile existingProfile in existingProfiles)
            {
                _profiles.Remove(existingProfile);

                action = NotifyCollectionChangedAction.Replace;
            }

            profile.UpdateDateTimeUtc = DateTime.UtcNow;

            _profiles.Add(profile);

            SaveAndBroadcastProfileChanges(action, profile.Id);
        }
    }

    public void DeleteProfile(Guid profileId)
    {
        lock (_lock)
        {
            List<IConnectionProfile> profiles = _profiles.Where(p => p.Id == profileId).ToList();
            if (profiles.Count > 0)
            {
                foreach (IConnectionProfile profile in profiles)
                {
                    _profiles.Remove(profile);
                }

                SaveAndBroadcastProfileChanges(NotifyCollectionChangedAction.Remove, profileId);
            }
        }
    }

    public void Receive(LoggedInMessage message)
    {
        lock (_lock)
        {
            LoadProfiles();
        }

        BroadcastProfileChanges(NotifyCollectionChangedAction.Reset);
    }

    private void SaveAndBroadcastProfileChanges(NotifyCollectionChangedAction action, Guid? changedProfileId = null)
    {
        SaveProfiles();
        BroadcastProfileChanges(action, changedProfileId);
    }

    private void LoadProfiles()
    {
        _profiles = _profilesFileReaderWriter.Read(out bool doesFileExists);

        // No profiles found and profile file does not exist, create list of default profiles
        if (!_profiles.Any() && !doesFileExists)
        {
            _profiles.AddRange(_defaultProfilesProvider.GetDefaultProfiles());

            SaveProfiles();
        }
    }

    private void SaveProfiles()
    {
        _profilesFileReaderWriter.Save(_profiles.ToList());
    }

    private void BroadcastProfileChanges(NotifyCollectionChangedAction action, Guid? changedProfileId = null)
    {
        _eventMessageSender.Send(new ProfilesChangedMessage()
        {
            Action = action,
            ChangedProfileId = changedProfileId
        });
    }
}