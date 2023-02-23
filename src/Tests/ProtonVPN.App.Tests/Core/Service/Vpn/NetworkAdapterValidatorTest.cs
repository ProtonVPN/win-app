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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.NetworkLogs;
using ProtonVPN.Common.OS.Net;
using ProtonVPN.Common.OS.Net.NetworkInterface;
using ProtonVPN.Core.Service.Vpn;

namespace ProtonVPN.App.Tests.Core.Service.Vpn
{
    [TestClass]
    public class NetworkAdapterValidatorTest
    {
        private INetworkInterfaceLoader _networkInterfaceLoader;
        private ILogger _logger;

        [TestInitialize]
        public void Initialize()
        {
            _networkInterfaceLoader = Substitute.For<INetworkInterfaceLoader>();
            _logger = Substitute.For<ILogger>();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _networkInterfaceLoader = null;
        }

        [TestMethod]
        public void IsOpenVpnAdapterAvailable_ShouldReturnFalseWhenNoAdaptersAvailable()
        {
            _networkInterfaceLoader.GetOpenVpnTunInterface().ReturnsNull();
            _networkInterfaceLoader.GetOpenVpnTapInterface().ReturnsNull();

            NetworkAdapterValidator validator = new(_networkInterfaceLoader, _logger);
            
            validator.IsOpenVpnAdapterAvailable().Should().BeFalse();
            _logger.Received(0).Info<NetworkLog>(Arg.Any<string>(), null, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
            _logger.Received(1).Warn<NetworkLog>(Arg.Any<string>(), null, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        }

        [TestMethod]
        public void IsOpenVpnAdapterAvailable_ShouldReturnTrueWhenTapIsAvailable()
        {
            _networkInterfaceLoader.GetOpenVpnTunInterface().ReturnsNull();
            _networkInterfaceLoader.GetOpenVpnTapInterface().Returns(new NullNetworkInterface());

            NetworkAdapterValidator validator = new(_networkInterfaceLoader, _logger);
            
            validator.IsOpenVpnAdapterAvailable().Should().BeTrue();
            _logger.Received(1).Info<NetworkLog>(Arg.Any<string>(), null, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
            _logger.Received(0).Warn<NetworkLog>(Arg.Any<string>(), null, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        }

        [TestMethod]
        public void IsOpenVpnAdapterAvailable_ShouldReturnTrueWhenTunIsAvailable()
        {
            _networkInterfaceLoader.GetOpenVpnTunInterface().Returns(new NullNetworkInterface());
            _networkInterfaceLoader.GetOpenVpnTapInterface().ReturnsNull();

            NetworkAdapterValidator validator = new(_networkInterfaceLoader, _logger);
            
            validator.IsOpenVpnAdapterAvailable().Should().BeTrue();
            _logger.Received(1).Info<NetworkLog>(Arg.Any<string>(), null, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
            _logger.Received(0).Warn<NetworkLog>(Arg.Any<string>(), null, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        }

        [TestMethod]
        public void IsOpenVpnAdapterAvailable_ShouldReturnTrueWhenBothAreAvailable()
        {
            _networkInterfaceLoader.GetOpenVpnTunInterface().Returns(new NullNetworkInterface());
            _networkInterfaceLoader.GetOpenVpnTapInterface().Returns(new NullNetworkInterface());

            NetworkAdapterValidator validator = new(_networkInterfaceLoader, _logger);
            
            validator.IsOpenVpnAdapterAvailable().Should().BeTrue();
            _logger.Received(1).Info<NetworkLog>(Arg.Any<string>(), null, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
            _logger.Received(0).Warn<NetworkLog>(Arg.Any<string>(), null, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        }
    }
}