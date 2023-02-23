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
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Core.Storage;
using ProtonVPN.Settings.Migrations;

namespace ProtonVPN.App.Tests.Core.Settings.Migrations
{
    [TestClass]
    [SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer")]
    public class BaseSettingsMigrationTest
    {
        private const string SettingsVersionKey = "SettingsVersion";

        private ISettingsStorage _storage;

        [TestInitialize]
        public void TestInitialize()
        {
            _storage = Substitute.For<ISettingsStorage>();
        }

        [TestMethod]
        public void Apply_ShouldSet_SettingsVersion_WhenLessThan_ToVersion()
        {
            // Arrange
            _storage.Get<string>(SettingsVersionKey).Returns("1.30.7");
            Migration migration = new Migration(_storage, "1.30.11");
            // Act
            migration.Apply();
            // Assert
            _storage.Received().Set(SettingsVersionKey, "1.30.11");
        }

        [TestMethod]
        public void Apply_ShouldCall_Migrate_WhenLessThan_ToVersion()
        {
            // Arrange
            _storage.Get<string>(SettingsVersionKey).Returns("1.30.7");
            Migration migration = new Migration(_storage, "1.30.11");
            // Act
            migration.Apply();
            // Assert
            migration.ReceivedMigrate.Should().BeTrue();
        }

        [DataTestMethod]
        [DataRow("1.30.9")]
        [DataRow("1.30.10")]
        public void Apply_ShouldNotSet_SettingsVersion_WhenEqualsOrGreaterThan_ToVersion(string toVersion)
        {
            // Arrange
            _storage.Get<string>(SettingsVersionKey).Returns("1.30.10");
            Migration migration = new Migration(_storage, toVersion);
            // Act
            migration.Apply();
            // Assert
            _storage.DidNotReceive().Set(SettingsVersionKey, Arg.Any<string>());
        }

        [DataTestMethod]
        [DataRow("1.30.9")]
        [DataRow("1.30.10")]
        public void Apply_ShouldNotCall_Migrate_WhenEqualsOrGreaterThan_ToVersion(string toVersion)
        {
            // Arrange
            _storage.Get<string>(SettingsVersionKey).Returns("1.30.10");
            Migration migration = new Migration(_storage, toVersion);
            // Act
            migration.Apply();
            // Assert
            migration.ReceivedMigrate.Should().BeFalse();
        }

        [TestMethod]
        public void SettingsVersion_ShouldGet_SettingsVersion()
        {
            // Arrange
            _storage.Get<string>(SettingsVersionKey).Returns("1.3.7");
            Migration migration = new Migration(_storage, "5.5.5");
            // Act
            Version result = migration.SettingsVersion;
            // Assert
            result.Should().Be(new Version(1,3,7));
        }

        [TestMethod]
        public void SettingsVersion_ShouldGet_SettingsVersion_WhenEmpty()
        {
            // Arrange
            _storage.Get<string>(SettingsVersionKey).Returns("");
            Migration migration = new Migration(_storage, "5.5.5");
            // Act
            Version result = migration.SettingsVersion;
            // Assert
            result.Should().Be(new Version(0, 0));
        }

        [TestMethod]
        public void SettingsVersion_ShouldGet_SettingsVersion_WhenNull()
        {
            // Arrange
            _storage.Get<string>(SettingsVersionKey).Returns((string)null);
            Migration migration = new Migration(_storage, "5.5.5");
            // Act
            Version result = migration.SettingsVersion;
            // Assert
            result.Should().Be(new Version(0, 0));
        }

        [TestMethod]
        public void SettingsVersion_ShouldSet_SettingsVersion()
        {
            // Arrange
            Migration migration = new Migration(_storage, "5.5.5");
            // Act
            migration.SettingsVersion = new Version(2, 17, 48);
            // Assert
            _storage.Received().Set(SettingsVersionKey, "2.17.48");
        }

        #region Helpers

        private class Migration : BaseSettingsMigration
        {
            public Migration(ISettingsStorage settings, string toVersion) : base(settings, toVersion)
            {
            }

            public new Version SettingsVersion
            {
                get => base.SettingsVersion;
                set => base.SettingsVersion = value;
            }

            public bool ReceivedMigrate { get; private set; }

            protected override void Migrate()
            {
                ReceivedMigrate = true;
            }
        }

        #endregion
    }
}
