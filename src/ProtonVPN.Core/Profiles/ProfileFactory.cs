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
using ProtonVPN.Core.Servers.Models;

namespace ProtonVPN.Core.Profiles
{
    public class ProfileFactory : IProfileFactory
    {
        private readonly ColorProvider _colorProvider;

        public ProfileFactory(ColorProvider colorProvider)
        {
            _colorProvider = colorProvider;
        }

        public Profile Create()
        {
            return Create(null);
        }

        public Profile Create(string id)
        {
            return new(id) { ColorCode = _colorProvider.GetRandomColor() };
        }

        public Profile CreateFromServer(Server server)
        {
            return new(null)
            {
                IsTemporary = true,
                ProfileType = ProfileType.Custom,
                Features = (Features)server.Features,
                ServerId = server.Id
            };
        }
    }
}