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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Core.MVVM.Converters;

namespace ProtonVPN.App.Tests.Core.MVVM.Converters
{
    [TestClass]
    public class BytesToSizeConverterTest
    {
        [TestMethod]
        public void Convert_ShouldBe_0_WhenValue_IsNull()
        {
            // Arrange
            BytesToSizeConverter converter = new BytesToSizeConverter();
            // Act
            object result = converter.Convert(null, typeof(string), null, null);
            // Assert
            result.Should().Be("0");
        }

        [DataTestMethod]
        [DataRow("0", 0.0)]
        [DataRow("7", 7.329)]
        [DataRow("16", 15.68)]
        [DataRow("100", 100.0)]
        [DataRow("999", 999.0)]
        [DataRow("1023", 1023.0)]
        [DataRow("1.00", 1024.0)]
        [DataRow("9.99", 9.99 * 1024.0)]
        [DataRow("10.0", 10.0 * 1024.0)]
        [DataRow("99.9", 99.9 * 1024.0)]
        [DataRow("100", 100.0 * 1024.0)]
        [DataRow("999", 999.0 * 1024.0)]
        [DataRow("1023", 1023.0 * 1024.0)]
        [DataRow("1.00", 1024.0 * 1024.0)]
        [DataRow("10.0", 10.0 * 1024.0 * 1024.0)]
        [DataRow("100", 100.0 * 1024.0 * 1024.0)]
        [DataRow("999", 999.0 * 1024.0 * 1024.0)]
        [DataRow("1023", 1023.0 * 1024.0 * 1024.0)]
        [DataRow("1.00", 1024.0 * 1024.0 * 1024.0)]
        public void Convert_ShouldBe_Expected_WhenValue_Is(object expected, object value)
        {
            // Arrange
            BytesToSizeConverter converter = new BytesToSizeConverter();
            // Act
            object result = converter.Convert(value, typeof(string), null, null);
            // Assert
            result.Should().Be(expected);
        }

    }
}
