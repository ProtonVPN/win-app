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
using ProtonVPN.Common.Logging;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ProtonVPN.App.Test.BugReporting.Attachments
{
    [TestClass]
    [DeploymentItem("BugReporting\\Attachments\\TestData", "TestData")]
    [SuppressMessage("ReSharper", "CoVariantArrayConversion")]
    [SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed")]
    public class AttachmentsTest
    {
        private ILogger _logger;
        private Common.Configuration.Config _appConfig;
        private IEnumerable<Attachment> _logFileSource;
        private IEnumerable<Attachment> _selectFileSource;

        [TestInitialize]
        public void TestInitialize()
        {
            _logger = Substitute.For<ILogger>();
            _appConfig = new Common.Configuration.Config();
            _logFileSource = Substitute.For<IEnumerable<Attachment>>();
            _selectFileSource = Substitute.For<IEnumerable<Attachment>>();

            _appConfig.ReportBugMaxFiles = 3;
            _appConfig.ReportBugMaxFileSize = 1000;
            _logFileSource.GetEnumerator().Returns(Enumerable.Empty<Attachment>().GetEnumerator());
            _selectFileSource.GetEnumerator().Returns(Enumerable.Empty<Attachment>().GetEnumerator());
        }

        [TestMethod]
        public void Load_ShouldClearItems_BeforeLoading()
        {
            // Arrange
            var items = new[]
            {
                new Attachment("TestData\\test.txt"),
                new Attachment("TestData\\test-2.txt"),
                new Attachment("TestData\\test-3.txt")
            };
            _logFileSource.GetEnumerator().Returns(items.Cast<Attachment>().GetEnumerator());
            var subject = new ProtonVPN.BugReporting.Attachments.Attachments(_logger, _appConfig, _logFileSource, _selectFileSource);

            subject.Load();
            subject.Items.Should().NotBeEmpty();
            _logFileSource.GetEnumerator().Returns(Enumerable.Empty<Attachment>().GetEnumerator());
            // Act
            subject.Load();
            // Assert
            subject.Items.Should().BeEmpty();
        }

        [TestMethod]
        public void Load_ShouldEnumerate_LogFileSource()
        {
            // Arrange
            var subject = new ProtonVPN.BugReporting.Attachments.Attachments(_logger, _appConfig, _logFileSource, _selectFileSource);
            // Act
            subject.Load();
            // Assert
            _logFileSource.Received(1).GetEnumerator();
        }

        [TestMethod]
        public void Load_ShouldNotEnumerate_SelectFileSource()
        {
            // Arrange
            var subject = new ProtonVPN.BugReporting.Attachments.Attachments(_logger, _appConfig, _logFileSource, _selectFileSource);
            // Act
            subject.Load();
            // Assert
            _selectFileSource.DidNotReceive().GetEnumerator();
        }

        [TestMethod]
        public void Load_ShouldLoadItems_FromLogFileSource()
        {
            // Arrange
            var items = new[]
            {
                new Attachment("TestData\\test.txt"),
                new Attachment("TestData\\test-2.txt"),
                new Attachment("TestData\\test-3.txt")
            };
            _logFileSource.GetEnumerator().Returns(items.Cast<Attachment>().GetEnumerator());
            var subject = new ProtonVPN.BugReporting.Attachments.Attachments(_logger, _appConfig, _logFileSource, _selectFileSource);
            // Act
            subject.Load();
            // Assert
            subject.Items.Should().BeEquivalentTo(items);
        }

        [TestMethod]
        public void Items_ShouldByEmpty_Initially()
        {
            // Arrange
            var subject = new ProtonVPN.BugReporting.Attachments.Attachments(_logger, _appConfig, _logFileSource, _selectFileSource);
            // Act
            var result = subject.Items;
            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void SelectFiles_ShouldEnumerate_SelectFileSource()
        {
            // Arrange
            var subject = new ProtonVPN.BugReporting.Attachments.Attachments(_logger, _appConfig, _logFileSource, _selectFileSource);
            // Act
            subject.SelectFiles();
            // Assert
            _selectFileSource.Received(1).GetEnumerator();
        }

        [TestMethod]
        public void SelectFiles_ShouldLoad_UpToMaxItems()
        {
            // Arrange
            const int maxItems = 2;
            _appConfig.ReportBugMaxFiles = maxItems;
            var items = new[]
            {
                new Attachment("TestData\\test.txt"),
                new Attachment("TestData\\test-2.txt"),
                new Attachment("TestData\\test-3.txt"),
                new Attachment("TestData\\test-4.txt")
            };
            _selectFileSource.GetEnumerator().Returns(items.Cast<Attachment>().GetEnumerator());
            var subject = new ProtonVPN.BugReporting.Attachments.Attachments(_logger, _appConfig, _logFileSource, _selectFileSource);
            // Act
            subject.SelectFiles();
            // Assert
            subject.Items.Should().HaveCount(maxItems);
        }

        [TestMethod]
        public void SelectFiles_ShouldRaise_OnErrorOccured_WithSkippedItems()
        {
            // Arrange
            const int maxItems = 2;
            _appConfig.ReportBugMaxFiles = maxItems;
            var items = new[]
            {
                new Attachment("TestData\\test.txt"),
                new Attachment("TestData\\test-2.txt"),
                new Attachment("TestData\\test-3.txt"),
                new Attachment("TestData\\test-4.txt")
            };
            var skipped = new[]
            {
                new Attachment("TestData\\test-3.txt"),
                new Attachment("TestData\\test-4.txt")
            };
            _selectFileSource.GetEnumerator().Returns(items.Cast<Attachment>().GetEnumerator());

            var subject = new ProtonVPN.BugReporting.Attachments.Attachments(_logger, _appConfig, _logFileSource, _selectFileSource);
            AttachmentErrorEventArgs error = null;
            subject.OnErrorOccured += (sender, attachmentError) => error = attachmentError;
            // Act
            subject.SelectFiles();
            // Assert
            error.Attachments.Should().BeEquivalentTo(skipped);
            error.Attachments.TooMany().Should().BeEquivalentTo(skipped);
        }

        [TestMethod]
        public void SelectFiles_ShouldRaise_OnErrorOccured_WithTooLargeItems()
        {
            // Arrange
            const long maxSize = 50;
            _appConfig.ReportBugMaxFileSize = maxSize;
            var items = new[]
            {
                new Attachment("TestData\\test.txt"),
                new Attachment("TestData\\test-2.txt"),
                new Attachment("TestData\\test-3.txt")
            };
            var tooLarge = new[]
            {
                new Attachment("TestData\\test-2.txt")
            };
            _selectFileSource.GetEnumerator().Returns(items.Cast<Attachment>().GetEnumerator());

            var subject = new ProtonVPN.BugReporting.Attachments.Attachments(_logger, _appConfig, _logFileSource, _selectFileSource);
            AttachmentErrorEventArgs error = null;
            subject.OnErrorOccured += (sender, attachmentError) => error = attachmentError;
            // Act
            subject.SelectFiles();
            // Assert
            error.Attachments.Should().BeEquivalentTo(tooLarge);
            error.Attachments.TooLarge().Should().BeEquivalentTo(tooLarge);
        }

        [TestMethod]
        public void SelectFiles_ShouldRaise_OnErrorOccured_WhenAttachments_HaveMaxCount()
        {
            // Arrange
            const int maxItems = 2;
            _appConfig.ReportBugMaxFiles = maxItems;
            var logItems = new[]
            {
                new Attachment("TestData\\test.txt"),
                new Attachment("TestData\\test-2.txt")
            };
            var newItems = new[]
            {
                new Attachment("TestData\\test-3.txt"),
                new Attachment("TestData\\test-4.txt")
            };
            _logFileSource.GetEnumerator().Returns(logItems.Cast<Attachment>().GetEnumerator());
            _selectFileSource.GetEnumerator().Returns(newItems.Cast<Attachment>().GetEnumerator());

            var subject = new ProtonVPN.BugReporting.Attachments.Attachments(_logger, _appConfig, _logFileSource, _selectFileSource);
            subject.Load();
            AttachmentErrorEventArgs error = null;
            subject.OnErrorOccured += (sender, attachmentError) => error = attachmentError;
            // Act
            subject.SelectFiles();
            // Assert
            error.Should().NotBeNull();
        }

        [TestMethod]
        public void Remove_ShouldRemoveItem()
        {
            // Arrange
            var items = new[]
            {
                new Attachment("TestData\\test.txt"),
                new Attachment("TestData\\test-2.txt"),
                new Attachment("TestData\\test-3.txt")
            };
            var expected = new[]
            {
                new Attachment("TestData\\test.txt"),
                new Attachment("TestData\\test-3.txt")
            };
            _logFileSource.GetEnumerator().Returns(items.Cast<Attachment>().GetEnumerator());
            var subject = new ProtonVPN.BugReporting.Attachments.Attachments(_logger, _appConfig, _logFileSource, _selectFileSource);

            subject.Load();
            subject.Items.Should().BeEquivalentTo(items);
            // Act
            subject.Remove(new Attachment("TestData\\test-2.txt"));
            // Assert
            subject.Items.Should().BeEquivalentTo(expected);
        }
    }
}
