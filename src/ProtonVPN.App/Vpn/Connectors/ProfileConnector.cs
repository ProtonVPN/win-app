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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProtonVPN.Common;
using ProtonVPN.Common.Abstract;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Networking;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Abstract;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Servers.Specs;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Settings.Contracts;
using ProtonVPN.Core.Window.Popups;
using ProtonVPN.Modals.Upsell;
using ProtonVPN.Translations;
using ProtonVPN.Windows.Popups.Delinquency;
using Profile = ProtonVPN.Core.Profiles.Profile;

namespace ProtonVPN.Vpn.Connectors
{
    public class ProfileConnector
    {
        private readonly Random _random = new();

        private readonly ILogger _logger;
        private readonly IUserStorage _userStorage;
        private readonly IAppSettings _appSettings;
        private readonly ServerManager _serverManager;
        private readonly ServerCandidatesFactory _serverCandidatesFactory;
        private readonly IVpnServiceManager _vpnServiceManager;
        private readonly IModals _modals;
        private readonly IDialogs _dialogs;
        private readonly IVpnCredentialProvider _vpnCredentialProvider;
        private readonly IPopupWindows _popupWindows;
        private readonly IDelinquencyPopupViewModel _delinquencyPopupViewModel;

        public ProfileConnector(
            ILogger logger,
            IUserStorage userStorage,
            IAppSettings appSettings,
            ServerManager serverManager,
            ServerCandidatesFactory serverCandidatesFactory,
            IVpnServiceManager vpnServiceManager,
            IModals modals,
            IDialogs dialogs,
            IVpnCredentialProvider vpnCredentialProvider,
            IPopupWindows popupWindows,
            IDelinquencyPopupViewModel delinquencyPopupViewModel)
        {
            _logger = logger;
            _userStorage = userStorage;
            _appSettings = appSettings;
            _serverManager = serverManager;
            _serverCandidatesFactory = serverCandidatesFactory;
            _vpnServiceManager = vpnServiceManager;
            _modals = modals;
            _dialogs = dialogs;
            _vpnCredentialProvider = vpnCredentialProvider;
            _popupWindows = popupWindows;
            _delinquencyPopupViewModel = delinquencyPopupViewModel;
        }

        public bool IsServerFromProfile(Server server, Profile profile)
        {
            if (server == null || profile == null)
            {
                return false;
            }

            Specification<LogicalServerContract> serverSpec = ProfileServerSpec(profile);
            return _serverManager.IsServerFromSpec(server, serverSpec);
        }

        public ServerCandidates ServerCandidates(Profile profile)
        {
            if (profile == null)
            {
                return _serverCandidatesFactory.ServerCandidates(new Server[0]);
            }

            Specification<LogicalServerContract> serverSpec = ProfileServerSpec(profile);
            IReadOnlyCollection<Server> servers = _serverManager.GetServers(serverSpec);
            return _serverCandidatesFactory.ServerCandidates(servers);
        }

        public bool CanConnect(ServerCandidates candidates)
        {
            IReadOnlyList<Server> servers = Servers(candidates);

            if (servers.Any())
            {
                SwitchSecureCoreMode(servers.First().IsSecureCore());
                return true;
            }

            return false;
        }

        public async Task Connect(ServerCandidates candidates, Profile profile, VpnProtocol? overrideProtocol = null, int? maxServers = null)
        {
            IReadOnlyList<Server> servers = Servers(candidates);
            IEnumerable<Server> sortedServers = SortServers(servers, profile.ProfileType);
            if (maxServers.HasValue)
            {
                sortedServers = sortedServers.Take(maxServers.Value);
            }

            VpnProtocol protocol = overrideProtocol ?? profile.VpnProtocol;

            await Connect(sortedServers, protocol);
        }

        public async Task ConnectWithPreSortedCandidates(ServerCandidates sortedCandidates, VpnProtocol protocol)
        {
            IReadOnlyList<Server> sortedServers = Servers(sortedCandidates);

            await Connect(sortedServers, protocol);
        }

