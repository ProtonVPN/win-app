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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.BugReporting.Attachments;
using System;
using System.Diagnostics.CodeAnalysis;

namespace ProtonVPN.App.Test.BugReporting.Attachments
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
            var attachment = new Attachment(path);
            // Assert
            attachment.Name.Should().Be(name);
        }

        [TestMethod]
        public void Path_ShouldBe_FilePath()
        {
            // Arrange
            const string path = "C:\\Program Files\\Some\\File 44.txt";
            // Act
            var attachment = new Attachment(path);
            // Assert
            attachment.Path.Should().Be(path);
        }

        [TestMethod]
        public void Length_ShouldBeZero_ByDefault()
        {
            // Act
            var attachment = new Attachment("filename");
            // Assert
            attachment.Length.Should().Be(0);
        }

        [TestMethod]
        public void ErrorType_ShouldBeNone_ByDefault()
        {
            // Act
            var attachment = new Attachment("some-file");
            // Assert
            attachment.ErrorType.Should().Be(AttachmentErrorType.None);
        }

        [TestMethod]
        public void WithLength_ShouldKeep_Name()
        {
            // Arrange
            const string path = "C:\\Programs\\Some\\File 55.txt";
            const string name = "File 55.txt";
            var attachment = new Attachment(path);
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
            var attachment = new Attachment(path);
            // Act
            attachment = attachment.WithLength(23457);
            // Assert
            attachment.Path.Should().Be(path);
        }

        [TestMethod]
        public void WithLength_ShouldKeep_ErrorType()
        {
            // Arrange
            const AttachmentErrorType errorType = AttachmentErrorType.TooManyFiles;
            var attachment = new Attachment("A").WithError(errorType);
            // Act
            attachment = attachment.WithLength(9876);
            // Assert
            attachment.ErrorType.Should().Be(errorType);
        }

        [TestMethod]
        public void WithLength_ShouldChange_FileLength()
        {
            // Arrange
            const long length = 45678;
            var attachment = new Attachment("file1");
            // Act
            attachment = attachment.WithLength(length);
            // Assert
            attachment.Length.Should().Be(length);
        }

        [TestMethod]
        public void WithError_ShouldKeep_Name()
        {
            // Arrange
            const string path = "C:\\Programs\\Some\\File 55.txt";
            const string name = "File 55.txt";
            var attachment = new Attachment(path);
            // Act
            attachment = attachment.WithError(AttachmentErrorType.TooManyFiles);
            // Assert
            attachment.Name.Should().Be(name);
        }

        [TestMethod]
        public void WithError_ShouldKeep_Path()
        {
            // Arrange
            const string path = "C:\\Program Files\\Some\\File-rr-33.txt";
            var attachment = new Attachment(path);
            // Act
            attachment = attachment.WithError(AttachmentErrorType.FileReadError);
            // Assert
            attachment.Path.Should().Be(path);
        }

        [TestMethod]
        public void WithError_ShouldKeep_Length()
        {
            // Arrange
            const long length = 90001;
            var attachment = new Attachment("B").WithLength(length);
            // Act
            attachment = attachment.WithError(AttachmentErrorType.None);
            // Assert
            attachment.Length.Should().Be(length);
        }

        [TestMethod]
        public void WithError_ShouldChange_ErrorType()
        {
            // Arrange
            const AttachmentErrorType errorType = AttachmentErrorType.FileReadError;
            var attachment = new Attachment("file1");
            // Act
            attachment = attachment.WithError(errorType);
            // Assert
            attachment.ErrorType.Should().Be(errorType);
        }

        #region IEquatable

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenOther_IsNull()
        {
            // Arrange
            var attachment = new Attachment("C:\\file");
            // Act
            var result = attachment.Equals(null);
            // Assert
            result.Should().Be(false);
        }

        [TestMethod]
        public void Equals_ShouldBeTrue_WhenOther_IsSelf()
        {
            // Arrange
            var attachment = new Attachment("C:\\file");
            // Act
            var result = attachment.Equals(attachment);
            // Assert
            result.Should().Be(true);
        }

        [TestMethod]
        public void Equals_ShouldBeTrue_WhenOther_HasSamePath()
        {
            // Arrange
            const string path = "C:\\Program Files\\Some\\File 22.txt";
            var attachment = new Attachment(path);
            var other = new Attachment(path);
            // Act
            var result = attachment.Equals(other);
            // Assert
            result.Should().Be(true);
        }

        [TestMethod]
        public void Equals_ShouldBeTrue_WhenOther_HasLowercasePath()
        {
            // Arrange
            const string path = "C:\\Program Files\\Some\\File 22.txt";
            var attachment = new Attachment(path);
            var other = new Attachment(path.ToLowerInvariant());
            // Act
            var result = attachment.Equals(other);
            // Assert
            result.Should().Be(true);
        }

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenOther_HasDifferentPath()
        {
            // Arrange
            const string path = "C:\\Program Files\\Some\\File 22.txt";
            const string otherPath = "C:\\Program Files\\Some\\File 11.txt";
            var attachment = new Attachment(path);
            var other = new Attachment(otherPath);
            // Act
            var result = attachment.Equals(other);
            // Assert
            result.Should().Be(false);
        }

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenOther_IsNull_Object()
        {
            // Arrange
            var attachment = new Attachment("C:\\file");
            // Act
            var result = attachment.Equals((object)null);
            // Assert
            result.Should().Be(false);
        }

        [TestMethod]
        public void Equals_ShouldBeTrue_WhenOther_IsSelf_Object()
        {
            // Arrange
            var attachment = new Attachment("C:\\file");
            // Act
            var result = attachment.Equals((object)attachment);
            // Assert
            result.Should().Be(true);
        }

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenOther_IsOtherType_Object()
        {
            // Arrange
            var attachment = new Attachment("C:\\file");
            var other = new object();
            // Act
            var result = attachment.Equals(other);
            // Assert
            result.Should().Be(false);
        }

        [TestMethod]
        public void Equals_ShouldBeTrue_WhenOtherObject_HasSamePath()
        {
            // Arrange
            const string path = "C:\\Program Files\\Some\\File 22.txt";
            var attachment = new Attachment(path);
            var other = new Attachment(path);
            // Act
            var result = attachment.Equals((object)other);
            // Assert
            result.Should().Be(true);
        }

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenOtherObject_HasDifferentPath()
        {
            // Arrange
            const string path = "C:\\Program Files\\Some\\File 22.txt";
            const string otherPath = "C:\\Program Files\\Some\\File 11.txt";
            var attachment = new Attachment(path);
            var other = new Attachment(otherPath);
            // Act
            var result = attachment.Equals((object)other);
            // Assert
            result.Should().Be(false);
        }

        [TestMethod]
        public void GetHashCode_ShouldDepend_OnPath()
        {
            // Arrange
            const string path = "C:\\Program Files\\Some\\File 22.txt";
            const string otherPath = "C:\\Program Files\\Some\\File 11.txt";
            // Act
            var hashCode = new Attachment(path).GetHashCode();
            var otherHashCode = new Attachment(otherPath).GetHashCode();
            // Assert
            hashCode.Should().NotBe(otherHashCode);
        }

        [TestMethod]
        public void GetHashCode_ShouldNotDepend_OnPathCase()
        {
            // Arrange
            const string path = "C:\\Program Files\\Some\\File 22.txt";
            // Act
            var hashCode = new Attachment(path).GetHashCode();
            var otherHashCode = new Attachment(path.ToLowerInvariant()).GetHashCode();
            // Assert
            hashCode.Should().Be(otherHashCode);
        }

        #endregion
    }
}
