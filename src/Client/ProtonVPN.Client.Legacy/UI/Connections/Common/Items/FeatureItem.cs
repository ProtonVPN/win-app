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

using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.UI.Assets.Icons.PathIcons;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;

namespace ProtonVPN.Client.Legacy.UI.Connections.Common.Items;

public class FeatureItem
{
    public ILocalizationProvider Localizer { get; }

    public Feature Feature { get; }

    public string Header => Localizer.GetFeatureName(Feature);

    public IconElement Icon { get; }

    public FeatureItem(
        ILocalizationProvider localizer,
        Feature feature)
    {
        Localizer = localizer;
        Feature = feature;
        Icon = feature switch
        {
            Feature.B2B => new Servers(),
            Feature.P2P => new ArrowRightArrowLeft(),
            Feature.SecureCore => new LockLayers(),
            Feature.Tor => new BrandTor(),
            _ => new Earth(),
        };
    }
}
