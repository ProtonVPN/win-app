/*
 * Copyright (c) 2021 Proton Technologies AG
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
using NSubstitute.ReturnsExtensions;
using ProtonVPN.Common.Networking;
using ProtonVPN.Common.OS.Net;
using ProtonVPN.Common.OS.Net.NetworkInterface;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.App.Test.Core.Service.Vpn
{
    [TestClass]
    public class NetworkAdapterValidatorTest
    {
        private IAppSettings _appSettings;
        private INetworkInterfaceLoader _networkInterfaceLoader;

        [TestInitialize]
        public void Initialize()
        {
            _appSettings = Substitute.For<IAppSettings>();
            _networkInterfaceLoader = Substitute.For<INetworkInterfaceLoader>();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _appSettings = null;
            _networkInterfaceLoader = null;
        }

        [TestMethod]
        public void IsAtLeastOneAdapterAvailable_ShouldReturnFalseWhenNoAdaptersAvailable()
        {
            // Arrange
            _appSettings.NetworkAdapterType.Returns(OpenVpnAdapter.Tun);
            _networkInterfaceLoader.GetOpenVpnTunInterface().ReturnsNull();
            _networkInterfaceLoader.GetOpenVpnTapInterface().ReturnsNull();

            NetworkAdapterValidator sut = new(_networkInterfaceLoader, _appSettings);

            // Assert
            sut.IsAdapterAvailable().Should().BeFalse();
        }

        [TestMethod]
        public void IsAtLeastOneAdapterAvailable_ShouldFallbackToTap()
        {
            // Arrange
            _appSettings.NetworkAdapterType.Returns(OpenVpnAdapter.Tun);
            _networkInterfaceLoader.GetOpenVpnTunInterface().ReturnsNull();
            _networkInterfaceLoader.GetOpenVpnTapInterface().Returns(new NullNetworkInterface());

            NetworkAdapterValidator sut = new(_networkInterfaceLoader, _appSettings);

            // Assert
            sut.IsAdapterAvailable().Should().BeTrue();
            _appSettings.NetworkAdapterType.Should().Be(OpenVpnAdapter.Tap);
        }

        [TestMethod]
        public void IsAtLeastOneAdapterAvailable_ShouldReturnTrueWhenTunAvailable()
        {
            // Arrange
            _appSettings.NetworkAdapterType.Returns(OpenVpnAdapter.Tun);
            _networkInterfaceLoader.GetOpenVpnTunInterface().Returns(new NullNetworkInterface());
            _networkInterfaceLoader.GetOpenVpnTapInterface().Returns(new NullNetworkInterface());

            NetworkAdapterValidator sut = new(_networkInterfaceLoader, _appSettings);

            // Assert
            sut.IsAdapterAvailable().Should().BeTrue();
            _appSettings.NetworkAdapterType.Should().Be(OpenVpnAdapter.Tun);
        }
    }
}