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

using ProtonVPN.Config.Url;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Sidebar.CountryFeatures;

namespace ProtonVPN.Servers
{
    public class CountrySeparatorViewModel : ViewModel, IServerListItem
    {
        public CountrySeparatorViewModel(IActiveUrls urls)
        {
            CountryFeaturesPopupViewModel = new CountryFeaturesPopupViewModel(urls);
        }

        public CountryFeaturesPopupViewModel CountryFeaturesPopupViewModel { get; }
        public string Name { get; set; }
        public bool Maintenance { get; } = false;
        public bool Connected { get; } = false;

        public void OnVpnStateChanged(VpnState state)
        {
        }
    }
}