        public async Task<bool> UpdateServers(ServerCandidates candidates, Profile profile)
        {
            IReadOnlyList<Server> servers = Servers(candidates);
            if (!servers.Any())
            {
                return false;
            }

            IEnumerable<Server> sortedServers = SortServers(servers, profile.ProfileType);
            await UpdateServers(sortedServers);

            return true;
        }

        public IReadOnlyList<Server> Servers(ServerCandidates candidates)
        {
            IReadOnlyList<Server> servers = candidates.Items
                .OnlineServers()
                .UpToTierServers(_userStorage.User().MaxTier)
                .ToList();

            return servers;
        }

        public IEnumerable<Server> SortServers(IEnumerable<Server> source, ProfileType profileType)
        {
            if (profileType == ProfileType.Random)
            {
                return source.OrderBy(_ => _random.NextDouble());
            }

            if (_appSettings.FeaturePortForwardingEnabled && _appSettings.PortForwardingEnabled)
            {
                return source.OrderByDescending(s => s.SupportsP2P()).ThenBy(s => s.Score);
            }

            return source.OrderBy(s => s.Score);
        }

        private Specification<LogicalServerContract> ProfileServerSpec(Profile profile)
        {
            if (profile.ProfileType == ProfileType.Custom)
            {
                return new ServerById(profile.ServerId);
            }

            Specification<LogicalServerContract> spec = new ServerByFeatures(ServerFeatures(profile));

            if (!string.IsNullOrEmpty(profile.CountryCode))
            {
                spec &= new ExitCountryServer(profile.CountryCode);
            }

            if (!string.IsNullOrEmpty(profile.EntryCountryCode))
            {
                spec &= new EntryCountryServer(profile.EntryCountryCode);
            }

            if (!string.IsNullOrEmpty(profile.City))
            {
                spec &= new ExitCityServer(profile.City);
            }

            if (profile.ExactTier.HasValue)
            {
                spec &= new ExactTierServer(profile.ExactTier.Value);
            }

            return spec;
        }

        private Features ServerFeatures(Profile profile)
        {
            if (profile.IsPredefined)
            {
                if (_appSettings.SecureCore)
                {
                    return Features.SecureCore;
                }
                if (_appSettings.IsPortForwardingEnabled())
                {
                    return profile.Features;
                }

                return Features.None;
            }

            return profile.Features;
        }

        public void HandleNoServersAvailable(IReadOnlyCollection<Server> candidates, Profile profile)
        {
            if (profile.ProfileType == ProfileType.Custom)
            {
                HandleNoCustomServerAvailable(candidates.FirstOrDefault());
                return;
            }

            if (_userStorage.User().IsDelinquent())
            {
                HandleDelinquentUser();
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
                _dialogs.ShowWarning(Translation.Get("Profiles_msg_NoServersAvailable"));
                return;
            }

            IEnumerable<Server> userTierServers = candidates.UpToTierServers(_userStorage.User().MaxTier);

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
                _dialogs.ShowWarning(Translation.Get("Profiles_msg_AllServersOffline"));
                return;
            }

            _modals.Show<NoServerDueTierUpsellModalViewModel>();
        }

        private void HandleNoCountryServersAvailable(IReadOnlyCollection<Server> candidates)
        {
            if (!candidates.Any())
            {
                _dialogs.ShowWarning(Translation.Get("Profiles_msg_NoServersAvailable"));
                return;
            }

            IEnumerable<Server> userTierServers = candidates.UpToTierServers(_userStorage.User().MaxTier);

            if (!userTierServers.Any())
            {
                _modals.Show<NoServerInCountryDueTierUpsellModalViewModel>();
                return;
            }

            if (!candidates.OnlineServers().Any())
            {
                _dialogs.ShowWarning(Translation.Get("Profiles_msg_CountryOffline"));
                return;
            }

            _modals.Show<NoServerInCountryDueTierUpsellModalViewModel>();
        }

