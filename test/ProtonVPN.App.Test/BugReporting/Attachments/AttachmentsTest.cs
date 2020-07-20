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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.BugReporting.Attachments;

namespace ProtonVPN.App.Test.BugReporting.Attachments
{
    [TestClass]
    [DeploymentItem("BugReporting\\Attachments\\TestData", "TestData")]
    [SuppressMessage("ReSharper", "CoVariantArrayConversion")]
    [SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed")]
    public class AttachmentsTest
    {
        private Common.Configuration.Config _appConfig;
        private IEnumerable<Attachment> _logFileSource;
        private IEnumerable<Attachment> _selectFileSource;

        [TestInitialize]
        public void TestInitialize()
        {
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
            var subject = new ProtonVPN.BugReporting.Attachments.Attachments(_logFileSource);

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
            var subject = new ProtonVPN.BugReporting.Attachments.Attachments(_logFileSource);
            // Act
            subject.Load();
            // Assert
            _logFileSource.Received(1).GetEnumerator();
        }

        [TestMethod]
        public void Load_ShouldNotEnumerate_SelectFileSource()
        {
            // Arrange
            var subject = new ProtonVPN.BugReporting.Attachments.Attachments(_logFileSource);
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
            var subject = new ProtonVPN.BugReporting.Attachments.Attachments(_logFileSource);
            // Act
            subject.Load();
            // Assert
            subject.Items.Should().BeEquivalentTo(items);
        }

        [TestMethod]
        public void Items_ShouldByEmpty_Initially()
        {
            // Arrange
            var subject = new ProtonVPN.BugReporting.Attachments.Attachments(_logFileSource);
            // Act
            var result = subject.Items;
            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void Remove_ShouldRemoveItem()
        {
            // Arrange
            var itemToRemove = new Attachment("TestData\\test-2.txt");
            var items = new[]
            {
                new Attachment("TestData\\test.txt"),
                itemToRemove,
                new Attachment("TestData\\test-3.txt")
            };
            var expected = new[]
            {
                new Attachment("TestData\\test.txt"),
                new Attachment("TestData\\test-3.txt")
            };
            _logFileSource.GetEnumerator().Returns(items.Cast<Attachment>().GetEnumerator());
            var subject = new ProtonVPN.BugReporting.Attachments.Attachments(_logFileSource);

            subject.Load();
            subject.Items.Should().BeEquivalentTo(items);

            // Act
            subject.Remove(itemToRemove);
            // Assert
            subject.Items.Should().BeEquivalentTo(expected);
        }
    }
}
