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
using ProtonVPN.Translations;

namespace ProtonVPN.Servers
{
    public class FastestServerViewModel : ViewModel, IServerListItem
    {
        public string Id => null;
        public string Name { get; set; } = Translation.Get("Servers_Fastest");
        public bool Maintenance => false;
        public bool Connected => false;
        public bool Dimmed => false;
        public bool Expanded => false;
        public bool UpgradeRequired => false;

        public void OnVpnStateChanged(VpnState state)
        {
        }
    }
}