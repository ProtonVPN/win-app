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

using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Config;
using ProtonVPN.Core.Abstract;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Servers.Specs;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Modals.Upsell;
using ProtonVPN.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Profile = ProtonVPN.Core.Profiles.Profile;

namespace ProtonVPN.Vpn.Connectors
{
    public class ProfileConnector
    {
        private readonly Random _random = new Random();

        private readonly ILogger _logger;
        private readonly IUserStorage _userStorage;
        private readonly IAppSettings _appSettings;
        private readonly ServerManager _serverManager;
        private readonly ServerCandidatesFactory _serverCandidatesFactory;
        private readonly IVpnServiceManager _vpnServiceManager;
        private readonly IVpnConfig _openVpnConfig;
        private readonly IModals _modals;
        private readonly IDialogs _dialogs;
        private readonly VpnCredentialProvider _vpnCredentialProvider;

        public ProfileConnector(
            ILogger logger,
            IUserStorage userStorage,
            IAppSettings appSettings,
            ServerManager serverManager,
            ServerCandidatesFactory serverCandidatesFactory,
            IVpnServiceManager vpnServiceManager,
            IVpnConfig openVpnConfig,
            IModals modals,
            IDialogs dialogs,
            VpnCredentialProvider vpnCredentialProvider)
        {
            _logger = logger;
            _vpnCredentialProvider = vpnCredentialProvider;
            _modals = modals;
            _dialogs = dialogs;
            _userStorage = userStorage;
            _serverManager = serverManager;
            _serverCandidatesFactory = serverCandidatesFactory;
            _appSettings = appSettings;
            _vpnServiceManager = vpnServiceManager;
            _openVpnConfig = openVpnConfig;
        }

        public ServerCandidates ServerCandidates(Profile profile)
        {
            if (profile == null)
                return _serverCandidatesFactory.ServerCandidates(new Server[0]);

            var serverSpec = ProfileServerSpec(profile);
            var servers = _serverManager.GetServers(serverSpec);
            return _serverCandidatesFactory.ServerCandidates(servers);
        }

        public bool CanConnect(ServerCandidates candidates, Profile profile)
        {
            var servers = Servers(candidates);

            if (servers.Any())
            {
                return SwitchSecureCoreMode(servers.First().IsSecureCore());
            }

            HandleNoServersAvailable(candidates.Items, profile);

            return false;
        }

        public async Task Connect(ServerCandidates candidates, Profile profile)
        {
            var servers = Servers(candidates);

            var sortedServers = Sorted(servers, profile.ProfileType);
            await Connect(sortedServers, VpnProtocol(profile.Protocol));
        }

        public async Task<bool> UpdateServers(ServerCandidates candidates, Profile profile)
        {
            var servers = Servers(candidates);
            if (!servers.Any()) return false;

            var sortedServers = Sorted(servers, profile.ProfileType);
            await UpdateServers(sortedServers);

            return true;
        }

        private IReadOnlyList<Server> Servers(ServerCandidates candidates)
        {
            var servers = candidates.Items
                .OnlineServers()
                .UpToTierServers(_userStorage.User().MaxTier)
                .ToList();

            return servers;
        }

        private IEnumerable<Server> Sorted(IEnumerable<Server> source, ProfileType profileType)
        {
            var random = new Random();
            return source.OrderBy(s => profileType == ProfileType.Random ? random.NextDouble() : s.Score);
        }

        private Specification<LogicalServerContract> ProfileServerSpec(Profile profile)
        {
            if (profile.ProfileType == ProfileType.Custom)
                return new ServerById(profile.ServerId);

            Specification<LogicalServerContract> spec = new ServerByFeatures(ServerFeatures(profile));

            if (!string.IsNullOrEmpty(profile.CountryCode))
                spec &= new ExitCountryServer(profile.CountryCode);

            return spec;
        }

        private Features ServerFeatures(Profile profile)
        {
            return profile.IsPredefined
                ? _appSettings.SecureCore ? Features.SecureCore : Features.None
                : profile.Features;
        }

        private void HandleNoServersAvailable(IReadOnlyCollection<Server> candidates, Profile profile)
        {
            if (profile.ProfileType == ProfileType.Custom)
            {
                HandleNoCustomServerAvailable(candidates.FirstOrDefault());
                return;
            }

            if ((profile.Features.IsSecureCore() || profile.IsPredefined && _appSettings.SecureCore) &&
                _userStorage.User().MaxTier < ServerTiers.Plus)
            {
                _modals.Show<ScUpsellModalViewModel>();
                return;
            }

            if (!string.IsNullOrEmpty(profile.CountryCode))
            {
                HandleNoCountryServersAvailable(candidates);
                return;
            }

            if (!candidates.Any())
            {
                _dialogs.ShowWarning(StringResources.Get("Profiles_msg_NoServersAvailable"));
                return;
            }

            var userTierServers = candidates.UpToTierServers(_userStorage.User().MaxTier);

            if (!userTierServers.Any())
            {
                if (candidates.BasicServers().Any())
                {
                    _modals.Show<UpsellModalViewModel>();
                    return;
                }

                if (candidates.PlusServers().Any())
                {
                    _modals.Show<PlusUpsellModalViewModel>();
                    return;
                }
            }

            if (!candidates.OnlineServers().Any())
            {
                _dialogs.ShowWarning(StringResources.Get("Profiles_msg_AllServersOffline"));
                return;
            }

            _modals.Show<NoServerDueTierUpsellModalViewModel>();
        }

