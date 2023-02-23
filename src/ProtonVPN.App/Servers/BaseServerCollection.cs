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
using System.Collections.ObjectModel;
using ProtonVPN.Core.Servers;

namespace ProtonVPN.Servers
{
    public abstract class BaseServerCollection : ViewModel, IServerCollection
    {
        private bool _connected;
        private bool _upgradeRequired;

        protected ObservableCollection<IServerListItem> ServersValue;
        protected bool ExpandedValue;
        protected bool? ServersAvailable;

        public string CountryCode { get; set; }

        public ObservableCollection<IServerListItem> Servers
        {
            get
            {
                if (ServersValue == null)
                {
                    LoadServers();
                }

                return ServersValue;
            }
            set
            {
                ServersValue = value;
                OnPropertyChanged();
            }
        }

        public bool Dimmed => !HasAvailableServers() || Maintenance;

        public bool Expanded
        {
            get => ExpandedValue;
            set
            {
                if (ExpandedValue != value)
                {
                    ExpandedValue = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool Connected
        {
            get => _connected;
            set => Set(ref _connected, value);
        }

        public bool UpgradeRequired
        {
            get => _upgradeRequired;
            set => Set(ref _upgradeRequired, value);
        }

        public string Name => Countries.GetName(CountryCode);

        public bool IsMarkedForRemoval { get; set; } = false;

        public abstract void LoadServers(string searchQuery = "", Features orderBy = Features.None);

        public abstract void OnVpnStateChanged(VpnState state);

        public abstract bool HasAvailableServers();

        public abstract bool Maintenance { get; }
    }
}