﻿/*
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
using ProtonVPN.Common.Legacy.OS.Net.NetworkInterface;

namespace ProtonVPN.Common.Tests.OS.Net.NetworkInterface
{
    [TestClass]
    public class NullNetworkInterfaceTest
    {
        [TestMethod]
        public void Id_ShouldBe_Empty()
        {
            // Arrange
            NullNetworkInterface subject = new();
            // Act
            string result = subject.Id;
            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void IsLoopback_ShouldBe_False()
        {
            // Arrange
            NullNetworkInterface subject = new();
            // Act
            bool result = subject.IsLoopback;
            // Assert
            result.Should().BeFalse();
        }
    }
}
