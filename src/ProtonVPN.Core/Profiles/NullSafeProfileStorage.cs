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
using System.Threading.Tasks;

namespace ProtonVPN.Core.Profiles
{
    public class NullSafeProfileStorage : IProfileStorageAsync
    {
        private readonly IProfileStorageAsync _storage;

        public NullSafeProfileStorage(IProfileStorageAsync storage)
        {
            _storage = storage;
        }

        public Task<IReadOnlyList<Profile>> GetAll()
        {
            return _storage.GetAll();
        }

        public async Task Create(Profile profile)
        {
            if (profile == null)
                return;

            await _storage.Create(profile);
        }

        public async Task Update(Profile profile)
        {
            if (profile == null)
                return;

            await _storage.Update(profile);
        }

        public async Task Delete(Profile profile)
        {
            if (profile == null)
                return;

            await _storage.Delete(profile);
        }
    }
}
