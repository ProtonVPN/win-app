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

using ProtonVPN.Core.MVVM;
using ProtonVPN.Core.Vpn;

namespace ProtonVPN.Servers
{
    public class ServerTierSeparatorViewModel : ViewModel, IServerListItem
    {
        public InfoPopupViewModel InfoPopupViewModel { get; }

        public string Name { get; set; }

        private bool _showPopup;

        public bool ShowPopup
        {
            get => _showPopup;
            set => Set(ref _showPopup, value);
        }

        public bool IsToShowInfoIcon => InfoPopupViewModel != null;

        public bool Maintenance => false;
        public bool Connected => false;
        public sbyte Tier { get; set; }

        public ServerTierSeparatorViewModel(InfoPopupViewModel infoPopupViewModel)
        {
            InfoPopupViewModel = infoPopupViewModel;
        }

        public void OnVpnStateChanged(VpnState state)
        {
        }
    }
}