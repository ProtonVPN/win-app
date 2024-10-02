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
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Legacy.Models.Activation.Custom;
using ProtonVPN.Client.Legacy.Models.Navigation;
using ProtonVPN.Client.Legacy.UI.Connections.Common.Enums;
using ProtonVPN.Client.Legacy.UI.Connections.Common.Factories;

namespace ProtonVPN.Client.Legacy.UI.Connections.Common.Items;

public class SecureCoreCountryLocationItem : CountryLocationItemBase
{
    public override GroupLocationType GroupType => GroupLocationType.SecureCoreCountries;

    public override string SecondaryActionLabel =>
        HasSubItems
            ? Localizer.GetPluralFormat("Connections_SeeServers", SubItemsCount)
            : string.Empty;

    public override IFeatureIntent? FeatureIntent => new SecureCoreFeatureIntent();

    public SecureCoreCountryLocationItem(
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
               true)
    { }

    protected override IEnumerable<LocationItemBase> GetSubItems()
    {
        return ServersLoader.GetSecureCoreCountryPairsByExitCountryCode(ExitCountryCode)
                            .Select(LocationItemFactory.GetSecureCoreCountryPair);
    }
}