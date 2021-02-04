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
using ProtonVPN.Core.Models;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Servers.Specs;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Translations;

namespace ProtonVPN.Servers
{
    internal class ServerListFactory : IServersAware, IVpnStateAware
    {
        private readonly ServerManager _serverManager;
        private readonly IUserStorage _userStorage;
        private readonly ISortedCountries _sortedCountries;

        private ObservableCollection<IServerListItem> _countries = new ObservableCollection<IServerListItem>();
        private VpnState _vpnState = new VpnState(VpnStatus.Disconnected);

        public ServerListFactory(
            ServerManager serverManager,
            IUserStorage userStorage,
            ISortedCountries sortedCountries)
        {
            _sortedCountries = sortedCountries;
            _serverManager = serverManager;
            _userStorage = userStorage;
        }

        public ObservableCollection<IServerListItem> BuildServerList(string searchQuery = null)
        {
            ObservableCollection<IServerListItem> list = new ObservableCollection<IServerListItem>();
            searchQuery = searchQuery?.ToLower();
            _countries = CreateCountryList();

            if (string.IsNullOrEmpty(searchQuery))
            {
                foreach (IServerListItem c in _countries)
                {
                    list.Add(c);
                }

                return list;
            }

            foreach (IServerListItem item in _countries)
            {
                if (item.MatchesQuery(searchQuery))
                {
                    list.Add(item);
                }
                else
                {
                    if (item is ServersByCountryViewModel country)
                    {
                        ObservableCollection<IServerListItem> servers = GetMatchedServers(country, searchQuery);
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
            ObservableCollection<IServerListItem> collection = new ObservableCollection<IServerListItem>();
            IEnumerable<string> countries = GetExitCountries();
            User user = _userStorage.User();

            foreach (string country in countries)
            {
                ServersByExitNodeViewModel row = new ServersByExitNodeViewModel(country, user.MaxTier, _serverManager);
                row.LoadServers();
                collection.Add(row);
            }

            return collection;
        }

        public ObservableCollection<IServerListItem> BuildPortForwardingList(string searchQuery = null)
        {
            searchQuery = searchQuery?.ToLower();
            User user = _userStorage.User();
            IReadOnlyCollection<Server> servers = _serverManager.GetServers(new StandardServer());

            IList<string> p2pList = servers
                .Where(s => s.SupportsP2P())
                .GroupBy(s => s.ExitCountry).Select(s => s.Key)
                .OrderBy(Countries.GetName).ToList();

            IList<string> nonP2PList = servers
                .GroupBy(s => s.ExitCountry).Select(s => s.Key)
                .Except(p2pList)
                .OrderBy(Countries.GetName).ToList();

            List<IServerListItem> list = new List<IServerListItem>();
            list.Add(CreateCountryListSeparator(Translation.Get("ServerType_val_P2P")));
            list.AddRange(CreateServersByCountryViewModels(user, p2pList, searchQuery));
            list.Add(CreateCountryListSeparator(Translation.Get("Sidebar_Separator_Others")));
            list.AddRange(CreateServersByCountryViewModels(user, nonP2PList, searchQuery));

            return new ObservableCollection<IServerListItem>(list);
        }

        private IEnumerable<IServerListItem> CreateServersByCountryViewModels(User user, IList<string> countryCodes, string searchQuery)
        {
            foreach (string countryCode in countryCodes)
            {
                ServersByCountryViewModel country = new ServersByCountryViewModel(countryCode, user.MaxTier, _serverManager, _vpnState);
                if (string.IsNullOrEmpty(searchQuery) || country.MatchesQuery(searchQuery))
                {
                    country.LoadServers();
                    yield return country;
                }
                else if (!string.IsNullOrEmpty(searchQuery))
                {
                    ObservableCollection<IServerListItem> servers = GetMatchedServers(country, searchQuery);
                    if (servers.Count > 0)
                    {
                        yield return new ServersByCountryViewModel(country.CountryCode, _userStorage.User().MaxTier, _serverManager, _vpnState)
                        {
                            Servers = servers,
                            Expanded = true,
                        };
                    }
                }
            }
        }

        private IServerListItem CreateCountryListSeparator(string name)
        {
            return new CountrySeparatorViewModel()
            {
                Name = name
            };
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
            List<string> countries = _sortedCountries.List();
            User user = _userStorage.User();
            ObservableCollection<IServerListItem> list = new ObservableCollection<IServerListItem>();

            foreach (string country in countries)
            {
                string name = Countries.GetName(country);
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
            IList<string> list = new List<string>();
            IReadOnlyCollection<Server> servers = _serverManager.GetServers(new SecureCoreServer());
            foreach (Server server in servers)
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
            ObservableCollection<IServerListItem> servers = new ObservableCollection<IServerListItem>();

            foreach (IServerListItem server in country.Servers)
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
