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
using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ProtonVPN.Common.Text.Serialization;

namespace ProtonVPN.Common.Tests.Text.Serialization
{
    [TestClass]
    public class JsonSerializerTest
    {
        [TestMethod]
        public void Deserialize_ShouldHave_ExpectedPropertyValues()
        {
            // Arrange
            StringReader reader = new("{\"Number\":199,\"Name\":\"Super Mario\"}");
            JsonSerializer<SampleContract> serializer = new JsonSerializer<SampleContract>();

            // Act
            SampleContract result = serializer.Deserialize(reader);
            
            // Assert
            result.Number.Should().Be(199);
            result.Name.Should().Be("Super Mario");
        }

        [TestMethod]
        public void Deserialize_ShouldThrow_ExpectedException_WhenNotCorrectJson()
        {
            // Arrange
            StringReader reader = new("{Segment");
            JsonSerializer<SampleContract> serializer = new JsonSerializer<SampleContract>();

            // Act
            Action action = () => serializer.Deserialize(reader);

            // Assert
            action.Should().Throw<JsonException>();
        }

        [TestMethod]
        public void Serialize_ShouldSerialize_ToJson()
        {
            // Arrange
            SampleContract value = new() { Number = 34, Name = "ZgD" };
            JsonSerializer<SampleContract> serializer = new JsonSerializer<SampleContract>();
            StringWriter writer = new();

            // Act
            serializer.Serialize(value, writer);

            // Assert
            string result = writer.GetStringBuilder().ToString();
            result.Should().Be("{\"Number\":34,\"Name\":\"ZgD\"}");
        }
    }
}
