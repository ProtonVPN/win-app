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
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Countries;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.StatisticalEvents.Contracts.Dimensions;

namespace ProtonVPN.Client.Models.Connections.Countries;

public class SecureCoreCountryPairLocationItem : LocationItemBase<SecureCoreCountryPair>
{
    public SecureCoreCountryPair CountryPair { get; }

    public override ConnectionGroupType GroupType { get; } = ConnectionGroupType.SecureCoreCountryPairs;

    public override string Header => Localizer.GetCountryName(CountryPair.ExitCountry);

    public override string Description => Localizer.GetSecureCoreLabel(CountryPair.EntryCountry);

    public override VpnTriggerDimension VpnTriggerDimension => IsSearchItem
        ? VpnTriggerDimension.SearchCountry
        : VpnTriggerDimension.CountriesCountry;

    public override ILocationIntent LocationIntent { get; }

    public override IFeatureIntent? FeatureIntent { get; }

    public override string? ToolTip =>
        IsRestricted
            ? Localizer.Get("Connections_Server_Restricted")
            : IsUnderMaintenance
                ? Localizer.Get("Connections_Server_UnderMaintenance")
                : null;

    public SecureCoreCountryPairLocationItem(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        SecureCoreCountryPair countryPair,
        bool isSearchItem)
        : base(localizer,
               serversLoader,
               connectionManager,
               upsellCarouselWindowActivator,
               countryPair,
               isSearchItem)
    {
        CountryPair = countryPair;

        LocationIntent = SingleCountryLocationIntent.From(CountryPair.ExitCountry);
        FeatureIntent = new SecureCoreFeatureIntent(CountryPair.EntryCountry);
    }

    protected override bool MatchesActiveConnection(ConnectionDetails? currentConnectionDetails)
    {
        return currentConnectionDetails is not null
            && !currentConnectionDetails.IsGateway
            && CountryPair.ExitCountry == currentConnectionDetails.ExitCountryCode
            && CountryPair.EntryCountry == currentConnectionDetails.EntryCountryCode
            && (FeatureIntent?.IsSameAs(currentConnectionDetails.OriginalConnectionIntent.Feature) ?? false);
    }
}