using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Settings.ReconnectNotification;

namespace ProtonVPN.App.Test.Settings
{
    [TestClass]
    public class ReconnectStateTest
    {
        private IAppSettings _appSettings;
        private SettingsBuilder _settingsBuilder;

        [TestInitialize]
        public void TestInitialize()
        {
            _appSettings = Substitute.For<IAppSettings>();
            _settingsBuilder = new SettingsBuilder(_appSettings);
        }

        [TestMethod]
        public async Task ReconnectShouldBeRequiredOnlyIfChangesPending()
        {
            // Arrange
            _appSettings.OvpnProtocol.Returns("tcp");
            var sut = new ReconnectState(_settingsBuilder);
            await sut.OnVpnStateChanged(GetVpnStateEventArgs(VpnStatus.Connected));
            _appSettings.OvpnProtocol.Returns("udp");

            // Assert
            sut.Required(nameof(IAppSettings.OvpnProtocol)).Should().BeTrue();
        }

        [TestMethod]
        public async Task ReconnectShouldNotBeRequiredIfDisconnected()
        {
            // Arrange
            _appSettings.OvpnProtocol.Returns("tcp");
            var sut = new ReconnectState(_settingsBuilder);
            await sut.OnVpnStateChanged(GetVpnStateEventArgs(VpnStatus.Connected));
            _appSettings.OvpnProtocol.Returns("udp");

            // Act
            await sut.OnVpnStateChanged(GetVpnStateEventArgs(VpnStatus.Disconnected));

            // Assert
            sut.Required(nameof(IAppSettings.OvpnProtocol)).Should().BeFalse();
        }

        private VpnStateChangedEventArgs GetVpnStateEventArgs(VpnStatus status)
        {
            return new VpnStateChangedEventArgs(status, VpnError.None, string.Empty, false, default);
        }
    }
}
