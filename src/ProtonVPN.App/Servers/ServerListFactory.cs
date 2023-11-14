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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ProtonVPN.Announcements.Contracts;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Config.Url;
using ProtonVPN.Core.Models;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Servers.Specs;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Streaming;
using ProtonVPN.Translations;

namespace ProtonVPN.Servers
{
    public class ServerListFactory : IVpnStateAware
    {
        private readonly IAppSettings _appSettings;
        private readonly ServerManager _serverManager;
        private readonly IUserStorage _userStorage;
        private readonly IStreamingServices _streamingServices;
        private readonly UpsellBannerViewModel _upsellBannerViewModel;
        private readonly AnnouncementBannerViewModel _announcementBannerViewModel;
        private readonly IActiveUrls _urls;
        private readonly IAnnouncementService _announcementService;
        private VpnState _vpnState = new(VpnStatus.Disconnected);

        public ServerListFactory(
            IAppSettings appSettings,
            ServerManager serverManager,
            IUserStorage userStorage,
            IStreamingServices streamingServices,
            UpsellBannerViewModel upsellBannerViewModel,
            AnnouncementBannerViewModel announcementBannerViewModel,
            IActiveUrls urls,
            IAnnouncementService announcementService)
        {
            _appSettings = appSettings;
            _serverManager = serverManager;
            _userStorage = userStorage;
            _streamingServices = streamingServices;
            _upsellBannerViewModel = upsellBannerViewModel;
            _announcementBannerViewModel = announcementBannerViewModel;
            _urls = urls;
            _announcementService = announcementService;
        }

        public ObservableCollection<IServerListItem> BuildServerList(string searchQuery = null)
        {
            return _userStorage.GetUser().Paid()
                ? GetPaidUserLocations(searchQuery)
                : GetFreeUserLocations(searchQuery);
        }

        private ObservableCollection<IServerListItem> GetFreeUserLocations(string searchQuery)
        {
            List<IServerListItem> serverListItems = new();
            if (_appSettings.FeatureFreeRescopeEnabled)
            {
                serverListItems.Add(new FreeConnectionSeparatorViewModel { Name = Translation.Get("Servers_FreeConnections") });
                serverListItems.Add(new FastestServerViewModel());
                serverListItems.AddRange(GetPaidUserLocations(searchQuery));
            }
            else
            {
                IServerListItem banner = GetServerListAnnouncementBanner();
                if (banner is not null)
                {
                    serverListItems.Add(banner);
                }

                AddB2BServers(serverListItems, searchQuery);

                IList<string> freeCountries = GetCountriesByTiers(ServerTiers.Free);
                IList<string> plusCountries = GetCountriesByTiers(ServerTiers.Plus).Except(freeCountries).ToList();

                IList<IServerListItem> freeLocationViewModels = CreateServersByCountryViewModels(freeCountries, searchQuery).ToList();
                serverListItems.AddRange(GetServerGroupViewModels(freeLocationViewModels,
                    Translation.Format("Sidebar_Countries_FreeLocationCount", freeLocationViewModels.Count)));

                IList<IServerListItem> plusLocationsViewModels = CreateServersByCountryViewModels(plusCountries, searchQuery).ToList();
                serverListItems.AddRange(GetServerGroupViewModels(plusLocationsViewModels,
                    Translation.Format("Sidebar_Countries_PlusLocationCount", plusLocationsViewModels.Count)));
            }

            return new ObservableCollection<IServerListItem>(serverListItems);
        }

        private void AddB2BServers(List<IServerListItem> serverListItems, string searchQuery)
        {
            IList<Server> b2bList = _serverManager.GetServers(new B2BServer()).ToList();
            List<IServerListItem> b2bViewModels = CreateServersByB2BCountryViewModels(b2bList, searchQuery).ToList();
            if (b2bViewModels.Count > 0)
            {
                serverListItems.Add(CreateB2BCountryListSeparator(Translation.Get("ServerType_val_B2B")));
                serverListItems.AddRange(b2bViewModels);
            }
        }

        private ObservableCollection<IServerListItem> GetPaidUserLocations(string searchQuery)
        {
            List<IServerListItem> serverListItems = new();
            AddB2BServers(serverListItems, searchQuery);

            IList<string> countries = _serverManager.GetEntryCountriesBySpec(new StandardServer())
                .OrderBy(Countries.GetName)
                .ToList();
            IList<IServerListItem> plusLocationViewModels = CreateServersByCountryViewModels(countries, searchQuery).ToList();
            if (plusLocationViewModels.Count > 0)
            {
                serverListItems.Add(CreateCountryListSeparator(Translation.Format("Sidebar_Countries_AllLocationCount", plusLocationViewModels.Count)));
                IServerListItem banner = GetServerListAnnouncementBanner();
                if (banner is not null)
                {
                    serverListItems.Add(banner);
                }
                serverListItems.AddRange(plusLocationViewModels);
            }

            return new ObservableCollection<IServerListItem>(serverListItems);
        }

