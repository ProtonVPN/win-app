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
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.Core;
using ProtonVPN.Api.Contracts.Geographical;
using ProtonVPN.Common.Abstract;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Networking;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Models;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Core.Windows.Popups;
using ProtonVPN.Crypto;
using ProtonVPN.Vpn;
using ProtonVPN.Vpn.Connectors;
using ProtonVPN.Windows.Popups.Delinquency;
using Profile = ProtonVPN.Core.Profiles.Profile;

namespace ProtonVPN.App.Tests.Vpn.Connectors
{
    [TestClass]
    public class ProfileConnectorTest
    {
        private const string AuthenticationCertificatePem = "AuthenticationCertificatePem";

        private readonly ILogger _logger = Substitute.For<ILogger>();
        private readonly IUserStorage _userStorage = Substitute.For<IUserStorage>();
        private readonly IAppSettings _appSettings = Substitute.For<IAppSettings>();
        private readonly IVpnServiceManager _vpnServiceManager = Substitute.For<IVpnServiceManager>();
        private readonly IModals _modals = Substitute.For<IModals>();
        private readonly IDialogs _dialogs = Substitute.For<IDialogs>();

        private ServerManager _serverManager;
        private IVpnCredentialProvider _vpnCredentialProvider;
        private IPopupWindows _popupWindows;
        private IDelinquencyPopupViewModel _delinquencyPopupViewModel;
        private ProfileConnector _profileConnector;

        private List<PhysicalServer> _standardPhysicalServers;
        private List<PhysicalServer> _p2pPhysicalServers;
        private List<PhysicalServer> _torPhysicalServers;
        private Server _standardServer;
        private Server _p2pServer;
        private Server _torServer;
        private List<Server> _servers;
        private Profile _profile;
        private VpnManagerProfileCandidates _vpnManagerProfileCandidates;

        private readonly PublicKey _publicKey = new("cHVibGljS2V5", KeyAlgorithm.Unknown);
        private readonly SecretKey _secretKey = new("c2VjcmV0S2V5", KeyAlgorithm.Unknown);

        [TestInitialize]
        public void Initialize()
        {
            InitializeDependencies();
            InitializeUser();
            InitializeArrangeVariables();
        }

        private void InitializeDependencies()
        {
            _serverManager = Substitute.For<ServerManager>(_userStorage, _appSettings, _logger);
            _vpnCredentialProvider = Substitute.For<IVpnCredentialProvider>();
            _popupWindows = Substitute.For<IPopupWindows>();
            _delinquencyPopupViewModel = Substitute.For<IDelinquencyPopupViewModel>();
            
            _vpnCredentialProvider.Credentials().Returns(GetVpnCredentials());

            _profileConnector = new ProfileConnector(_logger, _userStorage, _appSettings, _serverManager, _vpnServiceManager, _modals, _dialogs, _vpnCredentialProvider, _popupWindows, _delinquencyPopupViewModel);
        }

        private void InitializeUser()
        {
            User user = new()
            {
                MaxTier = ServerTiers.Plus
            };
            _userStorage.GetUser().Returns(user);
        }

        private Result<VpnCredentials> GetVpnCredentials()
        {
            return Result.Ok(new VpnCredentials(AuthenticationCertificatePem, new AsymmetricKeyPair(_secretKey, _publicKey)));
        }

