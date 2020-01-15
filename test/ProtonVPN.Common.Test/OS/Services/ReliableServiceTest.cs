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
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Polly;
using ProtonVPN.Common.Abstract;
using ProtonVPN.Common.OS.Services;

namespace ProtonVPN.Common.Test.OS.Services
{
    [TestClass]
    public class ReliableServiceTest
    {
        private Policy<Result> _retryPolicy;
        private IService _origin;

        [TestInitialize]
        public void TestInitialize()
        {
            _retryPolicy = Policy<Result>.HandleResult(r => r.Failure).WaitAndRetry(3, retryAttempt => TimeSpan.Zero);
            _origin = Substitute.For<IService>();
        }

        [TestMethod]
        public void Name_ShouldBe_Origin_Name()
        {
            // Arrange
            const string name = "My service";
            _origin.Name.Returns(name);
            var subject = new ReliableService(_retryPolicy, _origin);
            // Act
            var result = subject.Name;
            // Assert
            result.Should().Be(name);
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void IsRunning_ShouldBe_Origin_IsRunning(bool value)
        {
            // Arrange
            _origin.IsRunning().Returns(value);
            var subject = new ReliableService(_retryPolicy, _origin);
            // Act
            var result = subject.IsRunning();
            // Assert
            result.Should().Be(value);
        }

        [TestMethod]
        public void Start_ShouldBe_Origin_Start()
        {
            // Arrange
            var expected = Result.Ok();
            _origin.Start().Returns(expected);
            var subject = new ReliableService(_retryPolicy, _origin);
            // Act
            var result = subject.Start();
            // Assert
            result.Should().BeSameAs(expected);
        }

        [TestMethod]
        public void Start_ShouldCall_Origin_Start_RetryCountPlusOneTime_WhenReturnsFailure()
        {
            // Arrange
            var expected = Result.Fail();
            _origin.Start().Returns(expected);
            var subject = new ReliableService(_retryPolicy, _origin);
            // Act
            var result = subject.Start();
            // Assert
            result.Should().BeSameAs(expected);
            _origin.Received(4).Start();
        }

        [TestMethod]
        public void Stop_ShouldBe_Origin_Stop()
        {
            // Arrange
            var expected = Result.Ok();
            _origin.Stop().Returns(expected);
            var subject = new ReliableService(_retryPolicy, _origin);
            // Act
            var result = subject.Stop();
            // Assert
            result.Should().BeSameAs(expected);
        }

        [TestMethod]
        public void Stop_ShouldCall_Origin_Stop_RetryCountPlusOneTime_WhenReturnsFailure()
        {
            // Arrange
            var expected = Result.Fail();
            _origin.Stop().Returns(expected);
            var subject = new ReliableService(_retryPolicy, _origin);
            // Act
            var result = subject.Stop();
            // Assert
            result.Should().BeSameAs(expected);
            _origin.Received(4).Stop();
        }
    }
}
