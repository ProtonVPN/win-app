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

using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Factories;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Cities;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.StatisticalEvents.Contracts.Dimensions;

namespace ProtonVPN.Client.Models.Connections;

public abstract class CityLocationItemBase : HostLocationItemBase<City>
{
    public City City { get; }

    public override string Header => City.Name;

    public override string Description =>
        string.IsNullOrEmpty(City.StateName)
            ? Localizer.GetCountryName(City.CountryCode)
            : $"{City.StateName}, {Localizer.GetCountryName(City.CountryCode)}";

    public override string? ToolTip =>
        IsRestricted
            ? Localizer.Get("Connections_City_Restricted")
            : IsUnderMaintenance
                ? Localizer.Get("Connections_City_UnderMaintenance")
                : null;

    public override ILocationIntent LocationIntent { get; }

    public override VpnTriggerDimension VpnTriggerDimension => IsSearchItem
        ? VpnTriggerDimension.SearchCity
        : VpnTriggerDimension.CountriesCity;

    protected CityLocationItemBase(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IMainWindowOverlayActivator overlayActivator,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        IConnectionGroupFactory connectionGroupFactory,
        ILocationItemFactory locationItemFactory,
        City city,
        bool showBaseLocation,
        bool isSearchItem)
        : base(localizer,
               serversLoader,
               connectionManager,
               overlayActivator,
               upsellCarouselWindowActivator,
               connectionGroupFactory,
               locationItemFactory,
               city,
               isSearchItem)
    {
        City = city;
        IsDescriptionVisible = showBaseLocation;

        LocationIntent = SingleCityLocationIntent.From(City.CountryCode, City.StateName, City.Name);
    }

    public void OnExpandCity()
    {
        FetchSubItems();
    }

    public void OnCollapseCity()
    {
        ClearSubItems();
    }

    protected override bool MatchesActiveConnection(ConnectionDetails? currentConnectionDetails)
    {
        return currentConnectionDetails is not null
            && !currentConnectionDetails.IsGateway
            && City.CountryCode == currentConnectionDetails.ExitCountryCode
            && City.StateName == currentConnectionDetails.State
            && City.Name == currentConnectionDetails.City
            && (FeatureIntent?.GetType().IsAssignableTo(currentConnectionDetails.OriginalConnectionIntent.Feature?.GetType()) ?? true);
    }
}