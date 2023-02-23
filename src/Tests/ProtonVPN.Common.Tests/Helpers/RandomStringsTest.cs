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
using ProtonVPN.Common.Helpers;

namespace ProtonVPN.Common.Tests.Helpers
{
    [TestClass]
    public class ManagementPasswordsTest
    {
        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(10)]
        public void Password_ShouldHave_CorrectLength(int length)
        {
            // Arrange
            RandomStrings passwords = new();
            // Act
            string result = passwords.RandomString(length);
            // Assert
            result.Length.Should().Be(length);
        }

        [TestMethod]
        public void Password_ShouldThrow_WhenLength_IsNegative()
        {
            // Arrange
            RandomStrings passwords = new();
            // Act
            Action action = () => passwords.RandomString(-1);
            // Assert
            action.Should().Throw<ArgumentException>();
        }
    }
}
