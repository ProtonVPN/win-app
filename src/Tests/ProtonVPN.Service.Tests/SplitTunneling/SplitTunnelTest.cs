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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common;
using ProtonVPN.Common.Networking;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Service.Contract.Settings;
using ProtonVPN.Service.Firewall;
using ProtonVPN.Service.Settings;
using ProtonVPN.Service.SplitTunneling;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Service.Tests.SplitTunneling
{
    [TestClass]
    public class SplitTunnelTest
    {
        private IServiceSettings _serviceSettings;
        private ISplitTunnelClient _splitTunnelClient;
        private IncludeModeApps _reverseSplitTunnelApps;
        private IFilterCollection _appFilter;
        private IFilterCollection _permittedRemoteAddress;

        [TestInitialize]
        public void TestInitialize()
        {
            _serviceSettings = Substitute.For<IServiceSettings>();
            _splitTunnelClient = Substitute.For<ISplitTunnelClient>();
            _appFilter = Substitute.For<IFilterCollection>();
            _reverseSplitTunnelApps = new IncludeModeApps(_serviceSettings);
            _permittedRemoteAddress = Substitute.For<IFilterCollection>();
        }

        [TestMethod]
        public void OnVpnConnecting_WhenBlockMode_DisableReversed()
        {
            // Arrange
            _serviceSettings.SplitTunnelSettings.Returns(new SplitTunnelSettingsContract
            {
                Mode = SplitTunnelMode.Block
            });
            SplitTunnel splitTunnel = GetSplitTunnel(false, true);

            // Act
            splitTunnel.OnVpnConnecting(GetConnectingVpnState());

            // Assert
            _splitTunnelClient.Received(1).Disable();
        }

        [TestMethod]
        public void OnVpnConnecting_WhenBlockMode_Disable()
        {
            // Arrange
            _serviceSettings.SplitTunnelSettings.Returns(new SplitTunnelSettingsContract
            {
                Mode = SplitTunnelMode.Permit
            });
            SplitTunnel splitTunnel = GetSplitTunnel(true);

            // Act
            splitTunnel.OnVpnConnecting(GetConnectingVpnState());

            // Assert
            _splitTunnelClient.Received(1).Disable();
        }

        [TestMethod]
        public void OnVpnConnected_PermitRemoteAddressesOnBlockMode()
        {
            // Arrange
            string[] addresses = new[] { "127.0.0.1", "192.168.0.1", "8.8.8.8" };
            _serviceSettings.SplitTunnelSettings.Returns(new SplitTunnelSettingsContract
            {
                Mode = SplitTunnelMode.Block,
                Ips = addresses,
                AppPaths = new string[] {},
            });
            SplitTunnel splitTunnel = GetSplitTunnel();

            // Act
            splitTunnel.OnVpnConnected(GetConnectedVpnState());

            // Assert
            _permittedRemoteAddress.Received(1).Add(addresses, NetworkFilter.Action.SoftPermit);
        }

        [TestMethod]
        public void OnVpnConnected_WhenBlockMode_CallEnable()
        {
            // Arrange
            _serviceSettings.SplitTunnelSettings.Returns(new SplitTunnelSettingsContract
            {
                Mode = SplitTunnelMode.Block,
                AppPaths = new string[] {},
                Ips = new string[] {},
            });
            SplitTunnel splitTunnel = GetSplitTunnel();

            // Act
            splitTunnel.OnVpnConnected(GetConnectedVpnState());

            // Assert
            _splitTunnelClient.Received(1).EnableExcludeMode(Arg.Any<string[]>(), Arg.Any<string[]>());
        }

        [TestMethod]
        public void OnVpnConnected_WhenBlockMode_CalloutDriverStart()
        {
            // Arrange
            _serviceSettings.SplitTunnelSettings.Returns(new SplitTunnelSettingsContract
            {
                Mode = SplitTunnelMode.Block,
                AppPaths = new string[] {},
                Ips = new string[] { },
            });
            SplitTunnel splitTunnel = GetSplitTunnel();

            // Act
            splitTunnel.OnVpnConnected(GetConnectedVpnState());
        }

        [TestMethod]
        public void OnVpnConnected_WhenPermitMode_CalloutDriverStart()
        {
            // Arrange
            _serviceSettings.SplitTunnelSettings.Returns(new SplitTunnelSettingsContract
            {
                Mode = SplitTunnelMode.Permit
            });
            SplitTunnel splitTunnel = GetSplitTunnel();

            // Act
            splitTunnel.OnVpnConnected(GetConnectedVpnState());
        }

        [TestMethod]
        public void OnVpnConnected_WhenDisabled_CalloutDriverDoNotStart()
        {
            // Arrange
            _serviceSettings.SplitTunnelSettings.Returns(new SplitTunnelSettingsContract
            {
                Mode = SplitTunnelMode.Disabled
            });
            SplitTunnel splitTunnel = GetSplitTunnel();

            // Act
            splitTunnel.OnVpnConnected(GetConnectedVpnState());
        }

        [TestMethod]
        public void OnVpnConnected_WhenDisabled_DoNotEnable()
        {
            // Arrange
            _serviceSettings.SplitTunnelSettings.Returns(new SplitTunnelSettingsContract
            {
                Mode = SplitTunnelMode.Disabled
            });
            SplitTunnel splitTunnel = GetSplitTunnel();

            // Act
            splitTunnel.OnVpnConnected(GetConnectedVpnState());

            // Assert
            _splitTunnelClient.Received(0);
        }

        [TestMethod]
        public void OnVpnConnected_WhenPermitMode_EnableReversed()
        {
            // Arrange
            _serviceSettings.SplitTunnelSettings.Returns(new SplitTunnelSettingsContract
            {
                Mode = SplitTunnelMode.Permit
            });
            SplitTunnel splitTunnel = GetSplitTunnel();

            // Act
            splitTunnel.OnVpnConnected(GetConnectedVpnState());

            // Assert
            _splitTunnelClient
                .Received(1)
                .EnableIncludeMode(Arg.Any<string[]>(), Arg.Any<string[]>(), Arg.Any<string>());
        }

        [TestMethod]
        public void OnVpnConnected_PermitAppsOnBlockMode()
        {
            // Arrange
            string[] apps = new[] { "app1", "app2", "app3" };
            _serviceSettings.SplitTunnelSettings.Returns(new SplitTunnelSettingsContract
            {
                Mode = SplitTunnelMode.Block,
                AppPaths = apps,
                Ips = new string[] {},
            });
            SplitTunnel splitTunnel = GetSplitTunnel();

            // Act
            splitTunnel.OnVpnConnected(GetConnectedVpnState());

            // Assert
            _appFilter.Received(1).Add(apps, NetworkFilter.Action.SoftPermit);
        }

        [TestMethod]
        public void OnVpnConnecting_ShouldBlockApps_WhenModeIsPermit()
        {
            // Arrange
            string[] apps = new [] {"app1", "app2", "app3"};
            _serviceSettings.SplitTunnelSettings.Returns(new SplitTunnelSettingsContract
            {
                Mode = SplitTunnelMode.Permit,
                AppPaths = apps
            });
            SplitTunnel splitTunnel = GetSplitTunnel(true);

            // Act
            splitTunnel.OnVpnConnecting(GetConnectingVpnState());

            // Assert
            _appFilter.Received(1).Add(apps, NetworkFilter.Action.SoftBlock);
        }

        [TestMethod]
        public void OnVpnDisconnected_ManualDisconnect_ShouldDisable()
        {
            // Arrange
            _serviceSettings.SplitTunnelSettings.Returns(new SplitTunnelSettingsContract
            {
                Mode = SplitTunnelMode.Block
            });
            SplitTunnel splitTunnel = GetSplitTunnel(true);

            // Act
            splitTunnel.OnVpnDisconnected(GetDisconnectedVpnState(true));

            // Assert
            _splitTunnelClient.Received(1).Disable();
        }

        [TestMethod]
        public void OnVpnDisconnected_ManualDisconnect_ShouldDisableReversed()
        {
            // Arrange
            _serviceSettings.SplitTunnelSettings.Returns(new SplitTunnelSettingsContract
            {
                Mode = SplitTunnelMode.Permit
            });
            SplitTunnel splitTunnel = GetSplitTunnel(false, true);

            // Act
            splitTunnel.OnVpnDisconnected(GetDisconnectedVpnState(true));

            // Assert
            _splitTunnelClient.Received(1).Disable();
        }

        [TestMethod]
        public void OnVpnDisconnected_ManualDisconnect_ShouldStopCalloutDriver()
        {
            // Arrange
            _serviceSettings.SplitTunnelSettings.Returns(new SplitTunnelSettingsContract
            {
                Mode = SplitTunnelMode.Permit
            });
            SplitTunnel splitTunnel = GetSplitTunnel();

            // Act
            splitTunnel.OnVpnDisconnected(GetDisconnectedVpnState(true));
        }

        private SplitTunnel GetSplitTunnel(bool enabled = false, bool reverseEnabled = false)
        {
            return new SplitTunnel(
                enabled,
                reverseEnabled,
                _serviceSettings,
                _splitTunnelClient,
                _reverseSplitTunnelApps,
                _appFilter,
                _permittedRemoteAddress);
        }

        private VpnState GetConnectedVpnState()
        {
            return new VpnState(
                VpnStatus.Connected,
                VpnError.None,
                "1.1.1.1",
                "2.2.2.2",
                VpnProtocol.Smart);
        }

        private VpnState GetDisconnectedVpnState(bool manualDisconnect = false)
        {
            return new VpnState(
                VpnStatus.Disconnected,
                manualDisconnect ? VpnError.None : VpnError.Unknown,
                "1.1.1.1",
                "2.2.2.2",
                VpnProtocol.Smart);
        }

        private VpnState GetConnectingVpnState()
        {
            return new VpnState(
                VpnStatus.Disconnected,
                VpnError.None,
                "1.1.1.1",
                "2.2.2.2",
                VpnProtocol.Smart);
        }
    }
}
