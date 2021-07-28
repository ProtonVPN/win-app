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
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Abstract;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Servers.Specs;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Streaming;
using ProtonVPN.Translations;

namespace ProtonVPN.Servers
{
    internal class ServersByCountryViewModel : BaseServerCollection
    {
        private readonly sbyte _userTier;
        private readonly ServerManager _serverManager;
        private readonly VpnState _vpnConnectionStatus;
        private readonly IStreamingServices _streamingServices;

        private bool _tor;
        private bool _p2P;

        public bool Tor
        {
            get => _tor;
            set => Set(ref _tor, value);
        }

        public bool P2P
        {
            get => _p2P;
            set => Set(ref _p2P, value);
        }

        public bool IsVirtual => _serverManager.IsCountryVirtual(CountryCode);

        public ServersByCountryViewModel(
            string countryCode,
            sbyte userTier,
            ServerManager serverManager,
            VpnState vpnConnectionStatus,
            IStreamingServices streamingServices)
        {
            CountryCode = countryCode;
            _serverManager = serverManager;
            _vpnConnectionStatus = vpnConnectionStatus;
            _userTier = userTier;
            _streamingServices = streamingServices;
        }

        public override void LoadServers(string searchQuery = "")
        {
            var serverListItems = new ObservableCollection<IServerListItem>();

            foreach (sbyte tier in GetTierOrderByUserTier())
            {
                IReadOnlyCollection<Server> servers = GetServersByTierAndSearchQuery(tier, searchQuery);
                if (servers.Count == 0)
                {
                    continue;
                }

                SetFeatureFlags(servers);
                serverListItems.Add(GetServerTierSeparator(tier, servers.Count));

                foreach (Server server in servers)
                {
                    var item = new ServerItemViewModel(server, _userTier, GetStreamingInfoPopupViewModel((sbyte)server.Tier));
                    item.OnVpnStateChanged(_vpnConnectionStatus);
                    serverListItems.Add(item);
                }
            }

            if (!string.IsNullOrEmpty(searchQuery) && serverListItems.Count > 0)
            {
                Expanded = true;
            }

            Servers = serverListItems;
        }

        private void SetFeatureFlags(IReadOnlyCollection<Server> servers)
        {
            if (!Tor && servers.Any(s => ServerFeatures.SupportsTor(s.Features)))
            {
                Tor = true;
            }

            if (!P2P && servers.Any(s => ServerFeatures.SupportsP2P(s.Features)))
            {
                P2P = true;
            }
        }

        private IReadOnlyCollection<Server> GetServersByTierAndSearchQuery(sbyte tier, string searchQuery)
        {
            IReadOnlyCollection<Server> list = _serverManager.GetServers(GetServerSpec(tier));
            if (!string.IsNullOrEmpty(searchQuery))
            {
                list = list.Where(s => s.MatchesSearchCriteria(searchQuery)).ToList();
            }

            return list;
        }

        private IServerListItem GetServerTierSeparator(sbyte tier, int totalServers)
        {
            return new ServerTierSeparatorViewModel(GetStreamingInfoPopupViewModel(tier))
            {
                Name = Translation.Format("Sidebar_Countries_ServerCountByTier" + tier, totalServers)
            };
        }

        private StreamingInfoPopupViewModel GetStreamingInfoPopupViewModel(sbyte tier)
        {
            IReadOnlyList<StreamingService> services = _streamingServices.GetServices(CountryCode, tier);
            IReadOnlyList<StreamingServiceViewModel> list = services
                .Select(service => new StreamingServiceViewModel(service.Name, service.IconUrl)).ToList();

            return new StreamingInfoPopupViewModel(CountryCode, list);
        }

        private Specification<LogicalServerContract> GetServerSpec(sbyte tier)
        {
            return new EntryCountryServer(CountryCode) && new StandardServer() && new ExactTierServer(tier);
        }

        public override void OnVpnStateChanged(VpnState state)
        {
            Connected = state.Status.Equals(VpnStatus.Connected)
                        && state.Server is Server server
                        && server.EntryCountry.Equals(CountryCode);

            foreach (IServerListItem s in Servers)
            {
                s.OnVpnStateChanged(state);
            }
        }

        public override bool HasAvailableServers()
        {
            if (!ServersAvailable.HasValue)
            {
                ServersAvailable = _serverManager.CountryHasAvailableServers(CountryCode, _userTier);
            }

            return ServersAvailable.Value;
        }

        private sbyte[] GetTierOrderByUserTier()
        {
            return new[]{_userTier}.Concat(new[]
                {
                    ServerTiers.Internal,
                    ServerTiers.Plus,
                    ServerTiers.Basic,
                    ServerTiers.Free
                }.Except(new[] {_userTier})).ToArray();
        }

        public override bool Maintenance => _serverManager.CountryUnderMaintenance(CountryCode);
    }
}