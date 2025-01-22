﻿/*
 * Copyright (c) 2024 Proton AG
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
using System.Linq;
using System.Threading.Tasks;
using ProtonVPN.Update.Releases;

namespace ProtonVPN.Update.Storage;

/// <summary>
/// Orders stream of app releases by version number in descending order.
/// </summary>
public class OrderedReleaseStorage : IReleaseStorage
{
    private readonly IReleaseStorage _storage;

    public OrderedReleaseStorage(IReleaseStorage storage)
    {
        _storage = storage;
    }

    public async Task<IEnumerable<Release>> GetReleasesAsync()
    {
        return (await _storage.GetReleasesAsync())
            .OrderByDescending(r => r);
    }
}