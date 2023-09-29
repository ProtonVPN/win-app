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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ProtonVPN.Api.Contracts.Servers;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Abstract;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Servers.Specs;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Streaming;
using ProtonVPN.Translations;

namespace ProtonVPN.Servers
{
    public class ServersByCountryViewModel : BaseServerCollection
    {
        protected readonly IAppSettings AppSettings;
        protected readonly ServerManager ServerManager;
        protected readonly sbyte UserTier;

        private readonly VpnState _vpnConnectionStatus;
        private readonly IStreamingServices _streamingServices;

        private bool _tor;
        public bool Tor
        {
            get => _tor;
            set => Set(ref _tor, value);
        }

        private bool _p2P;
        public bool P2P
        {
            get => _p2P;
            set => Set(ref _p2P, value);
        }

        public bool IsVirtual => ServerManager.IsCountryVirtual(CountryCode);

        private bool _isB2B;
        public bool IsB2B => _isB2B;

        public ServersByCountryViewModel(
            string countryCode,
            sbyte userTier,
            IAppSettings appSettings,
            ServerManager serverManager,
            VpnState vpnConnectionStatus,
            IStreamingServices streamingServices)
            : this(isB2B: false, countryCode, userTier, appSettings, serverManager, vpnConnectionStatus, streamingServices)
        {
            UpgradeRequired = AppSettings.FeatureFreeRescopeEnabled && userTier == ServerTiers.Free;
        }

        protected ServersByCountryViewModel(bool isB2B,
            string countryCode,
            sbyte userTier,
            IAppSettings appSettings,
            ServerManager serverManager,
            VpnState vpnConnectionStatus,
            IStreamingServices streamingServices)
        {
            _isB2B = isB2B;
            CountryCode = countryCode;
            AppSettings = appSettings;
            ServerManager = serverManager;
            _vpnConnectionStatus = vpnConnectionStatus;
            UserTier = userTier;
            _streamingServices = streamingServices;
        }

        public override void LoadServers(string searchQuery = "", Features orderBy = Features.None)
        {
            ObservableCollection<IServerListItem> serverListItems = new();

            foreach (sbyte tier in GetTierOrderByUserTier())
            {
                IReadOnlyCollection<Server> servers = GetServersByTierAndSearchQuery(tier, searchQuery, orderBy).ToList();
                if (servers.Count == 0)
                {
                    continue;
                }

                SetFeatureFlags(servers);

                if (!IsB2B)
                {
                    serverListItems.Add(GetServerTierSeparator(tier, servers.Count));
                }

                foreach (Server server in servers)
                {
                    ServerItemViewModel item = new(server, UserTier, GetStreamingInfoPopup(tier));
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

        private IReadOnlyCollection<Server> GetServersByTierAndSearchQuery(sbyte tier, string searchQuery,
            Features orderBy)
        {
            IReadOnlyCollection<Server> list = ServerManager.GetServers(GetServerSpec(tier), orderBy);
            if (!string.IsNullOrEmpty(searchQuery))
            {
                list = list.Where(s => s.MatchesSearchCriteria(searchQuery)).ToList();
            }

            return list;
        }

        private IServerListItem GetServerTierSeparator(sbyte tier, int totalServers)
        {
            InfoPopupViewModel infoPopupViewModel = null;
            switch (tier)
            {
                case ServerTiers.Free:
                    infoPopupViewModel = GetFreeServersInfoPopup();
                    break;
                case ServerTiers.Plus:
                    infoPopupViewModel = GetStreamingInfoPopup(tier);
                    break;
            }

            return new ServerTierSeparatorViewModel(infoPopupViewModel)
            {
                Name = Translation.Format("Sidebar_Countries_ServerCountByTier" + tier, totalServers)
            };
        }

        private InfoPopupViewModel GetFreeServersInfoPopup()
        {
            return new InfoPopupViewModel(new FreeServersInfoPopupViewModel(),
                Translation.Format("Sidebar_Countries_Information"));
        }

        private InfoPopupViewModel GetStreamingInfoPopup(sbyte tier)
        {
            StreamingInfoPopupViewModel streamingInfoPopupViewModel = GetStreamingInfoPopupViewModel(tier);
            return streamingInfoPopupViewModel.StreamingServices.Count == 0
                ? null
                : new InfoPopupViewModel(streamingInfoPopupViewModel, Translation.Get("Sidebar_Streaming_Features"));
        }

        protected virtual StreamingInfoPopupViewModel GetStreamingInfoPopupViewModel(sbyte tier)
        {
            IReadOnlyList<StreamingService> services = _streamingServices.GetServices(CountryCode, tier);
            IReadOnlyList<StreamingServiceViewModel> list = services
                .Select(service => new StreamingServiceViewModel(service.Name, service.IconUrl)).ToList();

            return new StreamingInfoPopupViewModel(CountryCode, list);
        }

        protected virtual Specification<LogicalServerResponse> GetServerSpec(sbyte tier)
        {
            return new EntryCountryServer(CountryCode) && new StandardServer() && new ExactTierServer(tier);
        }

        public override void OnVpnStateChanged(VpnState state)
        {
            Connected = state.Status.Equals(VpnStatus.Connected)
                        && state.Server is Server server
                        && Servers.Any(s => s.Id is not null && s.Id == server.Id);

            foreach (IServerListItem s in Servers)
            {
                s.OnVpnStateChanged(state);
            }
        }

        public override bool HasAvailableServers()
        {
            if (!ServersAvailable.HasValue)
            {
                ServersAvailable = ServerManager.CountryHasAvailableServers(CountryCode, UserTier);
            }

            return ServersAvailable.Value;
        }

        private sbyte[] GetTierOrderByUserTier()
        {
            if (AppSettings.FeatureFreeRescopeEnabled && UserTier == ServerTiers.Free)
            {
                return new[] { ServerTiers.Internal, ServerTiers.Plus, ServerTiers.Basic };
            }

            return new[] { UserTier }.Concat(new[]
            {
                ServerTiers.Internal, ServerTiers.Plus, ServerTiers.Basic, ServerTiers.Free
            }.Except(new[] { UserTier })).ToArray();
        }

        public override bool Maintenance => ServerManager.CountryUnderMaintenance(CountryCode);
    }
}