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
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Factories;
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

namespace ProtonVPN.Client.Models.Connections;

public abstract partial class CountryLocationItemBase : HostLocationItemBase<Country>
{
    [ObservableProperty]
    private bool _isCountryExpanded;

    public Country Country { get; }

    public string ExitCountryCode => Country.Code;

    public override string Header => Localizer.GetCountryName(ExitCountryCode);

    public override string? ToolTip =>
        IsRestricted
            ? Localizer.Get("Connections_Country_Restricted")
            : IsUnderMaintenance
                ? Localizer.Get("Connections_Country_UnderMaintenance")
                : null;

    public override ILocationIntent LocationIntent { get; }

    public bool IsSecureCore => FeatureIntent is SecureCoreFeatureIntent;

    public override VpnTriggerDimension VpnTriggerDimension => IsSearchItem
        ? VpnTriggerDimension.SearchCountry
        : VpnTriggerDimension.CountriesCountry;

    protected override string AutomationName => ExitCountryCode;

    protected CountryLocationItemBase(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IMainWindowOverlayActivator overlayActivator,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        IConnectionGroupFactory connectionGroupFactory,
        ILocationItemFactory locationItemFactory,
        Country country,
        bool isSearchItem)
        : base(localizer,
               serversLoader,
               connectionManager,
               overlayActivator,
               upsellCarouselWindowActivator,
               connectionGroupFactory,
               locationItemFactory,
               country,
               isSearchItem)
    {
        Country = country;
        IsUnderMaintenance = country.IsUnderMaintenance();
        LocationIntent = SingleCountryLocationIntent.From(ExitCountryCode);
    }

    public void OnExpandCountry()
    {
        FetchSubItems();
    }

    public void OnCollapseCountry()
    {
        ClearSubItems();
    }

    protected override bool MatchesActiveConnection(ConnectionDetails? currentConnectionDetails)
    {
        return currentConnectionDetails is not null
            && !currentConnectionDetails.IsGateway
            && ExitCountryCode == currentConnectionDetails.ExitCountryCode
            && (FeatureIntent?.GetType().IsAssignableTo(currentConnectionDetails.OriginalConnectionIntent.Feature?.GetType()) ?? true);
    }

    partial void OnIsCountryExpandedChanged(bool value)
    {
        if (value)
        {
            OnExpandCountry();
        }
        else
        {
            OnCollapseCountry();
        }
    }
}