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

using System.Collections.Specialized;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Models.Activation.Custom;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Connections.Common.Factories;
using ProtonVPN.Common.Core.Extensions;

namespace ProtonVPN.Client.UI.Connections.Common.Items;

public abstract class CityLocationItemBase : LocationItemBase
{
    public string SubHeader { get; }

    public string SearchableSubHeader { get; }

    public override string? ToolTip =>
        IsRestricted
            ? Localizer.Get("Connections_City_Restricted")
            : IsUnderMaintenance
                ? Localizer.Get("Connections_City_UnderMaintenance")
                : null;

    public City City { get; }

    public bool ShowBaseLocation { get; }

    public override object FirstSortProperty => Header;

    public override object SecondSortProperty => SubHeader;

    public bool BelongsToState => !string.IsNullOrEmpty(City.StateName);

    public string SecondaryActionLabel =>
        HasSubItems
            ? Localizer.GetPluralFormat("Connections_SeeServers", SubItemsCount)
            : string.Empty;

    public override ILocationIntent LocationIntent => new CityLocationIntent(City.CountryCode, City.StateName, City.Name);

    protected CityLocationItemBase(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IMainViewNavigator mainViewNavigator,
        IUpsellCarouselDialogActivator upsellCarouselActivator,
        LocationItemFactory locationItemFactory,
        City city,
        bool showBaseLocation)
        : base(localizer,
               serversLoader,
               connectionManager,
               mainViewNavigator,
               upsellCarouselActivator,
               locationItemFactory,
               city.Name)
    {
        City = city;
        ShowBaseLocation = showBaseLocation;

        SubHeader = ShowBaseLocation
            ? BelongsToState
                ? $" -  {City.StateName}, {Localizer.GetCountryName(City.CountryCode)}"
                : $" -  {Localizer.GetCountryName(City.CountryCode)}"
            : string.Empty;
        SearchableSubHeader = SubHeader.RemoveDiacritics();

        FetchSubItems();
    }

    public override void InvalidateIsActiveConnection(ConnectionDetails? currentConnectionDetails)
    {
        IsActiveConnection = currentConnectionDetails is not null
                          && !currentConnectionDetails.IsGateway
                          && City.CountryCode == currentConnectionDetails.ExitCountryCode
                          && City.StateName == currentConnectionDetails.State
                          && City.Name == currentConnectionDetails.City
                          && (FeatureIntent?.GetType().IsAssignableTo(currentConnectionDetails.OriginalConnectionIntent.Feature?.GetType()) ?? true);

        foreach (LocationItemBase item in SubItems)
        {
            item.InvalidateIsActiveConnection(currentConnectionDetails);
        }
    }

    public override bool MatchesSearchQuery(string searchQuery)
    {
        return base.MatchesSearchQuery(searchQuery)
            || SubHeader.ContainsIgnoringCase(searchQuery)
            || SearchableSubHeader.ContainsIgnoringCase(searchQuery);
    }

    protected override void OnSubItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        base.OnSubItemsCollectionChanged(sender, e);

        OnPropertyChanged(nameof(SecondaryActionLabel));
    }
}