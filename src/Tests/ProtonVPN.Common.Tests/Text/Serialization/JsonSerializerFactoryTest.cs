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
using ProtonVPN.Common.Text.Serialization;

namespace ProtonVPN.Common.Tests.Text.Serialization
{
    [TestClass]
    public class JsonSerializerFactoryTest
    {
        [TestMethod]
        public void Serializer_Should_NotBeNull()
        {
            // Arrange
            JsonSerializerFactory factory = new();

            // Act
            ITextSerializer<int> result = factory.Serializer<int>();

            // Assert
            result.Should().NotBeNull();
        }

        [TestMethod]
        public void Serializer_ShouldBe_JsonSerializer()
        {
            // Arrange
            JsonSerializerFactory factory = new();

            // Act
            ITextSerializer<int> result = factory.Serializer<int>();

            // Assert
            result.Should().BeOfType<JsonSerializer<int>>();
        }
    }
}