        private IServerListItem GetServerListAnnouncementBanner()
        {
            if (!_announcementBannerViewModel.IsBannerVisible)
            {
                // Banner has been dismissed by the user. do not show anymore until next app start
                return null;
            }

            DateTime now = DateTime.UtcNow;
            Announcement announcement = _announcementService.Get()
                .Where(a => a.Type == AnnouncementType.Banner && a.StartDateTimeUtc <= now && a.EndDateTimeUtc > now && !a.Seen)
                .OrderBy(a => a.EndDateTimeUtc)
                .FirstOrDefault();

            _announcementBannerViewModel.Set(announcement);

            if (announcement is null)
            {
                if (!_userStorage.GetUser().Paid())
                {
                    return _upsellBannerViewModel;
                }
            }
            else
            {
                return _announcementBannerViewModel;
            }

            return null;
        }

        private IServerListItem CreateB2BCountryListSeparator(string name)
        {
            return new CountryB2BSeparatorViewModel(_urls) { Name = name };
        }

        private IEnumerable<IServerListItem> CreateServersByB2BCountryViewModels(IList<Server> b2bList, string searchQuery,
            Features orderBy = Features.None)
        {
            IList<string> gateways = b2bList.Select(s => s.GatewayName).Distinct().OrderBy(n => n).ToList();
            foreach (string gateway in gateways)
            {
                ServersByGatewayViewModel countryViewModel = new(gateway, _userStorage.GetUser().MaxTier,
                    _appSettings, _serverManager, _vpnState, _streamingServices);
                if (string.IsNullOrEmpty(searchQuery) || gateway.ToLower().Contains(searchQuery))
                {
                    countryViewModel.LoadServers(orderBy: orderBy);
                    yield return countryViewModel;
                }
                else if (!string.IsNullOrEmpty(searchQuery))
                {
                    countryViewModel.LoadServers(searchQuery, orderBy);
                    if (countryViewModel.Servers.Count > 0)
                    {
                        yield return countryViewModel;
                    }
                }
            }
        }

        private List<IServerListItem> GetServerGroupViewModels(IList<IServerListItem> viewModels, string groupName)
        {
            List<IServerListItem> list = new();
            if (viewModels.Count > 0)
            {
                list.Add(CreateCountryListSeparator(groupName));

                foreach (IServerListItem c in viewModels)
                {
                    list.Add(c);
                }
            }

            return list;
        }

        public ObservableCollection<IServerListItem> BuildSecureCoreList(string searchQuery = "")
        {
            ObservableCollection<IServerListItem> serverListItems = new ObservableCollection<IServerListItem>();
            IOrderedEnumerable<string> countries = _serverManager.GetSecureCoreCountries().OrderBy(Countries.GetName);
            User user = _userStorage.GetUser();

            foreach (string countryCode in countries)
            {
                if (string.IsNullOrEmpty(searchQuery) || Countries.MatchesSearch(countryCode, searchQuery))
                {
                    ServersByExitNodeViewModel row =
                        new ServersByExitNodeViewModel(countryCode, user.MaxTier, _serverManager);
                    row.LoadServers();
                    serverListItems.Add(row);
                }
            }

            return serverListItems;
        }

        public ObservableCollection<IServerListItem> BuildPortForwardingList(string searchQuery = null)
        {
            List<IServerListItem> serverListItems = new();
            AddB2BServers(serverListItems, searchQuery);

            IList<string> p2PList = _serverManager.GetEntryCountriesBySpec(new P2PServer()).OrderBy(Countries.GetName).ToList();
            IList<string> nonP2PList = _serverManager.GetCountries().Except(p2PList).OrderBy(Countries.GetName).ToList();

            List<IServerListItem> p2pViewModels = CreateServersByCountryViewModels(p2PList, searchQuery, Features.P2P).ToList();
            if (p2pViewModels.Count > 0)
            {
                serverListItems.Add(CreateCountryListSeparator(Translation.Get("ServerType_val_P2P")));
                serverListItems.AddRange(p2pViewModels);
            }

            List<IServerListItem> otherViewModels = CreateServersByCountryViewModels(nonP2PList, searchQuery).ToList();
            if (otherViewModels.Count > 0)
            {
                serverListItems.Add(CreateCountryListSeparator(Translation.Get("Sidebar_Separator_Others")));
                serverListItems.AddRange(otherViewModels);
            }

            return new ObservableCollection<IServerListItem>(serverListItems);
        }

        private IEnumerable<IServerListItem> CreateServersByCountryViewModels(IList<string> countries,
            string searchQuery, Features orderBy = Features.None)
        {
            foreach (string countryCode in countries)
            {
                ServersByCountryViewModel countryViewModel = new(countryCode, _userStorage.GetUser().MaxTier,
                    _appSettings, _serverManager, _vpnState, _streamingServices);
                if (string.IsNullOrEmpty(searchQuery) || Countries.MatchesSearch(countryCode, searchQuery))
                {
                    countryViewModel.LoadServers(orderBy: orderBy);
                }
                else if (!string.IsNullOrEmpty(searchQuery))
                {
                    countryViewModel.LoadServers(searchQuery, orderBy);
                }
                if (countryViewModel.Servers.Count > 0)
                {
                    yield return countryViewModel;
                }
            }
        }

        private IServerListItem CreateCountryListSeparator(string name)
        {
            return new CountrySeparatorViewModel(_urls) { Name = name };
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _vpnState = e.State;

            return Task.CompletedTask;
        }

        private List<string> GetCountriesByTiers(params sbyte[] tiers)
        {
            return _serverManager.GetCountriesByTier(tiers).OrderBy(Countries.GetName).ToList();
        }
    }
}