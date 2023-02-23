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
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Vpn.OpenVpn.Arguments;

namespace ProtonVPN.Vpn.Tests.OpenVpn.Arguments
{
    [TestClass]
    public class NoBindArgumentsTest
    {
        [TestMethod]
        public void Enumerable_ShouldContain_ExpectedNumberOfOptions()
        {
            // Arrange
            NoBindArguments subject = new();

            // Act
            List<string> result = subject.ToList();

            // Assert
            result.Should().HaveCount(1);
        }

        [TestMethod]
        public void Enumerable_ShouldContain_NoBindOption()
        {
            // Arrange
            NoBindArguments subject = new();

            // Act
            List<string> result = subject.ToList();

            // Assert
            result.Should().Contain($"--nobind");
        }
    }
}
