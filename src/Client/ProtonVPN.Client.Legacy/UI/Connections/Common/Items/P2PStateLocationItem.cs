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
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Legacy.Models.Activation.Custom;
using ProtonVPN.Client.Legacy.Models.Navigation;
using ProtonVPN.Client.Legacy.UI.Connections.Common.Enums;
using ProtonVPN.Client.Legacy.UI.Connections.Common.Factories;

namespace ProtonVPN.Client.Legacy.UI.Connections.Common.Items;

public class P2PStateLocationItem : StateLocationItemBase
{
    public override GroupLocationType GroupType => GroupLocationType.P2PStates;

    public override IFeatureIntent? FeatureIntent => new P2PFeatureIntent();

    public P2PStateLocationItem(
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
            ServersLoader.GetCitiesByFeaturesAndState(ServerFeatures.P2P, State)
                         .Select(city => LocationItemFactory.GetP2PCity(city, showBaseLocation: true));

        IEnumerable <LocationItemBase> servers =
            ServersLoader.GetServersByFeaturesAndState(ServerFeatures.P2P, State)
                         .Select(LocationItemFactory.GetP2PServer);

        return cities.Concat(servers);
    }
}
