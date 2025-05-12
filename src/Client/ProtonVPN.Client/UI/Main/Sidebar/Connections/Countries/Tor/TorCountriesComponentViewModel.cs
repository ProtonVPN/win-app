/*
 * Copyright (c) 2024 Proton AG
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

using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Factories;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Models.Connections;
using ProtonVPN.Client.Services.Upselling;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Bases.ViewModels;
using ProtonVPN.StatisticalEvents.Contracts;

namespace ProtonVPN.Client.UI.Main.Sidebar.Connections.Countries.Tor;

public class TorCountriesComponentViewModel : CountriesComponentViewModelBase
{
    public override CountriesConnectionType ConnectionType { get; } = CountriesConnectionType.Tor;

    public override string Header => Localizer.Get("Countries_Tor");

    public override string Description => Localizer.Get("Countries_Tor_Description");

    public override bool IsInfoBannerVisible => !IsUpsellBannerVisible
                                             && !Settings.IsTorInfoBannerDismissed;

    public override int SortIndex { get; } = 3;

    protected override ModalSource UpsellModalSource => ModalSource.Tor;

    public TorCountriesComponentViewModel(
        ISettings settings,
        IServersLoader serversLoader,
        ILocationItemFactory locationItemFactory,
        IViewModelHelper viewModelHelper,
        IAccountUpgradeUrlLauncher accountUpgradeUrlLauncher)
        : base(settings,
               serversLoader,
               locationItemFactory,
               viewModelHelper,
               accountUpgradeUrlLauncher)
    { }

    public override IEnumerable<ConnectionItemBase> GetItems()
    {
        IEnumerable<ConnectionItemBase> genericCountries = base.GetItems();

        IEnumerable<ConnectionItemBase> countries =
            ServersLoader.GetCountriesByFeatures(ServerFeatures.Tor)
                         .Select(c => LocationItemFactory.GetTorCountry(c));

        return genericCountries
            .Concat(countries);
    }

    protected override void DismissInfoBanner()
    {
        Settings.IsTorInfoBannerDismissed = true;
    }

    protected override void OnSettingsChanged(string propertyName)
    {
        if (propertyName == nameof(ISettings.IsTorInfoBannerDismissed))
        {
            OnPropertyChanged(nameof(IsInfoBannerVisible));
        }
    }
}