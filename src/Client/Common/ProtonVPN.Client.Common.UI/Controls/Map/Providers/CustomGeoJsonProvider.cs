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
using System.Threading.Tasks;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Nts.Providers;
using Mapsui.Providers;

namespace ProtonVPN.Client.Common.UI.Controls.Map.Providers;

public class CustomGeoJsonProvider : IProvider
{
    private readonly GeoJsonProvider _geoJsonProvider;

    private IEnumerable<IFeature>? _features;

    public CustomGeoJsonProvider(GeoJsonProvider geoJsonProvider)
    {
        _geoJsonProvider = geoJsonProvider;
    }

    public MRect GetExtent()
    {
        return _geoJsonProvider.GetExtent() ?? new MRect(0, 0, 0, 0);
    }

    public async Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
    {
        if (_features is not null)
        {
            return _features;
        }

        MSection section = new(fetchInfo.Section.Extent.Grow(200), fetchInfo.Resolution);
        FetchInfo fetchInfoCopy = new FetchInfo(section, fetchInfo.CRS, fetchInfo.ChangeType);

        _features = await _geoJsonProvider.GetFeaturesAsync(fetchInfoCopy);

        return _features;
    }

    public string? CRS
    {
        get => _geoJsonProvider.CRS;
        set => _geoJsonProvider.CRS = value;
    }
}