        private Common.Vpn.VpnConfig VpnConfig(VpnProtocol protocol)
        {
            Dictionary<VpnProtocol, IReadOnlyCollection<int>> portConfig = new Dictionary<VpnProtocol, IReadOnlyCollection<int>>
            {
                {VpnProtocol.OpenVpnUdp, _appSettings.OpenVpnUdpPorts},
                {VpnProtocol.OpenVpnTcp, _appSettings.OpenVpnTcpPorts},
            };

            List<string> customDns = (from ip in _appSettings.CustomDnsIps where ip.Enabled select ip.Ip).ToList();

            return new Common.Vpn.VpnConfig(
                new VpnConfigParameters
                {
                    Ports = portConfig,
                    CustomDns = _appSettings.CustomDnsEnabled ? customDns : new List<string>(),
                    SplitTunnelMode =
                        _appSettings.SplitTunnelingEnabled
                            ? _appSettings.SplitTunnelMode
                            : SplitTunnelMode.Disabled,
                    SplitTunnelIPs = GetSplitTunnelIPs(),
                    OpenVpnAdapter = _appSettings.NetworkAdapterType,
                    VpnProtocol = protocol,
                    NetShieldMode = _appSettings.IsNetShieldEnabled() ? _appSettings.NetShieldMode : 0,
                    SplitTcp = _appSettings.IsVpnAcceleratorEnabled(),
                });
        }

        private List<string> GetSplitTunnelIPs()
        {
            List<string> list = new List<string>();
            if (_appSettings.SplitTunnelMode != SplitTunnelMode.Disabled)
            {
                IpContract[] ips = _appSettings.SplitTunnelMode == SplitTunnelMode.Permit
                    ? _appSettings.SplitTunnelIncludeIps
                    : _appSettings.SplitTunnelExcludeIps;
                list.AddRange(from ip in ips where ip.Enabled select ip.Ip);
            }

            return list;
        }

        private async Task Connect(IEnumerable<Server> servers, VpnProtocol vpnProtocol)
        {
            Result<VpnCredentials> credentialsResult = await _vpnCredentialProvider.Credentials();
            if (credentialsResult.Failure)
            {
                _dialogs.ShowWarning(Translation.Get("ProfileConnector_msg_MissingAuthCert"));
                return;
            }

            IReadOnlyList<VpnHost> hosts = Servers(servers);
            if (hosts.Any())
            {
                VpnConnectionRequest request = new(hosts, vpnProtocol, VpnConfig(vpnProtocol), credentialsResult.Value);

                await _vpnServiceManager.Connect(request);

                _logger.Info("Connect requested");
                _modals.CloseAll();
            }
            else
            {
                _logger.Info("ProfileConnector - Connect received zero valid servers");
            }
        }

        private async Task UpdateServers(IEnumerable<Server> servers)
        {
            await _vpnServiceManager.UpdateServers(Servers(servers));
        }

        private void SwitchSecureCoreMode(bool secureCore)
        {
            if (secureCore)
            {
                if (_appSettings.PortForwardingEnabled)
                {
                    _appSettings.PortForwardingEnabled = false;
                }
                if (!_appSettings.SecureCore)
                {
                    _appSettings.SecureCore = true;
                }
            }
            else if (_appSettings.SecureCore)
            {
                _appSettings.SecureCore = false;
            }
        }

        private void HandleNoCustomServerAvailable(Server server)
        {
            if (server == null)
            {
                HandleEmptyServer();
                return;
            }

            if (_userStorage.User().IsDelinquent())
            {
                HandleDelinquentUser();
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
            _dialogs.ShowWarning(Translation.Get("Servers_msg_CantConnect_Missing"));
        }

        private void HandleDelinquentUser()
        {
            _delinquencyPopupViewModel.SetNoReconnectionData();
            _popupWindows.Show<DelinquencyPopupViewModel>();
        }

        private void HandleOfflineServer()
        {
            _dialogs.ShowWarning(Translation.Get("Servers_msg_CantConnect_Maintenance"));
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
                    _modals.Show<UpsellModalViewModel>();
                    break;
                case ServerTiers.Plus:
                    _modals.Show<PlusUpsellModalViewModel>();
                    break;
            }
        }

        private IReadOnlyList<VpnHost> Servers(IEnumerable<Server> servers)
        {
            return servers
                .SelectMany(s => s.Servers.OrderBy(_ => _random.Next()))
                .Where(s => s.Status != 0)
                .Select(s => new VpnHost(s.Domain, s.EntryIp, s.Label, s.X25519PublicKey))
                .Distinct(s => (s.Ip, s.Label))
                .ToList();
        }
    }
}