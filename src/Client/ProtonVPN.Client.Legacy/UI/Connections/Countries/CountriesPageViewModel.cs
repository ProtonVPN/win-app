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
using ProtonVPN.Client.Legacy.Helpers;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Legacy.Models.Navigation;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Legacy.UI.Connections.Bases;
using ProtonVPN.Client.Legacy.UI.Connections.Common.Factories;
using ProtonVPN.Client.Legacy.UI.Connections.Common.Items;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Legacy.UI.Connections.Countries;

public class CountriesPageViewModel : CountriesPageViewModelBase
{
    public override string? Title => Localizer.Get("Countries_Page_Title");

    public override string Description => Settings.VpnPlan.IsPaid
        ? string.Empty
        : Localizer.Get("Countries_Page_FreeUser_Description");

    public override ImageSource IllustrationSource { get; } = ResourceHelper.GetIllustration("AllCountriesIllustrationSource");

    public CountriesPageViewModel(
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
        IEnumerable<LocationItemBase> genericCountries =
        [
            LocationItemFactory.GetGenericCountry(ConnectionIntentKind.Fastest, false),
            LocationItemFactory.GetGenericCountry(ConnectionIntentKind.Fastest, true),
            LocationItemFactory.GetGenericCountry(ConnectionIntentKind.Random, false),
        ];

        IEnumerable<LocationItemBase> countries =
            ServersLoader.GetCountryCodes()
                         .Select(LocationItemFactory.GetCountry);

        IEnumerable<LocationItemBase> freeServers =
            ServersLoader.GetFreeServers()
                         .Select(LocationItemFactory.GetServer);

        return genericCountries
            .Concat(countries)
            .Concat(freeServers);
    }

    protected override void InvalidateRestrictions()
    {
        base.InvalidateRestrictions();

        if (IsActive)
        {
            OnPropertyChanged(nameof(Description));
        }
    }
}