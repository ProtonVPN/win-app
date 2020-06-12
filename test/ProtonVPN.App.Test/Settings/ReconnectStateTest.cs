using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Settings;

namespace ProtonVPN.App.Test.Settings
{
    [TestClass]
    public class ReconnectStateTest
    {
        private IAppSettings _appSettings;

        [TestInitialize]
        public void TestInitialize()
        {
            _appSettings = Substitute.For<IAppSettings>();
        }

        [TestMethod]
        public async Task ReconnectShouldBeRequiredOnlyIfChangesPending()
        {
            // Arrange
            _appSettings.KillSwitch.Returns(false);
            var sut = new ReconnectState(_appSettings);
            await sut.OnVpnStateChanged(GetVpnStateEventArgs(VpnStatus.Connected));
            _appSettings.KillSwitch.Returns(true);

            // Assert
            sut.Required().Should().BeTrue();
        }

        [TestMethod]
        public async Task ReconnectShouldNotBeRequiredIfDisconnected()
        {
            // Arrange
            _appSettings.KillSwitch.Returns(false);
            var sut = new ReconnectState(_appSettings);
            await sut.OnVpnStateChanged(GetVpnStateEventArgs(VpnStatus.Connected));
            _appSettings.KillSwitch.Returns(true);

            // Act
            await sut.OnVpnStateChanged(GetVpnStateEventArgs(VpnStatus.Disconnected));

            // Assert
            sut.Required().Should().BeFalse();
        }

        private VpnStateChangedEventArgs GetVpnStateEventArgs(VpnStatus status)
        {
            return new VpnStateChangedEventArgs(status, VpnError.None, string.Empty, false, default);
        }
    }
}
