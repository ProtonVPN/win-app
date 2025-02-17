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

using ProtonVPN.Client.Common.Enums;
using ProtonVPN.Client.Contracts.Enums;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.StatisticalEvents.Contracts.Dimensions;

namespace ProtonVPN.Client.Models.Connections.Gateways;

public class GenericGatewayLocationItem : LocationItemBase
{
    public ConnectionIntentKind IntentKind { get; }

    public bool ExcludeMyCountry { get; }

    public FlagType FlagType => IntentKind switch
    {
        ConnectionIntentKind.Random => FlagType.Random,
        _ => FlagType.Fastest,
    };

    public override string Header => Localizer.GetGatewayName(string.Empty, IntentKind);

    public override bool IsCounted => false;

    public override object FirstSortProperty => string.Empty;

    public override object SecondSortProperty => string.Empty;

    public override ILocationIntent LocationIntent { get; }

    public override IFeatureIntent? FeatureIntent { get; } = new B2BFeatureIntent();

    public override ConnectionGroupType GroupType => ConnectionGroupType.Gateways;

    public override string? ToolTip =>
        IsRestricted
            ? Localizer.Get("Connections_Gateway_Restricted")
            : IsUnderMaintenance
                ? Localizer.Get("Connections_Gateway_UnderMaintenance")
                : null;

    protected override string AutomationName => IntentKind switch
    {
        ConnectionIntentKind.Fastest => "Fastest",
        ConnectionIntentKind.Random => "Random",
        _ => throw new NotImplementedException($"Intent kind '{IntentKind}' is not supported."),
    };

    public override VpnTriggerDimension VpnTriggerDimension { get; } = VpnTriggerDimension.GatewaysGateway;

    public GenericGatewayLocationItem(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        ConnectionIntentKind intentKind)
        : base(localizer,
               serversLoader,
               connectionManager,
               upsellCarouselWindowActivator, 
               false)
    {
        IntentKind = intentKind;

        LocationIntent = new GatewayLocationIntent(IntentKind);
    }

    protected override bool MatchesActiveConnection(ConnectionDetails? currentConnectionDetails)
    {
        return currentConnectionDetails is not null
            && currentConnectionDetails.OriginalConnectionIntent.Location.IsSameAs(LocationIntent)
            && ((currentConnectionDetails.OriginalConnectionIntent.Feature == null && FeatureIntent == null)
               || (currentConnectionDetails.OriginalConnectionIntent.Feature?.IsSameAs(FeatureIntent) ?? false));
    }
}