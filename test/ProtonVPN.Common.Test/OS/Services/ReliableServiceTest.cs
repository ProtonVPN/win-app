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
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Abstract;
using ProtonVPN.Common.OS.Services;

namespace ProtonVPN.Common.Test.OS.Services
{
    [TestClass]
    public class ReliableServiceTest
    {
        private IServiceRetryPolicy _retryPolicy;
        private IService _origin;

        [TestInitialize]
        public void TestInitialize()
        {
            _retryPolicy = Substitute.For<IServiceRetryPolicy>();
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
        public void Running_ShouldBe_Origin_Running(bool value)
        {
            // Arrange
            _origin.Running().Returns(value);
            var subject = new ReliableService(_retryPolicy, _origin);

            // Act
            var result = subject.Running();

            // Assert
            result.Should().Be(value);
        }

        [TestMethod]
        public async Task StartAsync_ShouldBe_Origin_StartAsync()
        {
            // Arrange
            var expected = Result.Ok();
            var cancellationToken = new CancellationToken();
            _origin.StartAsync(cancellationToken).Returns(expected);
            _retryPolicy
                .ExecuteAsync(Arg.Any<Func<CancellationToken, Task<Result>>>(), Arg.Any<CancellationToken>())
                .Returns(args => args.Arg<Func<CancellationToken, Task<Result>>>()(cancellationToken));
            var subject = new ReliableService(_retryPolicy, _origin);

            // Act
            var result = await subject.StartAsync(cancellationToken);

            // Assert
            result.Should().BeSameAs(expected);
        }

        [TestMethod]
        public async Task StopAsync_ShouldBe_Origin_StopAsync()
        {
            // Arrange
            var expected = Result.Ok();
            var cancellationToken = new CancellationToken();
            _origin.StopAsync(cancellationToken).Returns(expected);
            var subject = new ReliableService(_retryPolicy, _origin);

            // Act
            var result = await subject.StopAsync(cancellationToken);

            // Assert
            result.Should().BeSameAs(expected);
        }
    }
}
