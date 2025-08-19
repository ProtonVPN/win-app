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
using System.Net;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.OperatingSystems.Network.Contracts;
using ProtonVPN.Service.Vpn;

namespace ProtonVPN.Service.Tests.Vpn
{
    [TestClass]
    public class ObservableNetworkInterfacesTest
    {
        private ISystemNetworkInterfaces _interfaces;

        [TestInitialize]
        public void TestInitialize()
        {
            _interfaces = Substitute.For<ISystemNetworkInterfaces>();
        }

        [TestMethod]
        public void NetworkInterfacesAdded_ShouldNotBeRaised_FirstTimeAfterInitialization()
        {
            // Arrange
            var wasRaised = false;
            _interfaces.GetInterfaces().Returns(new INetworkInterface[] {new TestNetworkInterface("n1")});
            var subject = new ObservableNetworkInterfaces(_interfaces);
            subject.NetworkInterfacesAdded += (s, e) => wasRaised = true;

            // Act
            _interfaces.NetworkAddressChanged += Raise.Event();

            // Assert
            wasRaised.Should().BeFalse();
        }

        [TestMethod]
        public void NetworkInterfacesAdded_ShouldBeRaised_WhenNewInterfaceAdded()
        {
            // Arrange
            var wasRaised = false;
            _interfaces.GetInterfaces().Returns(new INetworkInterface[] {new TestNetworkInterface("n1")});
            var subject = new ObservableNetworkInterfaces(_interfaces);
            subject.NetworkInterfacesAdded += (s, e) => wasRaised = true;
            _interfaces.NetworkAddressChanged += Raise.Event();

            _interfaces.GetInterfaces()
                .Returns(new INetworkInterface[] {new TestNetworkInterface("n1"), new TestNetworkInterface("n2")});
            wasRaised = false;

            // Act
            _interfaces.NetworkAddressChanged += Raise.Event();

            // Assert
            wasRaised.Should().BeTrue();
        }

        [TestMethod]
        public void NetworkInterfacesAdded_ShouldBeRaised_WhenNewInterfaceAdded_AndOldRemoved()
        {
            // Arrange
            var wasRaised = false;
            _interfaces.GetInterfaces().Returns(new INetworkInterface[] {new TestNetworkInterface("n1")});
            var subject = new ObservableNetworkInterfaces(_interfaces);
            subject.NetworkInterfacesAdded += (s, e) => wasRaised = true;
            _interfaces.NetworkAddressChanged += Raise.Event();

            _interfaces.GetInterfaces().Returns(new INetworkInterface[] {new TestNetworkInterface("n2")});
            wasRaised = false;

            // Act
            _interfaces.NetworkAddressChanged += Raise.Event();

            // Assert
            wasRaised.Should().BeTrue();
        }

        [TestMethod]
        public void NetworkInterfacesAdded_ShouldBeRaised_WhenSameNetworkInterfaces()
        {
            // Arrange
            var wasRaised = false;
            _interfaces.GetInterfaces()
                .Returns(new INetworkInterface[] {new TestNetworkInterface("n1"), new TestNetworkInterface("n2")});
            var subject = new ObservableNetworkInterfaces(_interfaces);
            subject.NetworkInterfacesAdded += (s, e) => wasRaised = true;
            _interfaces.NetworkAddressChanged += Raise.Event();

            _interfaces.GetInterfaces()
                .Returns(new INetworkInterface[] {new TestNetworkInterface("n1"), new TestNetworkInterface("n2")});
            wasRaised = false;

            // Act
            _interfaces.NetworkAddressChanged += Raise.Event();

            // Assert
            wasRaised.Should().BeFalse();
        }

        [TestMethod]
        public void NetworkInterfacesAdded_ShouldBeRaised_WhenRemovedNetworkInterfaces()
        {
            // Arrange
            var wasRaised = false;
            _interfaces.GetInterfaces()
                .Returns(new INetworkInterface[] {new TestNetworkInterface("n1"), new TestNetworkInterface("n2")});
            var subject = new ObservableNetworkInterfaces(_interfaces);
            subject.NetworkInterfacesAdded += (s, e) => wasRaised = true;
            _interfaces.NetworkAddressChanged += Raise.Event();

            _interfaces.GetInterfaces().Returns(new INetworkInterface[] {new TestNetworkInterface("n2")});
            wasRaised = false;

            // Act
            _interfaces.NetworkAddressChanged += Raise.Event();

            // Assert
            wasRaised.Should().BeFalse();
        }

        [TestMethod]
        public void NetworkInterfacesAdded_ShouldBeRaised_WhenNoNetworkInterfaces()
        {
            // Arrange
            var wasRaised = false;
            _interfaces.GetInterfaces().Returns(new INetworkInterface[] {new TestNetworkInterface("n1")});
            var subject = new ObservableNetworkInterfaces(_interfaces);
            subject.NetworkInterfacesAdded += (s, e) => wasRaised = true;
            _interfaces.NetworkAddressChanged += Raise.Event();

            _interfaces.GetInterfaces().Returns(new INetworkInterface[] { });
            wasRaised = false;

            // Act
            _interfaces.NetworkAddressChanged += Raise.Event();

            // Assert
            wasRaised.Should().BeFalse();
        }

        [TestMethod]
        public void NetworkInterfacesAdded_ShouldBeRaised_WhenNoNewInterfaces_AfterNoNetworkInterfaces()
        {
            // Arrange
            var wasRaised = false;
            _interfaces.GetInterfaces().Returns(new INetworkInterface[] {new TestNetworkInterface("n1")});
            var subject = new ObservableNetworkInterfaces(_interfaces);
            subject.NetworkInterfacesAdded += (s, e) => wasRaised = true;
            _interfaces.NetworkAddressChanged += Raise.Event();

            _interfaces.GetInterfaces().Returns(new INetworkInterface[] { });
            _interfaces.NetworkAddressChanged += Raise.Event();

            _interfaces.GetInterfaces().Returns(new INetworkInterface[] {new TestNetworkInterface("n1")});
            wasRaised = false;

            // Act
            _interfaces.NetworkAddressChanged += Raise.Event();

            // Assert
            wasRaised.Should().BeFalse();
        }

        #region Helpers

        private class TestNetworkInterface : INetworkInterface
        {
            public TestNetworkInterface(string id)
            {
                Id = id;
            }

            public string Id { get; }

            public string Name => string.Empty;

            public string Description => string.Empty;

            public bool IsLoopback => false;

            public bool IsActive => false;

            public IPAddress DefaultGateway => IPAddress.None;

            public uint Index => 0;

            public List<NetworkAddress> GetUnicastAddresses()
            {
                return [];
            }
        }

        #endregion
    }
}