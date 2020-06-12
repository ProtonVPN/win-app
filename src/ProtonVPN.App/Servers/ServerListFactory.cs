/*
 * Copyright (c) 2020 Proton Technologies AG
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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Specs;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;

namespace ProtonVPN.Servers
{
    internal class ServerListFactory : IServersAware, IVpnStateAware
    {
        private readonly ServerManager _serverManager;
        private readonly IUserStorage _userStorage;
        private readonly SortedCountries _sortedCountries;

        private ObservableCollection<IServerListItem> _countries = new ObservableCollection<IServerListItem>();
        private VpnState _vpnState = new VpnState(VpnStatus.Disconnected);

        public ServerListFactory(
            ServerManager serverManager,
            IUserStorage userStorage,
            SortedCountries sortedCountries)
        {
            _sortedCountries = sortedCountries;
            _serverManager = serverManager;
            _userStorage = userStorage;
        }

        public ObservableCollection<IServerListItem> BuildServerList(string searchQuery = null)
        {
            var list = new ObservableCollection<IServerListItem>();

            searchQuery = searchQuery?.ToLower();

            if (string.IsNullOrEmpty(searchQuery))
            {
                _countries = CreateCountryList();

                foreach (var c in _countries)
                {
                    list.Add(c);
                }

                return list;
            }

            foreach (var item in _countries)
            {
                if (item.MatchesQuery(searchQuery))
                {
                    list.Add(item);
                }
                else
                {
                    if (item is ServersByCountryViewModel country)
                    {
                        var servers = GetMatchedServers(country, searchQuery);
                        if (servers.Count > 0)
                        {
                            list.Add(new ServersByCountryViewModel(country.CountryCode, _userStorage.User().MaxTier, _serverManager, _vpnState)
                            {
                                Servers = servers,
                                Expanded = true,
                            });
                        }
                    }
                }
            }

            return list;
        }

        public ObservableCollection<IServerListItem> BuildSecureCoreList()
        {
            var collection = new ObservableCollection<IServerListItem>();
            var countries = GetExitCountries();
            var user = _userStorage.User();

            foreach (var country in countries)
            {
                var row = new ServersByExitNodeViewModel(country, user.MaxTier, _serverManager);
                row.LoadServers();
                collection.Add(row);
            }

            return collection;
        }

        public void OnServersUpdated()
        {
            _countries = CreateCountryList();
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _vpnState = e.State;

            return Task.CompletedTask;
        }

        private ObservableCollection<IServerListItem> CreateCountryList()
        {
            var countries = _sortedCountries.List();
            var user = _userStorage.User();
            var list = new ObservableCollection<IServerListItem>();

            foreach (var country in countries)
            {
                var name = Countries.GetName(country);
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                list.Add(new ServersByCountryViewModel(country, user.MaxTier, _serverManager, _vpnState));
            }

            return list;
        }

        private IEnumerable<string> GetExitCountries()
        {
            var list = new List<string>();
            var servers = _serverManager.GetServers(new SecureCoreServer());
            foreach (var server in servers)
            {
                if (!list.Contains(server.ExitCountry))
                {
                    list.Add(server.ExitCountry);
                }
            }

            return list.OrderBy(Countries.GetName);
        }

        private ObservableCollection<IServerListItem> GetMatchedServers(ServersByCountryViewModel country, string searchQuery)
        {
            var servers = new ObservableCollection<IServerListItem>();

            foreach (var server in country.Servers)
            {
                if (server.MatchesQuery(searchQuery))
                {
                    servers.Add(server);
                }
            }

            return servers;
        }
    }
}
