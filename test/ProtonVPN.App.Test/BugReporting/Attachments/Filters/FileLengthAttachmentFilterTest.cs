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
using NSubstitute;
using ProtonVPN.BugReporting.Attachments;
using ProtonVPN.BugReporting.Attachments.Filters;
using ProtonVPN.Common.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ProtonVPN.App.Test.BugReporting.Attachments.Filters
{
    [TestClass]
    [DeploymentItem("BugReporting\\Attachments\\TestData", "TestData")]
    [SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed")]
    [SuppressMessage("ReSharper", "CoVariantArrayConversion")]
    public class FileLengthAttachmentFilterTest
    {
        private ILogger _logger;

        [TestInitialize]
        public void TestInitialize()
        {
            _logger = Substitute.For<ILogger>();
        }

        [TestMethod]
        public void Enumerable_ShouldBe_Empty_WhenSource_IsEmpty()
        {
            // Arrange
            var source = Enumerable.Empty<Attachment>();
            var filter = new FileLengthAttachmentFilter(_logger, source);
            // Act
            var result = filter.ToList();
            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void Enumerable_ShouldBe_Attachments_FromSource()
        {
            // Arrange
            var source = new[]
            {
                new Attachment("TestData\\test.txt"),
                new Attachment("TestData\\test-2.txt"),
                new Attachment("TestData\\test-3.txt")
            };
            var filter = new FileLengthAttachmentFilter(_logger, source);
            // Act
            var result = filter.ToList();
            // Assert
            result.Should().BeEquivalentTo(source);
        }

        [TestMethod]
        public void Attachment_Length_ShouldBe_FileSize()
        {
            // Arrange
            var source = new[] { new Attachment("TestData\\test.txt") };
            var filter = new FileLengthAttachmentFilter(_logger, source);
            // Act
            var result = filter.ToList();
            // Assert
            result.First().Length.Should().Be(7);
        }

        [TestMethod]
        public void Attachment_ErrorType_ShouldBe_FileReadError_WhenFileDoesNotExist()
        {
            // Arrange
            var source = new[] { new Attachment("Not existing\\no file.rrr") };
            var filter = new FileLengthAttachmentFilter(_logger, source);
            // Act
            var result = filter.ToList();
            // Assert
            result.First().ErrorType.Should().Be(AttachmentErrorType.FileReadError);
        }

        [TestMethod]
        public void Enumerable_ShouldLog_WhenFileDoesNotExist()
        {
            // Arrange
            var source = new[] { new Attachment("Not existing\\no file.rrr") };
            var filter = new FileLengthAttachmentFilter(_logger, source);
            // Act
            filter.ToList();
            // Assert
            _logger.ReceivedWithAnyArgs().Warn("");
        }
    }
}
