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
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Models.Activation.Custom;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Connections.Common.Enums;
using ProtonVPN.Client.UI.Connections.Common.Factories;

namespace ProtonVPN.Client.UI.Connections.Common.Items;

public class StateLocationItem : StateLocationItemBase
{
    public override GroupLocationType GroupType => GroupLocationType.States;

    public StateLocationItem(
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
               locationItemFactory,
               state,
               showBaseLocation)
    { }

    protected override IEnumerable<LocationItemBase> GetSubItems()
    {
        IEnumerable<LocationItemBase> cities =
            ServersLoader.GetCitiesByState(State)
                         .Select(city => LocationItemFactory.GetCity(city, showBaseLocation: true));

        IEnumerable<LocationItemBase> servers =
            ServersLoader.GetServersByState(State)
                         .Select(LocationItemFactory.GetServer);

        return cities.Concat(servers);
    }
}