        private void InitializeArrangeVariables()
        {
            _standardPhysicalServers = new List<PhysicalServer>
            {
                new(id: "Standard-PS", entryIp: "192.168.0.1", exitIp: "192.168.1.1",
                    domain: "standard.protonvpn.ps", status: 1, label: string.Empty, x25519PublicKey: string.Empty, signature: string.Empty)
            };
            _p2pPhysicalServers = new List<PhysicalServer>
            {
                new(id: "P2P-PS", entryIp: "192.168.0.2", exitIp: "192.168.1.2",
                    domain: "p2p.protonvpn.ps", status: 1, label: string.Empty, x25519PublicKey: string.Empty, signature: string.Empty)
            };
            _torPhysicalServers = new List<PhysicalServer>
            {
                new(id: "Tor-PS", entryIp: "192.168.0.3", exitIp: "192.168.1.3",
                    domain: "tor.protonvpn.ps", status: 1, label: string.Empty, x25519PublicKey: string.Empty, signature: string.Empty)
            };

            _standardServer = new Server(id: "Standard-S", name: "Standard", city: "City", entryCountry: "CH", exitCountry: "CH", domain: "standard.protonvpn.s", status: 1, tier: ServerTiers.Basic,
                features: (sbyte)Features.None, load: 0, score: 1, locationResponse: Substitute.For<LocationResponse>(), physicalServers: _standardPhysicalServers, exitIp: "192.168.2.1");
            _p2pServer = new Server(id: "P2P-S", name: "P2P", city: "City", entryCountry: "CH", exitCountry: "CH", domain: "p2p.protonvpn.s", status: 1, tier: ServerTiers.Plus,
                features: (sbyte)Features.P2P, load: 100, score: 999, locationResponse: Substitute.For<LocationResponse>(), physicalServers: _p2pPhysicalServers, exitIp: "192.168.2.2");
            _torServer = new Server(id: "Tor-S", name: "Tor", city: "City", entryCountry: "CH", exitCountry: "CH", domain: "tor.protonvpn.s", status: 1, tier: ServerTiers.Plus,
                features: (sbyte)Features.Tor, load: 0, score: 0, locationResponse: Substitute.For<LocationResponse>(), physicalServers: _torPhysicalServers, exitIp: "192.168.2.3");
            _servers = new List<Server>
            {
                _standardServer,
                _p2pServer,
                _torServer
            };
            _profile = new Profile(null)
            {
                ProfileType = ProfileType.Fastest,
                VpnProtocol = VpnProtocol.Smart
            };
            _vpnManagerProfileCandidates = new VpnManagerProfileCandidates
            {
                Profile = _profile,
                Candidates = _servers
            };
        }

        [TestMethod]
        public async Task Connect_PicksFastestServer()
        {
            // Arrange
            List<Server> expectedServers = new List<Server>
            {
                _torServer,
                _standardServer,
                _p2pServer
            };
            _vpnServiceManager.Connect(Arg.Any<VpnConnectionRequest>())
                .Returns((arg) => AssertConnectionAsync(arg, expectedServers));

            _appSettings.FeaturePortForwardingEnabled = true;
            _appSettings.PortForwardingEnabled = false;

            // Act
            await _profileConnector.Connect(_vpnManagerProfileCandidates, VpnProtocol.Smart);
        }

        private async Task AssertConnectionAsync(CallInfo callInfo, IList<Server> expectedServers)
        {
            VpnConnectionRequest vpnConnectionRequest = (VpnConnectionRequest)callInfo[0];
            Assert.AreEqual(expectedServers.Count, vpnConnectionRequest.Servers.Count);
            for (int i = 0; i < expectedServers.Count; i++)
            {
                Assert.AreEqual(expectedServers[i].Servers[0].EntryIp, vpnConnectionRequest.Servers[i].Ip);
            }
        }

        [TestMethod]
        public async Task Connect_PicksP2PServerWhenPortForwardingIsEnabled()
        {
            // Arrange
            List<Server> expectedServers = new List<Server>
            {
                _p2pServer,
                _torServer,
                _standardServer
            };
            _vpnServiceManager.Connect(Arg.Any<VpnConnectionRequest>())
                .Returns((arg) => AssertConnectionAsync(arg, expectedServers));

            _appSettings.FeaturePortForwardingEnabled = true;
            _appSettings.PortForwardingEnabled = true;

            // Act
            await _profileConnector.Connect(_vpnManagerProfileCandidates, VpnProtocol.Smart);
        }
    }
}
