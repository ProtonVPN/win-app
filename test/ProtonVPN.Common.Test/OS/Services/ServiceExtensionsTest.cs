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

using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Abstract;
using ProtonVPN.Common.OS.Services;

namespace ProtonVPN.Common.Test.OS.Services
{
    [TestClass]
    public class ServiceExtensionsTest
    {
        private IService _service;

        [TestInitialize]
        public void TestInitialize()
        {
            _service = Substitute.For<IService>();
        }

        [TestMethod]
        public async Task StartAsync_ShouldBe_Service_Start()
        {
            // Arrange
            var expected = Result.Ok();
            _service.Start().Returns(expected);
            // Act
            var result = await _service.StartAsync();
            // Assert
            result.Should().BeSameAs(expected);
        }

        [TestMethod]
        public async Task StopAsync_ShouldBe_Service_Stop()
        {
            // Arrange
            var expected = Result.Fail();
            _service.Stop().Returns(expected);
            // Act
            var result = await _service.StopAsync();
            // Assert
            result.Should().BeSameAs(expected);
        }
    }
}
