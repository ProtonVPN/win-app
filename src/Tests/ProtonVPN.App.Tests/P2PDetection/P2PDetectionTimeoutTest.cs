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
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Configuration;
using ProtonVPN.P2PDetection;

namespace ProtonVPN.App.Tests.P2PDetection
{
    [TestClass]
    public class P2PDetectionTimeoutTest
    {
        [TestMethod]
        public void Value_ShouldBe_HalfOfTheInterval()
        {
            // Arrange
            TimeSpan interval = TimeSpan.FromSeconds(30);
            IConfiguration config = Substitute.For<IConfiguration>();
            config.P2PCheckInterval.Returns(interval);
            // Act
            TimeSpan timeout = new P2PDetectionTimeout(config).GetTimeoutValue();
            // Assert
            timeout.Should().BeCloseTo(TimeSpan.FromSeconds(15), TimeSpan.FromMilliseconds(1000));
        }
    }
}