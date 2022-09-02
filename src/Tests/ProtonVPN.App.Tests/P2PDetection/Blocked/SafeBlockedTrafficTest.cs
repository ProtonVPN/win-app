/*
 * Copyright (c) 2022 Proton Technologies AG
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
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ProtonVPN.P2PDetection.Blocked;

namespace ProtonVPN.App.Tests.P2PDetection.Blocked
{
    [TestClass]
    [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
    public class SafeBlockedTrafficTest
    {
        private IBlockedTraffic _origin;

        [TestInitialize]
        public void TestInitialize()
        {
            _origin = Substitute.For<IBlockedTraffic>();
        }

        [TestMethod]
        public void SafeBlockedTraffic_ShouldThrow_WhenOrigin_IsNull()
        {
            // Act
            Action action = () => new SafeBlockedTraffic(null);
            // Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task Detected_ShouldBe_Origin_Detected(bool value)
        {
            // Arrange
            _origin.Detected().Returns(value);
            SafeBlockedTraffic subject = new(_origin);
            // Act
            bool result = await subject.Detected();
            // Assert
            result.Should().Be(value);
        }

        [DataTestMethod]
        [DataRow(typeof(HttpRequestException))]
        [DataRow(typeof(OperationCanceledException))]
        public async Task Detected_ShouldSuppress_ExpectedException(Type exceptionType)
        {
            // Arrange
            Exception exception = (Exception)Activator.CreateInstance(exceptionType);
            _origin.Detected().Throws(exception);
            SafeBlockedTraffic subject = new(_origin);
            // Act
            bool result = await subject.Detected();
            // Assert
            result.Should().BeFalse();
        }

        [DataTestMethod]
        [DataRow(typeof(HttpRequestException))]
        [DataRow(typeof(OperationCanceledException))]
        public async Task Detected_ShouldSuppress_ExpectedException_Async(Type exceptionType)
        {
            // Arrange
            Exception exception = (Exception)Activator.CreateInstance(exceptionType);
            _origin.Detected().Returns(Task.FromException<bool>(exception));
            SafeBlockedTraffic subject = new(_origin);
            // Act
            bool result = await subject.Detected();
            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void Detected_ShouldPass_Exception()
        {
            // Arrange
            _origin.Detected().Throws<Exception>();
            SafeBlockedTraffic subject = new(_origin);
            // Act
            Func<Task> action = () => subject.Detected();
            // Assert
            action.Should().ThrowAsync<Exception>();
        }

        [TestMethod]
        public void Detected_ShouldPass_Exception_Async()
        {
            // Arrange
            _origin.Detected().Returns(Task.FromException<bool>(new Exception()));
            SafeBlockedTraffic subject = new(_origin);
            // Act
            Func<Task> action = async () => await subject.Detected();
            // Assert
            action.Should().ThrowAsync<Exception>();
        }
    }
}