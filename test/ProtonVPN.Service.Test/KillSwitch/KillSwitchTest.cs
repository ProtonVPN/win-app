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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Service.Contract.Settings;
using ProtonVPN.Service.Firewall;
using ProtonVPN.Service.Settings;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Service.Test.KillSwitch
{
    [TestClass]
    public class KillSwitchTest
    {
        private IFirewall _firewall;
        private IServiceSettings _serviceSettings;
        private const string RemoteIp = "2.2.2.2";

        [TestInitialize]
        public void Setup()
        {
            _firewall = Substitute.For<IFirewall>();
            _serviceSettings = Substitute.For<IServiceSettings>();
        }

        [TestMethod]
        [DataRow(SplitTunnelMode.Block, false)]
        [DataRow(SplitTunnelMode.Permit, true)]
        [DataRow(SplitTunnelMode.Disabled, false)]
        public void OnVpnConnecting_SplitTunnelBlockMode_BlockInternet(SplitTunnelMode mode, bool dnsLeakOnly)
        {
            // Arrange
            var killSwitch = GetKillSwitch(mode);

            // Act
            killSwitch.OnVpnConnecting(GetConnectingVpnState());

            // Assert
            _firewall.ReceivedWithAnyArgs(1).EnableLeakProtection(Arg.Is<FirewallParams>(p => p.DnsLeakOnly == dnsLeakOnly));
        }

        [TestMethod]
        public void OnVpnConnected_WhenSplitTunnelPermitMode_DoNotBlockInternet()
        {
            // Arrange
            var killSwitch = GetKillSwitch(SplitTunnelMode.Permit);

            // Act
            killSwitch.OnVpnConnected(GetConnectedVpnState());

            // Assert
            _firewall.Received(0).EnableLeakProtection(new FirewallParams("127.0.0.1", false));
        }

        [TestMethod]
        public void OnVpnDisconnected_ManualDisconnect_RestoreInternet()
        {
            // Arrange
            var sut = new Service.KillSwitch.KillSwitch(_firewall, _serviceSettings);

            // Act
            sut.OnVpnDisconnected(GetDisconnectedVpnState(true));

            // Assert
            _firewall.Received(1).DisableLeakProtection();
        }

        [TestMethod]
        public void OnVpnDisconnected_UnexpectedDisconnectWithKillSwitchOff_RestoreInternet()
        {
            // Arrange
            _serviceSettings.KillSwitchSettings.Returns(new KillSwitchSettingsContract
            {
                Enabled = false
            });
            var sut = new Service.KillSwitch.KillSwitch(_firewall, _serviceSettings);

            // Act
            sut.OnVpnDisconnected(GetDisconnectedVpnState());

            // Assert
            _firewall.Received(1).DisableLeakProtection();
        }

        [DataTestMethod]
        [DataRow(VpnStatus.Connecting)]
        [DataRow(VpnStatus.Reconnecting)]
        public void ExpectedLeakProtectionStatus_ShouldBe_Enabled_WhenConnecting(VpnStatus status)
        {
            // Arrange
            var state = new VpnState(status);
            _serviceSettings.SplitTunnelSettings.Returns(new SplitTunnelSettingsContract
            {
                Mode = SplitTunnelMode.Block
            });
            var killSwitch = new Service.KillSwitch.KillSwitch(_firewall, _serviceSettings);

            // Act
            var result = killSwitch.ExpectedLeakProtectionStatus(state);

            //Assert
            result.Should().Be(true);
        }

        [DataTestMethod]
        [DataRow(VpnStatus.Disconnecting, VpnError.None, false, false, false)]
        [DataRow(VpnStatus.Disconnecting, VpnError.None, true, false, false)]
        [DataRow(VpnStatus.Disconnecting, VpnError.None, false, true, false)]
        [DataRow(VpnStatus.Disconnecting, VpnError.None, true, true, false)]
        [DataRow(VpnStatus.Disconnecting, VpnError.NetshError, false, false, false)]
        [DataRow(VpnStatus.Disconnecting, VpnError.NetshError, false, true, false)]
        [DataRow(VpnStatus.Disconnecting, VpnError.NetshError, true, false, false)]
        [DataRow(VpnStatus.Disconnecting, VpnError.NetshError, true, true, true)]
        [DataRow(VpnStatus.Disconnected, VpnError.None, false, false, false)]
        [DataRow(VpnStatus.Disconnected, VpnError.None, true, false, false)]
        [DataRow(VpnStatus.Disconnected, VpnError.None, false, true, false)]
        [DataRow(VpnStatus.Disconnected, VpnError.None, true, true, false)]
        [DataRow(VpnStatus.Disconnected, VpnError.NetshError, false, false, false)]
        [DataRow(VpnStatus.Disconnected, VpnError.NetshError, false, true, false)]
        [DataRow(VpnStatus.Disconnected, VpnError.NetshError, true, false, false)]
        [DataRow(VpnStatus.Disconnected, VpnError.NetshError, true, true, true)]
        public void ExpectedLeakProtectionStatus_ShouldBe_Expected_WhenDisconnecting(VpnStatus status, VpnError error, bool killSwitchEnabled, bool leakProtectionEnabled, bool expected)
        {
            // Arrange
            var state = new VpnState(status, error);
            _serviceSettings.KillSwitchSettings.Returns(new KillSwitchSettingsContract
            {
                Enabled = killSwitchEnabled
            });
            _firewall.LeakProtectionEnabled.Returns(leakProtectionEnabled);
            var killSwitch = new Service.KillSwitch.KillSwitch(_firewall, _serviceSettings);

            // Act
            var result = killSwitch.ExpectedLeakProtectionStatus(state);

            //Assert
            result.Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow(VpnStatus.Waiting, false)]
        [DataRow(VpnStatus.Waiting, true)]
        [DataRow(VpnStatus.Authenticating, false)]
        [DataRow(VpnStatus.Authenticating, true)]
        [DataRow(VpnStatus.RetrievingConfiguration, false)]
        [DataRow(VpnStatus.RetrievingConfiguration, true)]
        [DataRow(VpnStatus.AssigningIp, false)]
        [DataRow(VpnStatus.AssigningIp, true)]
        [DataRow(VpnStatus.Connected, false)]
        [DataRow(VpnStatus.Connected, true)]
        public void ExpectedLeakProtectionStatus_ShouldBe_Firewall_LeakProtectionEnabled_WhenOtherStatus(VpnStatus status, bool leakProtectionEnabled)
        {
            // Arrange
            var state = new VpnState(status);
            _firewall.LeakProtectionEnabled.Returns(leakProtectionEnabled);
            var killSwitch = new Service.KillSwitch.KillSwitch(_firewall, _serviceSettings);

            // Act
            var result = killSwitch.ExpectedLeakProtectionStatus(state);

            //Assert
            result.Should().Be(leakProtectionEnabled);
        }

        private Service.KillSwitch.KillSwitch GetKillSwitch(SplitTunnelMode mode)
        {
            _serviceSettings.SplitTunnelSettings.Returns(new SplitTunnelSettingsContract
            {
                Mode = mode,
                AppPaths = new string[0],
                Ips = new string[0]
            });

            return new Service.KillSwitch.KillSwitch(_firewall, _serviceSettings);
        }

        private VpnState GetDisconnectedVpnState(bool manualDisconnect = false)
        {
            return new VpnState(
                VpnStatus.Disconnected,
                manualDisconnect ? VpnError.None : VpnError.Unknown,
                "1.1.1.1",
                RemoteIp);
        }

        private VpnState GetConnectedVpnState()
        {
            return new VpnState(
                VpnStatus.Connected,
                VpnError.None,
                "1.1.1.1",
                RemoteIp);
        }

        private VpnState GetConnectingVpnState()
        {
            return new VpnState(
                VpnStatus.Connecting,
                VpnError.None,
                "1.1.1.1",
                RemoteIp);
        }
    }
}