        private void HandleNoCountryServersAvailable(IReadOnlyCollection<Server> candidates)
        {
            if (!candidates.Any())
            {
                _dialogs.ShowWarning(StringResources.Get("Profiles_msg_NoServersAvailable"));
                return;
            }

            var userTierServers = candidates.UpToTierServers(_userStorage.User().MaxTier);

            if (!userTierServers.Any())
            {
                _modals.Show<NoServerInCountryDueTierUpsellModalViewModel>();
                return;
            }

            if (!candidates.OnlineServers().Any())
            {
                _dialogs.ShowWarning(StringResources.Get("Profiles_msg_CountryOffline"));
                return;
            }

            _modals.Show<NoServerInCountryDueTierUpsellModalViewModel>();
        }

        private VpnProtocol VpnProtocol(Protocol protocol)
        {
            switch (protocol)
            {
                case Protocol.OpenVpnUdp:
                    return Common.Vpn.VpnProtocol.OpenVpnUdp;
                case Protocol.OpenVpnTcp:
                    return Common.Vpn.VpnProtocol.OpenVpnTcp;
                default:
                    return Common.Vpn.VpnProtocol.Auto;
            }
        }

        private Common.Vpn.VpnConfig VpnConfig()
        {
            var portConfig = new Dictionary<VpnProtocol, IReadOnlyCollection<int>>
            {
                { Common.Vpn.VpnProtocol.OpenVpnUdp, _openVpnConfig.UdpPorts() },
                { Common.Vpn.VpnProtocol.OpenVpnTcp, _openVpnConfig.TcpPorts() },
            };

            return new Common.Vpn.VpnConfig(
                portConfig,
                (from ip in _appSettings.CustomDnsIps where ip.Enabled select ip.Ip).ToList());
        }

        private async Task Connect(IEnumerable<Server> servers, VpnProtocol protocol)
        {
            var request = new VpnConnectionRequest(
                Servers(servers),
                protocol,
                VpnConfig(),
                _vpnCredentialProvider.Credentials());

            await _vpnServiceManager.Connect(request);

            _logger.Info("Connect requested");
            _modals.CloseAll();
        }

        private async Task UpdateServers(IEnumerable<Server> servers)
        {
            await _vpnServiceManager.UpdateServers(
                Servers(servers),
                VpnConfig());
        }

        private bool SwitchSecureCoreMode(bool secureCore)
        {
            if (TryConnectToSecureCoreFromNonSecureCoreMode(secureCore))
                return false;

            if (TryConnectToNonSecureCoreFromSecureCoreMode(secureCore))
                return false;

            return true;
        }

        private void HandleNoCustomServerAvailable(Server server)
        {
            if (server == null)
            {
                HandleEmptyServer();
                return;
            }

            if (_userStorage.User().MaxTier < server.Tier)
            {
                HandleUserTierTooLow(server);
                return;
            }

            if (!server.Online())
            {
                HandleOfflineServer();
            }
        }

        private void HandleEmptyServer()
        {
            _dialogs.ShowWarning(StringResources.Get("Servers_msg_CantConnect_Missing"));
        }

        private void HandleOfflineServer()
        {
            _dialogs.ShowWarning(StringResources.Get("Servers_msg_CantConnect_Maintenance"));
        }

        private void HandleUserTierTooLow(Server server)
        {
            if (server.IsSecureCore())
            {
                _modals.Show<ScUpsellModalViewModel>();
                return;
            }

            switch (server.Tier)
            {
                case ServerTiers.Basic:
                    {
                        _modals.Show<UpsellModalViewModel>();
                        break;
                    }
                case ServerTiers.Plus:
                    {
                        _modals.Show<PlusUpsellModalViewModel>();
                        break;
                    }
            }
        }

        private bool TryConnectToSecureCoreFromNonSecureCoreMode(bool secureCore)
        {
            if (!secureCore || _appSettings.SecureCore)
                return false;

            var result = _dialogs.ShowQuestion(StringResources.Get("Servers_msg_EnableSecureCoreConfirm"));
            if (result.HasValue && result.Value)
            {
                _appSettings.SecureCore = true;
                return false;
            }

            return true;
        }

        private bool TryConnectToNonSecureCoreFromSecureCoreMode(bool secureCore)
        {
            if (secureCore || !_appSettings.SecureCore)
                return false;

            var result = _dialogs.ShowQuestion(StringResources.Get("Servers_msg_DisableSecureCoreConfirm"));
            if (result.HasValue && result.Value)
            {
                _appSettings.SecureCore = false;
                return false;
            }

            return true;
        }

        private IReadOnlyList<VpnHost> Servers(IEnumerable<Server> servers)
        {
            return servers
                .SelectMany(s => s.Servers.OrderBy(_ => _random.Next()))
                .Where(s => s.Status != 0)
                .Select(s => new VpnHost(s.Domain, s.EntryIp))
                .Distinct(s => s.Ip)
                .ToList();
        }
    }
}
