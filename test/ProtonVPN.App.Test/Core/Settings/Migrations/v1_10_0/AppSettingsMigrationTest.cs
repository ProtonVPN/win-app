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
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Core.Settings.Migrations.v1_10_0;
using ProtonVPN.Core.Storage;

namespace ProtonVPN.App.Test.Core.Settings.Migrations.v1_10_0
{
    [TestClass]
    public class AppSettingsMigrationTest
    {
        private ISettingsStorage _storage;

        [TestInitialize]
        public void TestInitialize()
        {
            _storage = Substitute.For<ISettingsStorage>();
        }

        [TestMethod]
        public void ToVersion_ShouldBe_1_10_0()
        {
            // Arrange
            var migration = new AppSettingsMigration(_storage);
            // Act
            var result = migration.ToVersion;
            // Assert
            result.Should().Be(new Version(1, 10, 0));
        }

        [DataTestMethod]
        [DataRow("udp")]
        [DataRow("tcp")]
        [DataRow("auto")]
        public void Apply_ShouldSet_OvpnProtocol_ToAuto(string previousValue)
        {
            // Arrange
            _storage.Get<string>("OvpnProtocol").Returns(previousValue);
            var migration = new AppSettingsMigration(_storage);
            // Act
            migration.Apply();
            // Assert
            _storage.Received().Set("OvpnProtocol", "auto");
        }

        [DataTestMethod]
        [DataRow(true, "")]
        [DataRow(true, "{{\"User\":\"The.User.Name\",\"Value\":\"1.9.2\"}}")]
        [DataRow(false, "{{\"User\":\"The.User.Name\",\"Value\":\"1.10.0\"}}")]
        [DataRow(false, "{{\"User\":\"The.User.Name\",\"Value\":\"1.10.1\"}}")]
        public void Apply_Should_NotApplyMigration_OvpnProtocol_WhenUserSettingsVersion_Contains_1_10(bool migrated, string userSettingsVersion)
        {
            // Arrange
            _storage.Get<string>("UserSettingsVersion").Returns(userSettingsVersion);
            _storage.Get<string>("OvpnProtocol").Returns("udp");
            var migration = new AppSettingsMigration(_storage);
            // Act
            migration.Apply();
            // Assert
            if (migrated)
            {
                _storage.Received().Set("OvpnProtocol", "auto");
            }
            else
            {
                _storage.DidNotReceive().Set("OvpnProtocol", Arg.Any<string>());
            }
        }
    }
}
