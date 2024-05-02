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

using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Connections.Bases;
using ProtonVPN.Client.UI.Connections.Common.Factories;
using ProtonVPN.Client.UI.Connections.Common.Items;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Connections.P2P;

public class P2PCountriesPageViewModel : CountriesPageViewModelBase
{
    public override string? Title => Localizer.Get("Countries_P2P");

    public override string Description => Localizer.Get("Countries_P2P_Description");

    public override ImageSource IllustrationSource { get; } = ResourceHelper.GetIllustration("P2PCountriesIllustrationSource");

    public P2PCountriesPageViewModel(
        IMainViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter,
        LocationItemFactory locationItemFactory,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        ISettings settings)
        : base(viewNavigator,
               localizationProvider,
               logger,
               issueReporter,
               locationItemFactory,
               serversLoader,
               connectionManager,
               settings)
    { }

    protected override IEnumerable<LocationItemBase> GetItems()
    {
        LocationItemBase fastestCountry =
            LocationItemFactory.GetP2PCountry(string.Empty);

        IEnumerable<LocationItemBase> countries =
            ServersLoader.GetCountryCodesByFeatures(ServerFeatures.P2P)
                         .Select(LocationItemFactory.GetP2PCountry);

        return countries.Concat([fastestCountry]);
    }
}