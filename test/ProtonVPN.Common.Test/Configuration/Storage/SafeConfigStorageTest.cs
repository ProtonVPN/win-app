/*
 * Copyright (c) 2020 Proton Technologies AG
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
using NSubstitute;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Configuration.Storage;

namespace ProtonVPN.Common.Test.Configuration.Storage
{
    [TestClass]
    public class SafeConfigStorageTest
    {
        private IConfigStorage _origin;

        [TestInitialize]
        public void TestInitialize()
        {
            _origin = Substitute.For<IConfigStorage>();
        }

        [TestMethod]
        public void Value_ShouldBe_OriginValue()
        {
            // Arrange
            var expected = new Config();
            _origin.Value().Returns(expected);
            var storage = new SafeConfigStorage(_origin);
            // Act
            var value = storage.Value();
            // Assert
            value.Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow(typeof(IOException))]
        [DataRow(typeof(UnauthorizedAccessException))]
        [DataRow(typeof(JsonException))]
        public void Value_ShouldBeNull_WhenOriginThrows_ExpectedException(Type exceptionType)
        {
            // Arrange
            var exception = (Exception)Activator.CreateInstance(exceptionType);
            _origin.When(x => x.Value()).Do(_ => throw exception);
            var storage = new SafeConfigStorage(_origin);
            // Act
            var result = storage.Value();
            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void Value_ShouldPass_UnexpectedException()
        {
            // Arrange
            _origin.When(x => x.Value()).Do(_ => throw new Exception());
            var storage = new SafeConfigStorage(_origin);
            // Act
            Action action = () => storage.Value();
            // Assert
            action.Should().Throw<Exception>();
        }
    }
}
