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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Common.Helpers;
using System;

namespace ProtonVPN.Common.Test.Helpers
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
            var passwords = new RandomStrings();
            // Act
            var result = passwords.RandomString(length);
            // Assert
            result.Length.Should().Be(length);
        }

        [TestMethod]
        public void Password_ShouldThrow_WhenLength_IsNegative()
        {
            // Arrange
            var passwords = new RandomStrings();
            // Act
            Action action = () => passwords.RandomString(-1);
            // Assert
            action.Should().Throw<ArgumentException>();
        }
    }
}
