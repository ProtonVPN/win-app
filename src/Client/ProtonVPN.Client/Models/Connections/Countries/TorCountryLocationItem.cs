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
using ProtonVPN.Client.Contracts.Enums;
using ProtonVPN.Client.Contracts.Services.Activation;
using ProtonVPN.Client.Factories;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Bases.Models;

namespace ProtonVPN.Client.Models.Connections.Countries;

public class TorCountryLocationItem : CountryLocationItemBase
{
    public override ConnectionGroupType GroupType { get; } = ConnectionGroupType.TorCountries;

    public override IFeatureIntent? FeatureIntent { get; } = new TorFeatureIntent();

    public TorCountryLocationItem(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IMainWindowOverlayActivator overlayActivator,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        IConnectionGroupFactory connectionGroupFactory,
        ILocationItemFactory locationItemFactory,
        string exitCountryCode)
        : base(localizer,
               serversLoader,
               connectionManager,
               overlayActivator,
               upsellCarouselWindowActivator,
               connectionGroupFactory,
               locationItemFactory,
               exitCountryCode)
    { }

    protected override IEnumerable<LocationItemBase> GetSubItems()
    {
        return ServersLoader.GetServersByFeaturesAndCountryCode(ServerFeatures.Tor, ExitCountryCode)
                            .Select(LocationItemFactory.GetTorServer);
    }
}