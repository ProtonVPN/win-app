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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Data;
using ProtonVPN.Client.Common.Collections;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Factories;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;

namespace ProtonVPN.Client.Models.Connections;

public abstract partial class HostLocationItemBase<TLocation> : LocationItemBase<TLocation>, IHostLocationItem
    where TLocation : ILocation
{
    protected readonly IMainWindowOverlayActivator OverlayActivator;
    protected readonly IConnectionGroupFactory ConnectionGroupFactory;
    protected readonly ILocationItemFactory LocationItemFactory;

    private IEnumerable<Server> _lastKnownServers = [];
    private bool _lastKnownIsPaidUser = false;
    private ConnectionDetails? _lastKnownConnectionDetails = null;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ShowSmartRoutingOverlayCommand))]
    private bool _hasVirtualServers;

    [ObservableProperty]
    private string? _smartRoutingDescription;

    public CollectionViewSource SubGroupsCvs { get; }

    public SmartObservableCollection<ConnectionGroup> SubGroups { get; } = [];

    public SmartObservableCollection<ConnectionItemBase> SubItems { get; } = [];

    protected virtual bool IsSubGroupHeaderHidden => false;

    protected HostLocationItemBase(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IMainWindowOverlayActivator overlayActivator,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        IConnectionGroupFactory connectionGroupFactory,
        ILocationItemFactory locationItemFactory,
        TLocation location,
        bool isSearchItem)
        : base(localizer,
               serversLoader,
               connectionManager,
               upsellCarouselWindowActivator,
               location,
               isSearchItem)
    {
        OverlayActivator = overlayActivator;
        ConnectionGroupFactory = connectionGroupFactory;
        LocationItemFactory = locationItemFactory;

        SubGroupsCvs = new()
        {
            Source = SubGroups,
            IsSourceGrouped = true
        };
    }

    public override void InvalidateIsActiveConnection(ConnectionDetails? currentConnectionDetails)
    {
        _lastKnownConnectionDetails = currentConnectionDetails;

        base.InvalidateIsActiveConnection(currentConnectionDetails);

        foreach (ConnectionItemBase item in SubItems)
        {
            item.InvalidateIsActiveConnection(currentConnectionDetails);
        }
    }

    public override void InvalidateIsRestricted(bool isPaidUser)
    {
        _lastKnownIsPaidUser = isPaidUser;

        base.InvalidateIsRestricted(isPaidUser);

        foreach (ConnectionItemBase item in SubItems)
        {
            item.InvalidateIsRestricted(isPaidUser);
        }
    }

    protected abstract IEnumerable<ConnectionItemBase> GetSubItems();

    public void FetchSubItems()
    {
        SubItems.Reset(
            GetSubItems().OrderBy(item => item.GroupType)
                         .ThenBy(item => item.FirstSortProperty)
                         .ThenBy(item => item.SecondSortProperty));

        GroupSubItems();

        InvalidateIsActiveConnection(_lastKnownConnectionDetails);
        InvalidateIsRestricted(_lastKnownIsPaidUser);

        ServerLocationItemBase? virtualServer = SubItems.OfType<ServerLocationItemBase>().FirstOrDefault(s => s.IsVirtual);

        HasVirtualServers = virtualServer != null;
        SmartRoutingDescription = virtualServer?.Server != null
            ? Localizer.GetFormat("Overlay_SmartRouting_Header", Localizer.GetCountryName(virtualServer.Server.HostCountry), Localizer.GetCountryName(virtualServer.Server.ExitCountry))
            : null;
    }

    protected void ClearSubItems()
    {
        SubItems.Clear();
        SubGroups.Clear();

        HasVirtualServers = false;
        SmartRoutingDescription = null;
    }

    private void GroupSubItems()
    {
        SubGroups.Reset(
            SubItems.GroupBy(item => item.GroupType)
                    .Select(group => ConnectionGroupFactory.GetGroup(group.Key, group, showHeader: !IsSubGroupHeaderHidden)));
    }

    [RelayCommand(CanExecute = nameof(CanShowSmartRoutingOverlay))]
    private Task ShowSmartRoutingOverlayAsync()
    {
        return OverlayActivator.ShowSmartRoutingInfoOverlayAsync();
    }

    private bool CanShowSmartRoutingOverlay()
    {
        return HasVirtualServers;
    }
}