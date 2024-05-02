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
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Models.Activation.Custom;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Connections.Common.Factories;

namespace ProtonVPN.Client.UI.Connections.Common.Items;

public abstract partial class CountryLocationItemBase : LocationItemBase
{
    public override string Header => Localizer.GetCountryName(ExitCountryCode);

    public override string? ToolTip =>
        IsRestricted
            ? Localizer.Get("Connections_Country_Restricted")
            : IsUnderMaintenance
                ? Localizer.Get("Connections_Country_UnderMaintenance")
                : null;

    public override bool IsCounted => !IsFastest;

    public override object FirstSortProperty => IsCounted;

    public override object SecondSortProperty => Header;

    public string ExitCountryCode { get; }

    public bool IsSecureCore { get; }

    public virtual string SecondaryActionLabel =>
        HasSubItems
            ? Localizer.GetPluralFormat("Connections_SeeCities", SubItemsCount)
            : string.Empty;

    protected bool IsFastest => string.IsNullOrEmpty(ExitCountryCode);

    protected override ILocationIntent LocationIntent => new CountryLocationIntent(ExitCountryCode);

    protected override string AutomationName => string.IsNullOrEmpty(ExitCountryCode) ? "Fastest" : ExitCountryCode;

    protected CountryLocationItemBase(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IMainViewNavigator mainViewNavigator,
        IUpsellCarouselDialogActivator upsellCarouselActivator,
        LocationItemFactory locationItemFactory,
        string exitCountryCode,
        bool isSecureCore)
        : base(localizer,
               serversLoader,
               connectionManager,
               mainViewNavigator,
               upsellCarouselActivator,
               locationItemFactory)
    {
        ExitCountryCode = exitCountryCode;
        IsSecureCore = isSecureCore;

        FetchSubItems();
    }

    public override bool MatchesSearchQuery(string searchQuery)
    {
        return IsFastest
            ? string.IsNullOrWhiteSpace(searchQuery)
            : base.MatchesSearchQuery(searchQuery);
    }

    public override void InvalidateIsActiveConnection(ConnectionDetails? currentConnectionDetails)
    {
        IsActiveConnection = currentConnectionDetails is not null
                          && !currentConnectionDetails.IsGateway
                          && ExitCountryCode == currentConnectionDetails.ExitCountryCode
                          && (FeatureIntent?.GetType().IsAssignableTo(currentConnectionDetails.OriginalConnectionIntent.Feature?.GetType()) ?? true);

        foreach (LocationItemBase item in SubItems)
        {
            item.InvalidateIsActiveConnection(currentConnectionDetails);
        }
    }

    protected override void OnSubItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        base.OnSubItemsCollectionChanged(sender, e);

        OnPropertyChanged(nameof(SecondaryActionLabel));
    }

    [RelayCommand]
    protected virtual Task NavigateToCountryAsync()
    {
        return Task.CompletedTask;
    }
}