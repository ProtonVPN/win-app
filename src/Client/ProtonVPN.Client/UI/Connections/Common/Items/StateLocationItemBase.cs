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

public abstract class StateLocationItemBase : LocationItemBase
{
    public override string Header => State.Name; 
    
    public string SubHeader => ShowBaseLocation
        ? $" -  {Localizer.GetCountryName(State.CountryCode)}"
        : string.Empty;

    public override string? ToolTip =>
        IsRestricted
            ? Localizer.Get("Connections_State_Restricted")
            : IsUnderMaintenance
                ? Localizer.Get("Connections_State_UnderMaintenance")
                : null;

    public State State { get; }

    public bool ShowBaseLocation { get; }

    public override object FirstSortProperty => Header;

    public override object SecondSortProperty => SubHeader;

    protected int ServersItemsCount { get; private set; }

    public string SecondaryActionLabel =>
        HasSubItems && ServersItemsCount > 0
            ? Localizer.GetPluralFormat("Connections_SeeServers", ServersItemsCount)
            : string.Empty;

    protected override ILocationIntent LocationIntent => new StateLocationIntent(State.CountryCode, State.Name);

    protected StateLocationItemBase(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IMainViewNavigator mainViewNavigator,
        IUpsellCarouselDialogActivator upsellCarouselActivator,
        LocationItemFactory locationItemFactory,
        State state,
        bool showBaseLocation)
        : base(localizer,
               serversLoader,
               connectionManager,
               mainViewNavigator,
               upsellCarouselActivator,
               locationItemFactory)
    {
        State = state;
        ShowBaseLocation = showBaseLocation;

        FetchSubItems();
    }

    public override void InvalidateIsActiveConnection(ConnectionDetails? currentConnectionDetails)
    {
        IsActiveConnection = currentConnectionDetails is not null
                          && !currentConnectionDetails.IsGateway
                          && State.CountryCode == currentConnectionDetails.ExitCountryCode
                          && State.Name == currentConnectionDetails.State
                          && (FeatureIntent?.GetType().IsAssignableTo(currentConnectionDetails.OriginalConnectionIntent.Feature?.GetType()) ?? true);

        foreach (LocationItemBase item in SubItems)
        {
            item.InvalidateIsActiveConnection(currentConnectionDetails);
        }
    }

    public override bool MatchesSearchQuery(string searchQuery)
    {
        return base.MatchesSearchQuery(searchQuery)
            || SubHeader.ContainsIgnoringCase(searchQuery);
    }

    protected override void GroupSubItems()
    {
        // States have cities and servers as subitems. 
        // Cities are only displayed as search results. Only display servers in flyout.
        SubGroups.Reset(
            SubItems.OfType<ServerLocationItemBase>()
                    .GroupBy(item => item.GroupType)
                    .Select(group => LocationItemFactory.GetGroup(group.Key, group)));
    }

    protected override void OnSubItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        base.OnSubItemsCollectionChanged(sender, e);

        ServersItemsCount = SubItems
            .OfType<ServerLocationItemBase>()
            .Count(item => item.IsCounted);

        OnPropertyChanged(nameof(SecondaryActionLabel));
    }
}