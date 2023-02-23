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
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Servers.Specs;

namespace ProtonVPN.Settings.Migrations.v1_7_2
{
    internal class MigratedProfile
    {
        private readonly ProfileV1 _profile;
        private readonly ServerManager _serverManager;
        private readonly IProfileFactory _profileFactory;
        private readonly ColorProvider _colorProvider;

        public MigratedProfile(ProfileV1 profile,
            ServerManager serverManager,
            IProfileFactory profileFactory, 
            ColorProvider colorProvider)
        {
            _profile = profile;
            _serverManager = serverManager;
            _profileFactory = profileFactory;
            _colorProvider = colorProvider;
        }

        public bool HasValue => !_profile.Predefined;

        public Profile Value
        {
            get
            {
                if (!HasValue)
                {
                    throw new InvalidOperationException();
                }

                string profileId = !string.IsNullOrEmpty(_profile.Hash) ? _profile.Hash : _profile.Id;
                string serverId = new MigratedServerId(_profile.ServerId).Value();
                Server server = _serverManager.GetServer(new ServerById(serverId));

                Profile profile = _profileFactory.Create(profileId);
                profile.Name = _profile.Name;
                profile.VpnProtocol = new MigratedProtocol(_profile.Protocol);
                profile.CountryCode = new MigratedCountryCode(_profile.Country, server);
                profile.ColorCode = _colorProvider.GetRandomColorIfInvalid(new MigratedColorCode(_profile.Color));
                profile.ProfileType = new MigratedProfileType(_profile.ProfileType);
                profile.ServerId = serverId;
                profile.Features = new MigratedFeatures(_profile.ServerType, server);
                profile.Status = ProfileStatus.Created;
                profile.SyncStatus = ProfileSyncStatus.InProgress;
                profile.ModifiedAt = DateTime.MinValue;
                return profile;
            }
        }

        public static implicit operator Profile(MigratedProfile item) => item.Value;
    }
}
