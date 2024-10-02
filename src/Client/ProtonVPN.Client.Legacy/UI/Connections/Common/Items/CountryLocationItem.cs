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
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Legacy.Models.Activation.Custom;
using ProtonVPN.Client.Legacy.Models.Navigation;
using ProtonVPN.Client.Legacy.UI.Connections.Common.Enums;
using ProtonVPN.Client.Legacy.UI.Connections.Common.Factories;
using ProtonVPN.Client.Legacy.UI.Connections.Countries;

namespace ProtonVPN.Client.Legacy.UI.Connections.Common.Items;

public class CountryLocationItem : CountryLocationItemBase
{
    public override GroupLocationType GroupType => GroupLocationType.Countries;

    public CountryLocationItem(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IMainViewNavigator mainViewNavigator,
        IUpsellCarouselDialogActivator upsellCarouselActivator,
        LocationItemFactory locationItemFactory,
        string exitCountryCode,
        ConnectionIntentKind intentKind = ConnectionIntentKind.Fastest,
        bool excludeMyCountry = false)
        : base(localizer,
               serversLoader,
               connectionManager,
               mainViewNavigator,
               upsellCarouselActivator,
               locationItemFactory,
               exitCountryCode,
               intentKind,
               excludeMyCountry,
               false)
    { }

    protected override IEnumerable<LocationItemBase> GetSubItems()
    {
        IEnumerable<LocationItemBase> states =
            ServersLoader.GetStatesByCountryCode(ExitCountryCode)
                         .Select(state => LocationItemFactory.GetState(state, showBaseLocation: true));

        return states.Any()
            ? states
            : ServersLoader.GetCitiesByCountryCode(ExitCountryCode)
                           .Select(city => LocationItemFactory.GetCity(city, showBaseLocation: true));
    }

    protected override async Task NavigateToCountryAsync()
    {
        await MainViewNavigator.NavigateToAsync<CountryPageViewModel>(ExitCountryCode);
    }
}