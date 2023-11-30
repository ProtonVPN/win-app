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
using Newtonsoft.Json;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Core.FeatureFlags;

public class FeatureFlagsCache : IFeatureFlagsCache
{
    private readonly IAppSettings _appSettings;

    public FeatureFlagsCache(IAppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    public IReadOnlyList<FeatureFlag> Get()
    {
        return JsonConvert.DeserializeObject<IReadOnlyList<FeatureFlag>>(_appSettings.FeatureFlags)
            ?? new List<FeatureFlag>();
    }

    public void Store(IReadOnlyList<FeatureFlag> flags)
    {
        _appSettings.FeatureFlags = JsonConvert.SerializeObject(flags ?? new List<FeatureFlag>());
    }
}