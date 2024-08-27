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
using ProtonVPN.Api;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Networking;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Servers.Contracts;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Crypto;
using ProtonVPN.GuestHoles.FileStoraging;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Vpn.Connectors;

namespace ProtonVPN.App.Tests.Vpn.Connectors
{
    [TestClass]
    public class GuestHoleConnectorTest
    {
        private const int MAX_RETRIES = 2;

        private GuestHoleConnector _connector;
        private readonly IVpnServiceManager _serviceManager = Substitute.For<IVpnServiceManager>();
        private readonly IAppSettings _appSettings = Substitute.For<IAppSettings>();
        private readonly ILogger _logger = Substitute.For<ILogger>();
        private readonly IAuthKeyManager _authKeyManager = Substitute.For<IAuthKeyManager>();
        private readonly IConfiguration _config = new Common.Configuration.Config()
        {
            MaxGuestHoleRetries = MAX_RETRIES,
        };
        private readonly IGuestHoleServersFileStorage _guestHoleServers = Substitute.For<IGuestHoleServersFileStorage>();

        [TestInitialize]
        public void Initialize()
        {
            GuestHoleState guestHoleState = new();
            guestHoleState.SetState(true);
            _authKeyManager.GenerateTemporaryKeyPair().Returns(GetKeyPair());
            _guestHoleServers.Get().Returns(new List<GuestHoleServerContract>());
            _connector = new GuestHoleConnector(_serviceManager, _appSettings, guestHoleState, 
                _config, _guestHoleServers, _authKeyManager, _logger);
        }

        private AsymmetricKeyPair GetKeyPair()
        {
            return new AsymmetricKeyPair(
                new SecretKey("PVPN", KeyAlgorithm.Unknown),
                new PublicKey("PVPN", KeyAlgorithm.Unknown));
        }

        [TestMethod]
        public async Task ItShouldConnectWithoutAuthCertificateAsync()
        {
            // Act
            await _connector.Connect();

            // Assert
            await _serviceManager.Received(1)
                .Connect(Arg.Is<VpnConnectionRequest>(c =>
                    c.Credentials.ClientCertPem.IsNullOrEmpty()));
        }

        [TestMethod]
        public async Task ItShouldDisconnectAfterCoupleOfRetries()
        {
            // Act
            for (int i = 0; i < MAX_RETRIES; i++)
            {
                await _connector.OnVpnStateChanged(GetEventArgs());
            }

            // Assert
            await _serviceManager.Received(1).Disconnect(VpnError.NoneKeepEnabledKillSwitch, 
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        }

        private VpnStateChangedEventArgs GetEventArgs() =>
            new(VpnStatus.Reconnecting,
                VpnError.None,
                string.Empty,
                true,
                VpnProtocol.Smart,
                OpenVpnAdapter.Tun);
    }
}