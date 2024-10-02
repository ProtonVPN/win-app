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
using ProtonVPN.Client.Common.Enums;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Legacy.Models.Activation.Custom;
using ProtonVPN.Client.Legacy.Models.Navigation;
using ProtonVPN.Client.Legacy.UI.Connections.Common.Enums;
using ProtonVPN.Client.Legacy.UI.Connections.Common.Factories;

namespace ProtonVPN.Client.Legacy.UI.Connections.Common.Items;

public abstract partial class CountryLocationItemBase : LocationItemBase
{
    public override string? ToolTip =>
        IsRestricted
            ? Localizer.Get("Connections_Country_Restricted")
            : IsUnderMaintenance
                ? Localizer.Get("Connections_Country_UnderMaintenance")
                : null;

    public override bool IsCounted => IsSpecificCountry;

    public override object FirstSortProperty => IsCounted;

    public override object SecondSortProperty => IsCounted ? Header : IntentKind;

    public bool HasStatesItems => StatesItemsCount > 0;

    public bool HasCitiesItems => CitiesItemsCount > 0;
    public string ExitCountryCode { get; }
    public ConnectionIntentKind IntentKind { get; }
    public bool ExcludeMyCountry { get; }
    public bool IsSecureCore { get; }

    public virtual string SecondaryActionLabel =>
        HasSubItems
            ? HasStatesItems
                ? Localizer.GetPluralFormat("Connections_SeeStates", StatesItemsCount)
                : Localizer.GetPluralFormat("Connections_SeeCities", CitiesItemsCount)
            : string.Empty;

    public FlagType FlagType => IsSpecificCountry
        ? FlagType.Country
        : IntentKind switch
        {
            ConnectionIntentKind.Random => FlagType.Random,
            _ => FlagType.Fastest
        };

    public override ILocationIntent LocationIntent => new CountryLocationIntent(ExitCountryCode, IntentKind, ExcludeMyCountry);
    protected int StatesItemsCount { get; private set; }
    protected int CitiesItemsCount { get; private set; }
    protected bool IsSpecificCountry => !string.IsNullOrEmpty(ExitCountryCode);

    protected override string AutomationName => string.IsNullOrEmpty(ExitCountryCode)
        ? IntentKind switch
        {
            ConnectionIntentKind.Fastest when ExcludeMyCountry => "FastestExcludingMyCountry",
            ConnectionIntentKind.Random when ExcludeMyCountry => "RandomExcludingMyCountry",
            ConnectionIntentKind.Fastest => "Fastest",
            ConnectionIntentKind.Random => "Random",
            _ => throw new NotImplementedException($"Intent kind '{IntentKind}' is not supported."),
        }
        : ExitCountryCode;

    protected CountryLocationItemBase(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IMainViewNavigator mainViewNavigator,
        IUpsellCarouselDialogActivator upsellCarouselActivator,
        LocationItemFactory locationItemFactory,
        string exitCountryCode,
        ConnectionIntentKind intentKind,
        bool excludeMyCountry,
        bool isSecureCore)
        : base(localizer,
               serversLoader,
               connectionManager,
               mainViewNavigator,
               upsellCarouselActivator,
               locationItemFactory,
               localizer.GetCountryName(exitCountryCode, intentKind, excludeMyCountry))
    {
        ExitCountryCode = exitCountryCode;
        IntentKind = intentKind;
        ExcludeMyCountry = excludeMyCountry;
        IsSecureCore = isSecureCore;

        FetchSubItems();
    }

    public override bool MatchesSearchQuery(string searchQuery)
    {
        return IsSpecificCountry
            ? base.MatchesSearchQuery(searchQuery)
            : string.IsNullOrWhiteSpace(searchQuery);
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

        StatesItemsCount = SubItems
            .Count(item => item.IsCounted
                        && item.GroupType is GroupLocationType.States or GroupLocationType.P2PStates);
        CitiesItemsCount = SubItems
            .Count(item => item.IsCounted
                        && item.GroupType is GroupLocationType.Cities or GroupLocationType.P2PCities);

        OnPropertyChanged(nameof(HasStatesItems));
        OnPropertyChanged(nameof(HasCitiesItems));
        OnPropertyChanged(nameof(SecondaryActionLabel));
    }

    [RelayCommand]
    protected virtual Task NavigateToCountryAsync()
    {
        return Task.CompletedTask;
    }
}