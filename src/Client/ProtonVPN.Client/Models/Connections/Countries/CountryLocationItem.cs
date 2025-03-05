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
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Factories;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Contracts.Enums;

namespace ProtonVPN.Client.Models.Connections.Countries;

public class CountryLocationItem : CountryLocationItemBase
{
    public override ConnectionGroupType GroupType { get; } = ConnectionGroupType.Countries;

    public override IFeatureIntent? FeatureIntent { get; } = null;

    protected override bool IsSubGroupHeaderHidden => true;

    public CountryLocationItem(
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
        IsUnderMaintenance = country.IsStandardUnderMaintenance;
    }

    protected override IEnumerable<ConnectionItemBase> GetSubItems()
    {
        IEnumerable<ConnectionItemBase> states =
            ServersLoader.GetStatesByCountryCode(ExitCountryCode)
                         .Select(state => LocationItemFactory.GetState(state, showBaseLocation: false, isSearchItem: IsSearchItem));

        return states.Any()
            ? states
            : ServersLoader.GetCitiesByCountryCode(ExitCountryCode)
                           .Select(city => LocationItemFactory.GetCity(city, showBaseLocation: false, isSearchItem: IsSearchItem));
    }
}