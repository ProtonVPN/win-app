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
using Microsoft.UI.Xaml.Data;
using ProtonVPN.Client.Common.Collections;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Legacy.Models.Activation.Custom;
using ProtonVPN.Client.Legacy.Models.Navigation;
using ProtonVPN.Client.Legacy.UI.Connections.Common.Enums;
using ProtonVPN.Client.Legacy.UI.Connections.Common.Extensions;
using ProtonVPN.Client.Legacy.UI.Connections.Common.Factories;
using ProtonVPN.Common.Core.Extensions;

namespace ProtonVPN.Client.Legacy.UI.Connections.Common.Items;

public abstract partial class LocationItemBase : ConnectionItemBase
{
    protected readonly LocationItemFactory LocationItemFactory;

    public abstract GroupLocationType GroupType { get; }

    public CollectionViewSource SubLocationGroupsCvs { get; }

    public SmartObservableCollection<LocationItemBase> SubItems { get; } = [];

    public SmartObservableCollection<LocationGroup> SubGroups { get; } = [];

    public bool HasSubItems => SubItemsCount > 0;

    public abstract object FirstSortProperty { get; }

    public abstract object SecondSortProperty { get; }

    public override string SecondaryCommandAutomationId => $"Navigate_to_{AutomationName}";

    public override ModalSources UpsellModalSource => GroupType.GetUpsellModalSources();

    protected int SubItemsCount { get; private set; }

    public abstract ILocationIntent LocationIntent { get; }

    public virtual IFeatureIntent? FeatureIntent => null;

    protected LocationItemBase(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IMainViewNavigator mainViewNavigator,
        IUpsellCarouselDialogActivator upsellCarouselActivator,
        LocationItemFactory locationItemFactory,
        string header)
        : base(localizer,
               serversLoader,
               connectionManager,
               mainViewNavigator,
               upsellCarouselActivator,
               header)
    {
        LocationItemFactory = locationItemFactory;

        SubItems.CollectionChanged += OnSubItemsCollectionChanged;

        SubLocationGroupsCvs = new()
        {
            Source = SubGroups,
            IsSourceGrouped = true
        };
    }

    public override IConnectionIntent GetConnectionIntent()
    {
        return new ConnectionIntent(LocationIntent, FeatureIntent);
    }

    public virtual bool MatchesSearchQuery(string searchQuery)
    {
        return string.IsNullOrWhiteSpace(searchQuery)
               || Header.ContainsIgnoringCase(searchQuery)
               || SearchableHeader.ContainsIgnoringCase(searchQuery);
    }

    public override void InvalidateIsRestricted(bool isPaidUser)
    {
        base.InvalidateIsRestricted(isPaidUser);

        foreach (LocationItemBase item in SubItems)
        {
            item.InvalidateIsRestricted(isPaidUser);
        }
    }

    public override void InvalidateIsUnderMaintenance()
    {
        IsUnderMaintenance = SubItems.Any() && SubItems.All(item => item.IsUnderMaintenance);
    }

    protected virtual IEnumerable<LocationItemBase> GetSubItems()
    {
        return [];
    }

    protected void FetchSubItems()
    {
        SubItems.Reset(
            GetSubItems().OrderBy(item => item.GroupType)
                         .ThenBy(item => item.FirstSortProperty)
                         .ThenBy(item => item.SecondSortProperty));

        GroupSubItems();

        InvalidateIsUnderMaintenance();
    }

    protected virtual void GroupSubItems()
    {
        SubGroups.Reset(
            SubItems.GroupBy(item => item.GroupType)
                    .Select(group => LocationItemFactory.GetGroup(group.Key, group)));
    }

    protected virtual void OnSubItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SubItemsCount = SubItems.Count(item => item.IsCounted);

        OnPropertyChanged(nameof(HasSubItems));
    }
}