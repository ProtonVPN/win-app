using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Storage;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Config;
using ProtonVPN.Core.Api;
using ProtonVPN.Core.Servers.Contracts;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Vpn.Connectors;

namespace ProtonVPN.App.Test.Vpn.Connectors
{
    [TestClass]
    public class GuestHoleConnectorTest
    {
        private const int MaxRetries = 2;

        private GuestHoleConnector _connector;
        private readonly IVpnServiceManager _serviceManager = Substitute.For<IVpnServiceManager>();
        private readonly IVpnConfig _vpnConfig = Substitute.For<IVpnConfig>();
        private readonly Common.Configuration.Config _config = new Common.Configuration.Config
        {
            MaxGuestHoleRetries = MaxRetries,
            GuestHoleVpnUsername = "guest",
            GuestHoleVpnPassword = "guest",
        };

        private readonly ICollectionStorage<GuestHoleServerContract> _guestHoleServers =
            Substitute.For<ICollectionStorage<GuestHoleServerContract>>();

        [TestInitialize]
        public void Initialize()
        {
            var guestHoleState = new GuestHoleState();
            guestHoleState.SetState(true);

            _guestHoleServers.GetAll().Returns(new List<GuestHoleServerContract>());
            _connector = new GuestHoleConnector(_serviceManager, _vpnConfig, guestHoleState, _config, _guestHoleServers);
        }

        [TestMethod]
        public async Task ItShouldConnectWithGuestCredentials()
        {
            // Act
            await _connector.Connect();

            // Assert
            await _serviceManager.Received(1)
                .Connect(Arg.Is<VpnConnectionRequest>(c =>
                    c.Credentials.Username == _config.GuestHoleVpnUsername &&
                    c.Credentials.Password == _config.GuestHoleVpnPassword));
        }

        [TestMethod]
        public async Task ItShouldDisconnectAfterCoupleOfRetries()
        {
            // Act
            for (var i = 0; i < MaxRetries; i++) await _connector.OnVpnStateChanged(GetEventArgs());

            // Assert
            await _serviceManager.Received(1).Disconnect(VpnError.NoneKeepEnabledKillSwitch);
        }

        private VpnStateChangedEventArgs GetEventArgs() =>
            new VpnStateChangedEventArgs(
                VpnStatus.Reconnecting,
                VpnError.None,
                string.Empty,
                true,
                VpnProtocol.Auto);
    }
}