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

using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Legacy.Models.Activation.Custom;
using ProtonVPN.Client.Legacy.Models.Navigation;
using ProtonVPN.Client.Legacy.UI.Connections.Common.Enums;
using ProtonVPN.Client.Legacy.UI.Connections.Common.Factories;
using ProtonVPN.Common.Core.Extensions;

namespace ProtonVPN.Client.Legacy.UI.Connections.Common.Items;

public class SecureCoreCountryPairLocationItem : LocationItemBase
{
    public override GroupLocationType GroupType => GroupLocationType.SecureCoreCountryPairs;

    public SecureCoreCountryPair CountryPair { get; }

    public override string? ToolTip =>
        IsRestricted
            ? Localizer.Get("Connections_Server_Restricted")
            : IsUnderMaintenance
                ? Localizer.Get("Connections_Server_UnderMaintenance")
                :  null;

    public string Description => Localizer.GetSecureCoreLabel(CountryPair.EntryCountry);

    public override object FirstSortProperty => Header;

    public override object SecondSortProperty => Description;

    public override ILocationIntent LocationIntent => new CountryLocationIntent(CountryPair.ExitCountry);

    public override IFeatureIntent? FeatureIntent => new SecureCoreFeatureIntent(CountryPair.EntryCountry);

    protected override string AutomationName => $"{CountryPair.ExitCountry}_via_{CountryPair.EntryCountry}";

    public SecureCoreCountryPairLocationItem(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IMainViewNavigator mainViewNavigator,
        IUpsellCarouselDialogActivator upsellCarouselActivator,
        LocationItemFactory locationItemFactory,
        SecureCoreCountryPair countryPair)
        : base(localizer,
               serversLoader,
               connectionManager,
               mainViewNavigator,
               upsellCarouselActivator,
               locationItemFactory,
               localizer.GetCountryName(countryPair.ExitCountry))
    {
        CountryPair = countryPair;

        FetchSubItems();
    }

    protected override IEnumerable<LocationItemBase> GetSubItems()
    {
        return ServersLoader.GetServersBySecureCoreCountryPair(CountryPair)
                            .Select(LocationItemFactory.GetSecureCoreServer);
    }

    public override bool MatchesSearchQuery(string searchQuery)
    {
        return base.MatchesSearchQuery(searchQuery)
            || Description.ContainsIgnoringCase(searchQuery);
    }

    public override void InvalidateIsActiveConnection(ConnectionDetails? currentConnectionDetails)
    {
        IsActiveConnection = currentConnectionDetails is not null
                          && !currentConnectionDetails.IsGateway
                          && CountryPair.ExitCountry == currentConnectionDetails.ExitCountryCode
                          && CountryPair.EntryCountry == currentConnectionDetails.EntryCountryCode
                          && (FeatureIntent?.IsSameAs(currentConnectionDetails.OriginalConnectionIntent.Feature) ?? false);
    }
}