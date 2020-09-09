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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.NetworkInformation;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.OS.Net.NetworkInterface;

namespace ProtonVPN.Common.Test.OS.Net.NetworkInterface
{
    [TestClass]
    [SuppressMessage("ReSharper", "AssignmentIsFullyDiscarded")]
    public class SafeSystemNetworkInterfacesTest
    {
        private ILogger _logger;
        private INetworkInterfaces _origin;

        [TestInitialize]
        public void TestInitialize()
        {
            _logger = Substitute.For<ILogger>();
            _origin = Substitute.For<INetworkInterfaces>();
        }

        [TestMethod]
        public void NetworkAddressChanged_ShouldRaise_WhenOrigin_NetworkAddressChanged_IsRaised()
        {
            // Arrange
            var wasRaised = false;
            var subject = new SafeSystemNetworkInterfaces(_logger, _origin);
            subject.NetworkAddressChanged += (s, e) => wasRaised = true;
            // Act
            _origin.NetworkAddressChanged += Raise.Event<EventHandler>();
            // Assert
            wasRaised.Should().BeTrue();
        }

        [TestMethod]
        public void Interfaces_ShouldBe_Origin_Interfaces()
        {
            // Arrange
            var expected = new INetworkInterface[] { new TestNetworkInterface("t1"), new TestNetworkInterface("t2") };
            _origin.Interfaces().Returns(expected);
            var subject = new SafeSystemNetworkInterfaces(_logger, _origin);
            // Act
            var result = subject.Interfaces();
            // Assert
            _origin.Received().Interfaces();
            result.Should().HaveCount(2);
        }

        [DataTestMethod]
        [DataRow(typeof(NetworkInformationException))]
        public void Interfaces_ShouldSuppress_ExpectedException(Type exceptionType)
        {
            // Arrange
            var exception = (Exception)Activator.CreateInstance(exceptionType);
            _origin.Interfaces().Throws(exception);
            var subject = new SafeSystemNetworkInterfaces(_logger, _origin);
            // Act
            var result = subject.Interfaces();
            // Assert
            result.Should().BeEmpty();
        }

        [DataTestMethod]
        public void Interfaces_ShouldPass_NotExpectedException()
        {
            // Arrange
            var exception = new Exception();
            _origin.Interfaces().Throws(exception);
            var subject = new SafeSystemNetworkInterfaces(_logger, _origin);
            // Act
            Action action = () => subject.Interfaces();
            // Assert
            action.Should().Throw<Exception>();
        }

        [DataTestMethod]
        public void Interfaces_ShouldLog_ExpectedException()
        {
            // Arrange
            var exception = new NetworkInformationException();
            _origin.Interfaces().Throws(exception);
            var subject = new SafeSystemNetworkInterfaces(_logger, _origin);
            // Act
            _ = subject.Interfaces();
            // Assert
            _logger.ReceivedWithAnyArgs().Error("");
        }

        [TestMethod]
        public void Interface_ShouldBe_Origin_Interface()
        {
            // Arrange
            const string description = "Some interface";
            var expected = new TestNetworkInterface("t3");
            _origin.Interface(description).Returns(expected);
            var subject = new SafeSystemNetworkInterfaces(_logger, _origin);
            // Act
            var result = subject.Interface(description);
            // Assert
            _origin.Received().Interface(description);
            result.Id.Should().Be("t3");
        }

        [DataTestMethod]
        [DataRow(typeof(NetworkInformationException))]
        public void Interface_ShouldSuppress_ExpectedException(Type exceptionType)
        {
            // Arrange
            const string description = "Another interface";
            var exception = (Exception)Activator.CreateInstance(exceptionType);
            _origin.Interface(description).Throws(exception);
            var subject = new SafeSystemNetworkInterfaces(_logger, _origin);
            // Act
            var result = subject.Interface(description);
            // Assert
            result.Id.Should().BeEmpty();
        }

        [TestMethod]
        public void Interface_ShouldPass_NotExpectedException()
        {
            // Arrange
            const string description = "TG-ina";
            var exception = new Exception();
            _origin.Interface(description).Throws(exception);
            var subject = new SafeSystemNetworkInterfaces(_logger, _origin);
            // Act
            Action action = () => subject.Interface(description);
            // Assert
            action.Should().Throw<Exception>();
        }

        [DataTestMethod]
        public void Interface_ShouldLog_ExpectedException()
        {
            // Arrange
            const string description = "CE-ina";
            var exception = new NetworkInformationException();
            _origin.Interface(description).Throws(exception);
            var subject = new SafeSystemNetworkInterfaces(_logger, _origin);
            // Act
            _ = subject.Interface(description);
            // Assert
            _logger.ReceivedWithAnyArgs().Error("");
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
        }

        #endregion
    }
}
