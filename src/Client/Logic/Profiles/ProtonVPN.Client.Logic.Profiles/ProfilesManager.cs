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

using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Profiles.Contracts;
using ProtonVPN.Client.Logic.Profiles.Contracts.Messages;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Logic.Profiles.Files;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Logic.Profiles;

public class ProfilesManager : IProfilesManager,
    IEventMessageReceiver<LoggedInMessage>
{
    private readonly ILogger _logger;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IProfilesFileReaderWriter _profilesFileReaderWriter;
    private readonly IDefaultProfilesProvider _defaultProfilesProvider;
    private readonly ISettings _settings;

    private readonly object _lock = new();

    private List<IConnectionProfile> _profiles = new();

    public ProfilesManager(
        ILogger logger,
        IEventMessageSender eventMessageSender,
        IProfilesFileReaderWriter profilesFileReaderWriter,
        IDefaultProfilesProvider defaultProfilesProvider,
        ISettings settings)
    {
        _logger = logger;
        _eventMessageSender = eventMessageSender;
        _profilesFileReaderWriter = profilesFileReaderWriter;
        _defaultProfilesProvider = defaultProfilesProvider;
        _settings = settings;
    }

    public IOrderedEnumerable<IConnectionProfile> GetAll()
    {
        return _profiles.OrderBy(p => p.Name);
    }

    public IConnectionProfile? GetById(Guid profileId)
    {
        return _profiles.FirstOrDefault(p => p.Id == profileId);
    }

    public void OverrideProfiles(IEnumerable<IConnectionProfile> profiles, IConnectionProfile? quickConnectionProfile = null)
    {
        lock (_lock)
        {
            _profiles.Clear();

            foreach (IConnectionProfile profile in profiles)
            {
                AddOrEditProfile(profile);
            }

            if (quickConnectionProfile is not null)
            {
                AddOrEditProfile(quickConnectionProfile);
                SetAsDefaultConnection(quickConnectionProfile.Id);
            }

            SaveProfiles();
        }
    }

    public void AddOrEditProfile(IConnectionProfile profile)
    {
        lock (_lock)
        {
            List<IConnectionProfile> existingProfiles = _profiles.Where(p => p.Id == profile.Id).ToList();
            foreach (IConnectionProfile existingProfile in existingProfiles)
            {
                _profiles.Remove(existingProfile);
            }

            profile.UpdateDateTimeUtc = DateTime.UtcNow;

            _profiles.Add(profile);

            SaveAndBroadcastProfileChanges();
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

                SaveAndBroadcastProfileChanges();
            }

            ResetDefaultConnectionIfProfileDoesNotExist();
        }
    }

    private void ResetDefaultConnectionIfProfileDoesNotExist()
    {
        DefaultConnection defaultConnection = _settings.DefaultConnection;
        if (defaultConnection.Type == DefaultConnectionType.Profile && !_profiles.Any(p => p.Id == defaultConnection.ProfileId))
        {
            _settings.DefaultConnection = DefaultSettings.DefaultConnection;
        }
    }

    public void Receive(LoggedInMessage message)
    {
        lock (_lock)
        {
            LoadProfiles();
        }

        BroadcastProfileChanges();
    }

    private void SaveAndBroadcastProfileChanges()
    {
        SaveProfiles();
        BroadcastProfileChanges();
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

        ResetDefaultConnectionIfProfileDoesNotExist();
    }

    private void SaveProfiles()
    {
        _profilesFileReaderWriter.Save(_profiles.ToList());
    }

    private void BroadcastProfileChanges()
    {
        _eventMessageSender.Send(new ProfilesChangedMessage());
    }

    public void SetAsDefaultConnection(Guid profileId)
    {
        _settings.DefaultConnection = new DefaultConnection(profileId);
    }
}
