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
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Factories;

using ProtonVPN.Client.UI.Main.Sidebar.Connections.Bases.ViewModels;
using ProtonVPN.Client.Models.Connections;
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;

namespace ProtonVPN.Client.UI.Main.Sidebar.Connections.Countries.All;

public class AllCountriesComponentViewModel : CountriesComponentViewModelBase
{
    public override CountriesConnectionType ConnectionType { get; } = CountriesConnectionType.All;

    public override string Header => Localizer.Get("Countries_All");

    public override int SortIndex { get; } = 0;

    public override string Description => string.Empty;

    public override bool IsInfoBannerVisible => false;

    protected override ModalSources UpsellModalSources => ModalSources.Countries;

    public AllCountriesComponentViewModel(
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        ISettings settings,
        IServersLoader serversLoader,
        ILocationItemFactory locationItemFactory,
        IUrlsBrowser urlsBrowser,
        IWebAuthenticator webAuthenticator)
        : base(localizer,
               logger,
               issueReporter,
               settings,
               serversLoader,
               locationItemFactory,
               urlsBrowser,
               webAuthenticator)
    { }

    public override IEnumerable<ConnectionItemBase> GetItems()
    {
        IEnumerable<ConnectionItemBase> genericCountries = base.GetItems();

        IEnumerable<ConnectionItemBase> countries =
            ServersLoader.GetCountries()
                         .Select(LocationItemFactory.GetCountry);

        return genericCountries
            .Concat(countries);
    }

    protected override void DismissInfoBanner()
    { }
}