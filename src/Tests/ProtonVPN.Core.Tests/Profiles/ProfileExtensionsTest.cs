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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Core.Profiles;

namespace ProtonVPN.Core.Tests.Profiles
{
    [TestClass]
    public class ProfileExtensionsTest
    {
        [TestMethod]
        public void Exists_ShouldBe_False_WhenProfileIsNull()
        {
            // Act
            bool result = ((Profile) null).Exists();
            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void Exists_ShouldBe_True_WhenProfileIsNotNull()
        {
            // Arrange
            Profile profile = new Profile(null);
            // Act
            bool result = profile.Exists();
            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void IsValid_ShouldBe_False_WhenProfileIsNull()
        {
            // Act
            bool result = ((Profile) null).IsValid();
            // Assert
            result.Should().BeFalse();
        }

        [DataTestMethod]
        [DataRow(false, "12")]
        [DataRow(false, "270")]
        [DataRow(true, "")]
        [DataRow(true, null)]
        public void IsValid_ShouldBe_WhenServerId_Is(bool expected, string serverId)
        {
            // Arrange
            Profile profile = new(null) { ServerId = serverId, ColorCode = "#FFFFFF" };
            // Act
            bool result = profile.IsValid();
            // Assert
            result.Should().Be(expected);
        }

        [TestMethod]
        public void ModifiedLaterThan_ShouldBe_False_WhenProfile_IsNull()
        {
            // Arrange
            Profile another = new Profile(null) { ModifiedAt = DateTime.MinValue };
            // Act
            bool result = ((Profile) null).ModifiedLaterThan(another);
            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void ModifiedLaterThan_ShouldBe_False_WhenAnother_IsNull()
        {
            // Arrange
            Profile profile = new Profile(null) { ModifiedAt = DateTime.MaxValue };
            // Act
            bool result = profile.ModifiedLaterThan(null);
            // Assert
            result.Should().BeFalse();
        }

        [DataTestMethod]
        [DataRow(false,"2019-09-26 10:11:10", "2019-09-26 10:11:11")]
        [DataRow(false, "2019-09-26 10:11:11", "2019-09-26 10:11:11")]
        [DataRow(true, "2019-09-26 10:11:12", "2019-09-26 10:11:11")]
        public void ModifiedLaterThan_ShouldBe_WhenProfile_ModifiedAt_AndAnother_ModifiedAt_Are(bool expected, string profileModifiedAt, string anotherModifiedAt)
        {
            // Arrange
            Profile profile = new Profile(null) { ModifiedAt = DateTime.Parse(profileModifiedAt, CultureInfo.InvariantCulture.DateTimeFormat) };
            Profile another = new Profile(null) { ModifiedAt = DateTime.Parse(anotherModifiedAt, CultureInfo.InvariantCulture.DateTimeFormat) };
            // Act
            bool result = profile.ModifiedLaterThan(another);
            // Assert
            result.Should().Be(expected);
        }

        [TestMethod]
        public void WithIdFrom_ShouldBe_Null_WhenProfile_IsNull()
        {
            // Arrange
            Profile another = new Profile(null) { Id = "def" };
            // Act
            Profile profile = ((Profile) null).WithIdFrom(another);
            // Assert
            profile.Should().BeNull();
        }

        [TestMethod]
        public void WithIdFrom_Id_ShouldBe_Another_Id()
        {
            // Arrange
            Profile profile = new Profile(null) { Id = "abc" };
            Profile another = new Profile(null) { Id = "def" };
            // Act
            profile = profile.WithIdFrom(another);
            // Assert
            profile.Id.Should().Be("def");
        }

        [TestMethod]
        public void WithIdFrom_Id_ShouldBeSame_WhenAnother_IsNull()
        {
            // Arrange
            Profile profile = new Profile(null) { Id = "abc" };
            // Act
            profile = profile.WithIdFrom(null);
            // Assert
            profile.Id.Should().Be("abc");
        }

        [TestMethod]
        public void WithExternalIdFrom_ShouldBeNull_WhenProfile_IsNull()
        {
            // Arrange
            Profile another = new Profile(null) { ExternalId = "def" };
            // Act
            Profile profile = ((Profile)null).WithExternalIdFrom(another);
            // Assert
            profile.Should().BeNull();
        }

        [TestMethod]
        public void WithExternalIdFrom_ExternalId_ShouldBe_Another_ExternalId()
        {
            // Arrange
            Profile profile = new Profile(null) { ExternalId = "abc" };
            Profile another = new Profile(null) { ExternalId = "def" };
            // Act
            profile = profile.WithExternalIdFrom(another);
            // Assert
            profile.ExternalId.Should().Be("def");
        }

        [TestMethod]
        public void WithExternalIdFrom_ExternalId_ShouldBeSame_WhenAnother_IsNull()
        {
            // Arrange
            Profile profile = new Profile(null) { ExternalId = "abc" };
            // Act
            profile = profile.WithExternalIdFrom(null);
            // Assert
            profile.ExternalId.Should().Be("abc");
        }

        [TestMethod]
        public void WithModifiedAt_ModifiedAt_ShouldBe_Value()
        {
            // Arrange
            Profile profile = new Profile(null) { ModifiedAt = DateTime.Parse("2020-01-02 03:04:05") };
            DateTime value = DateTime.Parse("2019-10-11 12:13:14");
            // Act
            profile = profile.WithModifiedAt(value);
            // Assert
            profile.ModifiedAt.Should().Be(value);
        }

        [TestMethod]
        public void WithStatus_Status_ShouldBe_Value()
        {
            // Arrange
            Profile profile = new Profile(null) { Status = ProfileStatus.Created };
            const ProfileStatus value = ProfileStatus.Synced;
            // Act
            profile = profile.WithStatus(value);
            // Assert
            profile.Status.Should().Be(value);
        }

        [TestMethod]
        public void WithStatus_OriginalName_ShouldBe_Null_WhenValueIs_Synced()
        {
            // Arrange
            Profile profile = new Profile(null) { OriginalName = "Profile name" };
            const ProfileStatus value = ProfileStatus.Synced;
            // Act
            profile = profile.WithStatus(value);
            // Assert
            profile.OriginalName.Should().BeNull();
        }

        [TestMethod]
        public void WithStatus_UniqueNameIndex_ShouldBe_Zero_WhenValueIs_Synced()
        {
            // Arrange
            Profile profile = new Profile(null) { UniqueNameIndex = 5 };
            const ProfileStatus value = ProfileStatus.Synced;
            // Act
            profile = profile.WithStatus(value);
            // Assert
            profile.UniqueNameIndex.Should().Be(0);
        }

        [TestMethod]
        public void WithSyncStatus_SyncStatus_ShouldBe_Value()
        {
            // Arrange
            Profile profile = new Profile(null) { SyncStatus = ProfileSyncStatus.InProgress };
            const ProfileSyncStatus value = ProfileSyncStatus.Succeeded;
            // Act
            profile = profile.WithSyncStatus(value);
            // Assert
            profile.SyncStatus.Should().Be(value);
        }

        [TestMethod]
        public void WithStatusMergedFrom_Status_ShouldBe_Another_Status_When_StatusIs_Updated()
        {
            // Arrange
            Profile profile = new Profile(null) { Status = ProfileStatus.Updated };
            Profile another = new Profile(null) { Status = ProfileStatus.Deleted };
            // Act
            profile = profile.WithStatusMergedFrom(another);
            // Assert
            profile.Status.Should().Be(ProfileStatus.Deleted);
        }

        [TestMethod]
        public void WithStatusMergedFrom_Status_ShouldBeSame_WhenAnother_IsNull()
        {
            // Arrange
            Profile profile = new Profile(null) { Status = ProfileStatus.Created };
            // Act
            profile = profile.WithStatusMergedFrom(null);
            // Assert
            profile.Status.Should().Be(ProfileStatus.Created);
        }

        [DataTestMethod]
        [DataRow(ProfileStatus.Created)]
        [DataRow(ProfileStatus.Deleted)]
        public void WithStatusMergedFrom_Status_ShouldBeSame_When_StatusIsNot_Updated(ProfileStatus status)
        {
            // Arrange
            Profile profile = new Profile(null) { Status = status };
            Profile another = new Profile(null) { Status = ProfileStatus.Updated };
            // Act
            profile = profile.WithStatusMergedFrom(another);
            // Assert
            profile.Status.Should().Be(status);
        }

        [TestMethod]
        public void WithUniqueNameCandidate_Name_ShouldBeSame()
        {
            // Arrange
            Profile profile = new Profile(null) { Name = "Buggy" };
            // Act
            Profile result = profile.WithUniqueNameCandidate(25);
            // Assert
            result.Name.Should().Be("Buggy");
        }

        [TestMethod]
        public void WithNextUniqueNameCandidate_Name_ShouldBeUnique_InASequence()
        {
            // Arrange
            Profile profile = new Profile(null) { Name = "Buggy" }.WithUniqueNameCandidate(25);
            string[] expected = new[] { "Buggy (1)", "Buggy (2)", "Buggy (3)", "Buggy (4)" };
            // Act
            IEnumerable<string> result = Enumerable.Range(1, expected.Length)
                .Select(_ => profile = profile.WithNextUniqueNameCandidate(25))
                .Select(p => p.Name);
            // Assert
            result.Should().ContainInOrder(expected);
        }

        [TestMethod]
        public void WithNextUniqueNameCandidate_Name_ShouldBeTrimmed_ToMaxLength()
        {
            // Arrange
            Profile profile = new Profile(null) { Name = "Buggy" }.WithUniqueNameCandidate(7);
            string[] expected = new[] { "Bug (1)", "Bug (2)", "Bug (3)", "Bug (4)" };
            // Act
            IEnumerable<string> result = Enumerable.Range(1, expected.Length)
                .Select(_ => profile = profile.WithNextUniqueNameCandidate(7))
                .Select(p => p.Name);
            // Assert
            result.Should().ContainInOrder(expected);
        }
    }
}
