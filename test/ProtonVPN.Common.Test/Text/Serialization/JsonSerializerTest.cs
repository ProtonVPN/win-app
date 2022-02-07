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

using System;
using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ProtonVPN.Common.Text.Serialization;

namespace ProtonVPN.Common.Test.Text.Serialization
{
    [TestClass]
    public class JsonSerializerTest
    {
        [TestMethod]
        public void Deserialize_ShouldHave_ExpectedPropertyValues()
        {
            // Arrange
            var reader = new StringReader("{\"Number\":199,\"Name\":\"Super Mario\"}");
            var serializer = new JsonSerializer<SampleContract>();

            // Act
            var result = serializer.Deserialize(reader);
            
            // Assert
            result.Number.Should().Be(199);
            result.Name.Should().Be("Super Mario");
        }

        [TestMethod]
        public void Deserialize_ShouldThrow_ExpectedException_WhenNotCorrectJson()
        {
            // Arrange
            var reader = new StringReader("{Segment");
            var serializer = new JsonSerializer<SampleContract>();

            // Act
            Action action = () => serializer.Deserialize(reader);

            // Assert
            action.Should().Throw<JsonException>();
        }

        [TestMethod]
        public void Serialize_ShouldSerialize_ToJson()
        {
            // Arrange
            var value = new SampleContract { Number = 34, Name = "ZgD" };
            var serializer = new JsonSerializer<SampleContract>();
            var writer = new StringWriter();

            // Act
            serializer.Serialize(value, writer);

            // Assert
            var result = writer.GetStringBuilder().ToString();
            result.Should().Be("{\"Number\":34,\"Name\":\"ZgD\"}");
        }

        [DataTestMethod]
        [DataRow(true, typeof(JsonException))]
        [DataRow(false, typeof(InvalidOperationException))]
        [DataRow(false, typeof(IOException))]
        [DataRow(false, typeof(Exception))]
        public void IsExpectedException_ShouldBeTrue_WhenExpectedException(bool expected, Type exceptionType)
        {
            // Arrange
            var exception = (Exception)Activator.CreateInstance(exceptionType);
            var serializer = new JsonSerializer<SampleContract>();

            // Act
            var result = serializer.IsExpectedException(exception);

            // Assert
            result.Should().Be(expected);
        }

        #region Helpers

        public class SampleContract
        {
            public int Number { get; set; }
            public string Name{ get; set; }
        }

        #endregion
    }
}
