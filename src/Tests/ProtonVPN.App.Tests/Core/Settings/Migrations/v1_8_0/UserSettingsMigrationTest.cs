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
using NSubstitute;
using ProtonVPN.Core.Storage;
using ProtonVPN.Settings.Migrations.v1_8_0;

namespace ProtonVPN.App.Tests.Core.Settings.Migrations.v1_8_0
{
    [TestClass]
    public class UserSettingsMigrationTest
    {
        private ISettingsStorage _appSettings;
        private ISettingsStorage _userSettings;

        [TestInitialize]
        public void TestInitialize()
        {
            _appSettings = Substitute.For<ISettingsStorage>();
            _userSettings = Substitute.For<ISettingsStorage>();
        }

        [TestMethod]
        public void ToVersion_ShouldBe_1_8_0()
        {
            // Arrange
            UserSettingsMigration migration = new UserSettingsMigration(_appSettings, _userSettings);
            // Act
            Version result = migration.ToVersion;
            // Assert
            result.Should().Be(new Version(1, 8, 0));
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Apply_ShouldMigrate_SecureCore(bool value)
        {
            // Arrange
            _appSettings.Get<bool>("SecureCore").Returns(value);
            UserSettingsMigration migration = new UserSettingsMigration(_appSettings, _userSettings);
            // Act
            migration.Apply();
            // Assert
            _userSettings.Received().Set("SecureCore", value);
        }
    }
}
