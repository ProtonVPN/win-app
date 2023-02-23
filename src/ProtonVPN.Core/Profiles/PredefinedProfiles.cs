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

using ProtonVPN.Core.Servers;
using System.Collections.Generic;

namespace ProtonVPN.Core.Profiles
{
    public class PredefinedProfiles : IProfileSource
    {
        private readonly IProfileFactory _profileFactory;

        public PredefinedProfiles(IProfileFactory profileFactory)
        {
            _profileFactory = profileFactory;
        }

        public IReadOnlyList<Profile> GetAll()
        {
            return new List<Profile>
            {
                CreateFastestProfile(),
                CreateRandomProfile()
            };
        }

        private Profile CreateFastestProfile()
        {
            Profile profile = _profileFactory.Create("Fastest");
            profile.IsPredefined = true;
            profile.Name = "Fastest";
            profile.ProfileType = ProfileType.Fastest;
            profile.Features = Features.None;
            return profile;
        }

        private Profile CreateRandomProfile()
        {
            Profile profile = _profileFactory.Create("Random");
            profile.IsPredefined = true;
            profile.Name = "Random";
            profile.ProfileType = ProfileType.Random;
            profile.Features = Features.None;
            return profile;
        }
    }
}
