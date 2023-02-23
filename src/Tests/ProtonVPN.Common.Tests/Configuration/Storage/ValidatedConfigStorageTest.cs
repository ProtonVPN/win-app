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

namespace ProtonVPN.Common.Tests.Configuration.Storage
{
    [TestClass]
    public class ValidatedConfigStorageTest
    {
        private IConfigStorage _origin;

        [TestInitialize]
        public void TestInitialize()
        {
            _origin = Substitute.For<IConfigStorage>();
        }

        [TestMethod]
        public void Value_ShouldBe_OriginValue_WhenValid()
        {
            // Arrange
            IConfiguration contract = new DefaultConfig().Value();
            _origin.Value().Returns(contract);
            ValidatedConfigStorage config = new(_origin);
            // Act
            IConfiguration result = config.Value();
            // Assert
            result.Should().Be(contract);
        }

        [TestMethod]
        public void Value_ShouldBeNull_WhenNotValid()
        {
            // Arrange
            IConfiguration contract = new DefaultConfig().Value();
            contract.ReportBugMaxFiles = -1;
            _origin.Value().Returns(contract);
            ValidatedConfigStorage config = new(_origin);
            // Act
            IConfiguration result = config.Value();
            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void Value_ShouldBeNull_WhenNull()
        {
            // Arrange
            _origin.Value().ReturnsNull();
            ValidatedConfigStorage config = new(_origin);
            // Act
            IConfiguration result = config.Value();
            // Assert
            result.Should().BeNull();
        }
    }
}
