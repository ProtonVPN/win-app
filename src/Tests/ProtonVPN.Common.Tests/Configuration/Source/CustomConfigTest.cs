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
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Configuration.Source;
using ProtonVPN.Common.Configuration.Storage;

namespace ProtonVPN.Common.Tests.Configuration.Source
{
    [TestClass]
    public class CustomConfigTest
    {
        private IConfigSource _default;
        private IConfigStorage _storage;

        [TestInitialize]
        public void TestInitialize()
        {
            _default = Substitute.For<IConfigSource>();
            _storage = Substitute.For<IConfigStorage>();
        }

        [TestMethod]
        public void Value_ShouldBe_Default_WhenMode_IsDefault()
        {
            // Arrange
            IConfiguration defaultValue = new DefaultConfig().Value();
            _default.Value().Returns(defaultValue);
            CustomConfig config = new(ConfigMode.Default, _default, _storage);
            // Act
            IConfiguration result = config.Value();
            // Assert
            result.Should().Be(defaultValue);
        }

        [TestMethod]
        public void Value_ShouldBe_StorageValue_WhenMode_IsNotDefault()
        {
            // Arrange
            IConfiguration value = new DefaultConfig().Value();
            _default.Value().Returns(new DefaultConfig().Value());
            _storage.Value().Returns(value);
            CustomConfig config = new(ConfigMode.CustomOrDefault, _default, _storage);
            // Act
            IConfiguration result = config.Value();
            // Assert
            result.Should().Be(value);
        }

        [TestMethod]
        public void Value_ShouldBe_Default_WhenMode_IsNotDefault_AndStorageValue_IsNull()
        {
            // Arrange
            IConfiguration defaultValue = new DefaultConfig().Value();
            _default.Value().Returns(defaultValue);
            _storage.Value().ReturnsNull();
            CustomConfig config = new(ConfigMode.CustomOrDefault, _default, _storage);
            // Act
            IConfiguration result = config.Value();
            // Assert
            result.Should().Be(defaultValue);
        }

        [TestMethod]
        public void Value_ShouldCall_StorageSave_WhenMode_IsCustomOrSavedDefault_AndStorageValue_IsNull()
        {
            // Arrange
            IConfiguration defaultValue = new DefaultConfig().Value();
            _default.Value().Returns(defaultValue);
            _storage.Value().ReturnsNull();
            CustomConfig config = new(ConfigMode.CustomOrSavedDefault, _default, _storage);
            // Act
            config.Value();
            // Assert
            _storage.Received().Save(defaultValue);
        }
    }
}
