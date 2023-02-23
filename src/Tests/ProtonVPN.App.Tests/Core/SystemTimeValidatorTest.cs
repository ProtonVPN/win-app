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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Logging;
using ProtonVPN.Core;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.OS;
using ProtonVPN.Modals;

namespace ProtonVPN.App.Tests.Core
{
    [TestClass]
    public class SystemTimeValidatorTest
    {
        private INtpClient _ntpClient;
        private IModals _modals;
        private ILogger _logger;

        private static IEnumerable<object[]> Timestamps => new[]
        {
            new object[] {DateTime.UtcNow + TimeSpan.FromSeconds(120)},
            new object[] {DateTime.UtcNow - TimeSpan.FromSeconds(120)},
        };

        [TestInitialize]
        public void Initialize()
        {
            _ntpClient = Substitute.For<INtpClient>();
            _modals = Substitute.For<IModals>();
            _logger = Substitute.For<ILogger>();
        }

        [TestMethod]
        [DynamicData(nameof(Timestamps))]
        public async Task ValidateShouldShowModal(DateTime dateTime)
        {
            // Arrange
            _ntpClient.GetNetworkUtcTime().Returns(dateTime);

            // Act
            SystemTimeValidator sut = new(_ntpClient, _modals, _logger);
            await sut.Validate();

            // Assert
            _modals.Received(1).Show<IncorrectSystemTimeModalViewModel>();
        }

        [TestMethod]
        public async Task ValidateShouldNotShowModal()
        {
            // Arrange
            _ntpClient.GetNetworkUtcTime().Returns(DateTime.UtcNow);

            // Act
            SystemTimeValidator sut = new(_ntpClient, _modals, _logger);
            await sut.Validate();

            // Assert
            _modals.Received(0).Show<IncorrectSystemTimeModalViewModel>();
        }
    }
}