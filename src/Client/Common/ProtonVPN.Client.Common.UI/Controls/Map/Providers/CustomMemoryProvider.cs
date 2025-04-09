/*
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
using Mapsui;
using Mapsui.Layers;
using Mapsui.Providers;

namespace ProtonVPN.Client.Common.UI.Controls.Map.Providers;

public class CustomMemoryProvider : MemoryProvider
{
    public CustomMemoryProvider(IEnumerable<IFeature> features) : base(features)
    {
    }

    public override Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
    {
        IFeature[] source = Features.ToArray();
        return Task.FromResult((IEnumerable<IFeature>)source.Where((IFeature f) => f != null).ToList());
    }
}