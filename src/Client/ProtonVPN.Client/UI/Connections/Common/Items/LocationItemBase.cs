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
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Data;
using ProtonVPN.Client.Common.Collections;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Models.Activation.Custom;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Connections.Common.Enums;
using ProtonVPN.Client.UI.Connections.Common.Extensions;
using ProtonVPN.Client.UI.Connections.Common.Factories;
using ProtonVPN.Client.UI.Home;
using ProtonVPN.Common.Core.Extensions;

namespace ProtonVPN.Client.UI.Connections.Common.Items;

public abstract partial class LocationItemBase : ObservableObject
{
    protected readonly IServersLoader ServersLoader;
    protected readonly IConnectionManager ConnectionManager;
    protected readonly IMainViewNavigator MainViewNavigator;
    protected readonly IUpsellCarouselDialogActivator UpsellCarouselActivator;

    protected readonly LocationItemFactory LocationItemFactory;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ToggleConnectionCommand))]
    [NotifyPropertyChangedFor(nameof(IsEnabled))]
    [NotifyPropertyChangedFor(nameof(ToolTip))]
    private bool _isUnderMaintenance;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ToggleConnectionCommand))]
    [NotifyPropertyChangedFor(nameof(PrimaryActionLabel))]
    [NotifyPropertyChangedFor(nameof(PrimaryCommandAutomationId))]
    private bool _isActiveConnection;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ToggleConnectionCommand))]
    [NotifyPropertyChangedFor(nameof(IsEnabled))]
    [NotifyPropertyChangedFor(nameof(ToolTip))]
    private bool _isRestricted;

    private string SearchableHeader { get; }

    public bool IsEnabled => !IsRestricted && !IsUnderMaintenance;

    public ILocalizationProvider Localizer { get; }

    public abstract GroupLocationType GroupType { get; }

    public string Header { get; }

    public abstract string? ToolTip { get; }

    public virtual bool IsCounted => true;

    public abstract object FirstSortProperty { get; }

    public abstract object SecondSortProperty { get; }

    public CollectionViewSource SubLocationGroupsCvs { get; }

    public SmartObservableCollection<LocationItemBase> SubItems { get; } = [];

    public SmartObservableCollection<LocationGroup> SubGroups { get; } = [];

    public bool HasSubItems => SubItemsCount > 0;

    public string PrimaryActionLabel => Localizer.Get(
        IsActiveConnection
            ? "Common_Actions_Disconnect"
            : "Common_Actions_Connect");

    public string PrimaryCommandAutomationId =>
        IsActiveConnection
            ? $"Disconnect_from_{AutomationName}"
            : $"Connect_to_{AutomationName}";

    public string SecondaryCommandAutomationId => $"Navigate_to_{AutomationName}";

    public string ActiveConnectionAutomationId => $"Active_connection_{AutomationName}";

    protected int SubItemsCount { get; private set; }

    protected abstract ILocationIntent LocationIntent { get; }

    protected virtual IFeatureIntent? FeatureIntent => null;

    protected virtual string AutomationName => Header;

    protected LocationItemBase(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IMainViewNavigator mainViewNavigator,
        IUpsellCarouselDialogActivator upsellCarouselActivator,
        LocationItemFactory locationItemFactory,
        string header)
    {
        Localizer = localizer;
        ServersLoader = serversLoader;
        ConnectionManager = connectionManager;
        MainViewNavigator = mainViewNavigator;
        UpsellCarouselActivator = upsellCarouselActivator;
        LocationItemFactory = locationItemFactory;

        Header = header;
        SearchableHeader = header.RemoveDiacritics();

        SubItems.CollectionChanged += OnSubItemsCollectionChanged;

        SubLocationGroupsCvs = new()
        {
            Source = SubGroups,
            IsSourceGrouped = true
        };
    }

    public ConnectionIntent GetConnectionIntent()
    {
        return new(LocationIntent, FeatureIntent);
    }

    public virtual bool MatchesSearchQuery(string searchQuery)
    {
        return string.IsNullOrWhiteSpace(searchQuery)
               || Header.ContainsIgnoringCase(searchQuery)
               || SearchableHeader.ContainsIgnoringCase(searchQuery);
    }

    public abstract void InvalidateIsActiveConnection(ConnectionDetails? currentConnectionDetails);

    public void InvalidateIsRestricted(bool isPaidUser)
    {
        IsRestricted = !isPaidUser;

        foreach (LocationItemBase item in SubItems)
        {
            item.InvalidateIsRestricted(isPaidUser);
        }
    }

    protected virtual void InvalidateIsUnderMaintenance()
    {
        IsUnderMaintenance = SubItems.Any() && SubItems.All(item => item.IsUnderMaintenance);
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

    protected virtual IEnumerable<LocationItemBase> GetSubItems()
    {
        return [];
    }

    protected virtual void OnSubItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SubItemsCount = SubItems.Count(item => item.IsCounted);
        OnPropertyChanged(nameof(HasSubItems));
    }

    [RelayCommand(CanExecute = nameof(CanToggleConnection))]
    protected async Task ToggleConnectionAsync()
    {
        if (IsRestricted)
        {
            UpsellCarouselActivator.ShowDialog(GroupType.GetUpsellModalSources());
            return;
        }

        await MainViewNavigator.NavigateToAsync<HomeViewModel>();

        if (IsActiveConnection)
        {
            await ConnectionManager.DisconnectAsync();
        }
        else
        {
            await ConnectionManager.ConnectAsync(GetConnectionIntent());
        }
    }

    private bool CanToggleConnection()
    {
        return !IsUnderMaintenance
            || IsActiveConnection
            || IsRestricted;
    }
}