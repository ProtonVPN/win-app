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

using ProtonVPN.Client.Contracts.Enums;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.StatisticalEvents.Contracts.Dimensions;

namespace ProtonVPN.Client.Models.Connections;

public class GenericFastestLocationItem : LocationItemBase
{
    public override ILocationIntent LocationIntent { get; }

    public override IFeatureIntent? FeatureIntent => null;

    public override ConnectionGroupType GroupType { get; }

    public override string Header => Localizer.Get("Connections_Fastest");

    public override string? ToolTip => null;

    public override VpnTriggerDimension VpnTriggerDimension { get; } = VpnTriggerDimension.Profile;

    public GenericFastestLocationItem(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        ConnectionGroupType groupType,
        ILocationIntent locationIntent)
        : base(localizer,
               serversLoader,
               connectionManager,
               upsellCarouselWindowActivator,
               false)
    {
        GroupType = groupType;
        LocationIntent = locationIntent;
    }

    protected override bool MatchesActiveConnection(ConnectionDetails? currentConnectionDetails)
    {
        return false;
    }
}