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
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Specs;

namespace ProtonVPN.Settings.Migrations.v1_7_2
{
    internal class MigratedProfile
    {
        private readonly ProfileV1 _profile;
        private readonly ServerManager _serverManager;

        public MigratedProfile(ProfileV1 profile, ServerManager serverManager)
        {
            _profile = profile;
            _serverManager = serverManager;
        }

        public bool HasValue => !_profile.Predefined;

        public Profile Value
        {
            get
            {
                if (!HasValue)
                    throw new InvalidOperationException();

                var profileId = !string.IsNullOrEmpty(_profile.Hash) ? _profile.Hash : _profile.Id;
                var serverId = new MigratedServerId(_profile.ServerId).Value();
                var server = _serverManager.GetServer(new ServerById(serverId));

                return new Profile(profileId)
                {
                    Name = _profile.Name,
                    Protocol = new MigratedProtocol(_profile.Protocol),
                    CountryCode = new MigratedCountryCode(_profile.Country, server),
                    ColorCode = new MigratedColorCode(_profile.Color),
                    ProfileType = new MigratedProfileType(_profile.ProfileType),
                    ServerId = serverId,
                    Features = new MigratedFeatures(_profile.ServerType, server),
                    Status = ProfileStatus.Created,
                    SyncStatus = ProfileSyncStatus.InProgress,
                    ModifiedAt = DateTime.MinValue
                };
            }
        }

        public static implicit operator Profile(MigratedProfile item) => item.Value;
    }
}
