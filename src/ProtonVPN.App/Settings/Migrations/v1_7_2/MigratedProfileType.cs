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
using ProtonVPN.Core.Profiles;

namespace ProtonVPN.Settings.Migrations.v1_7_2
{
    class MigratedProfileType
    {
        public static readonly Dictionary<ProfileTypeV1, ProfileType> Map = new Dictionary<ProfileTypeV1, ProfileType>
        {
            { ProfileTypeV1.Custom, ProfileType.Custom },
            { ProfileTypeV1.Fastest, ProfileType.Fastest },
            { ProfileTypeV1.Random, ProfileType.Random }
        };

        private readonly ProfileTypeV1 _profileType;

        public MigratedProfileType(ProfileTypeV1 profileType)
        {
            _profileType = profileType;
        }

        public static implicit operator ProfileType(MigratedProfileType item) => item.Value();

        public ProfileType Value() => Map[_profileType];
    }
}
