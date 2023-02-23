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
using ProtonVPN.BugReporting.Attachments;

namespace ProtonVPN.App.Tests.BugReporting.Attachments
{
    [TestClass]
    [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
    public class AttachmentTest
    {
        [TestMethod]
        public void Attachment_ShouldThrow_WhenFilePath_IsNull()
        {
            // Act
            Action action = () => new Attachment(null);
            // Assert
            action.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void Attachment_ShouldThrow_WhenFilePath_IsEmpty()
        {
            // Act
            Action action = () => new Attachment("");
            // Assert
            action.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void Attachment_ShouldThrow_WhenFilePath_ContainsNoFileName()
        {
            // Act
            Action action = () => new Attachment("C:\\");
            // Assert
            action.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void Name_ShouldBe_NameOfFile()
        {
            // Arrange
            const string path = "C:\\Program Files\\Some\\File 55.txt";
            const string name = "File 55.txt";
            // Act
            Attachment attachment = new Attachment(path);
            // Assert
            attachment.Name.Should().Be(name);
        }

        [TestMethod]
        public void Path_ShouldBe_FilePath()
        {
            // Arrange
            const string path = "C:\\Program Files\\Some\\File 44.txt";
            // Act
            Attachment attachment = new Attachment(path);
            // Assert
            attachment.Path.Should().Be(path);
        }

        [TestMethod]
        public void Length_ShouldBeZero_ByDefault()
        {
            // Act
            Attachment attachment = new Attachment("filename");
            // Assert
            attachment.Length.Should().Be(0);
        }

        [TestMethod]
        public void WithLength_ShouldKeep_Name()
        {
            // Arrange
            const string path = "C:\\Programs\\Some\\File 55.txt";
            const string name = "File 55.txt";
            Attachment attachment = new Attachment(path);
            // Act
            attachment = attachment.WithLength(2245);
            // Assert
            attachment.Name.Should().Be(name);
        }

        [TestMethod]
        public void WithLength_ShouldKeep_Path()
        {
            // Arrange
            const string path = "C:\\Program Files\\Some\\File-rr-33.txt";
            Attachment attachment = new Attachment(path);
            // Act
            attachment = attachment.WithLength(23457);
            // Assert
            attachment.Path.Should().Be(path);
        }

        [TestMethod]
        public void WithLength_ShouldChange_FileLength()
        {
            // Arrange
            const long length = 45678;
            Attachment attachment = new Attachment("file1");
            // Act
            attachment = attachment.WithLength(length);
            // Assert
            attachment.Length.Should().Be(length);
        }
    }
}
