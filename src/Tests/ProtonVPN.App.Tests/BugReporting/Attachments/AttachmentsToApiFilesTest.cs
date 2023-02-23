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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.BugReporting.Attachments;
using File = ProtonVPN.Api.Contracts.File;

namespace ProtonVPN.App.Tests.BugReporting.Attachments
{
    [TestClass]
    [SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed")]
    public class AttachmentsToApiFilesTest
    {
        private IEnumerable<Attachment> _source;

        [TestInitialize]
        public void TestInitialize()
        {
            _source = Substitute.For<IEnumerable<Attachment>>();
        }

        [TestMethod]
        public void Enumerable_ShouldBeEmpty_WhenSource_IsEmpty()
        {
            // Arrange
            _source.GetEnumerator().Returns(Enumerable.Empty<Attachment>().GetEnumerator());
            AttachmentsToApiFiles converter = new AttachmentsToApiFiles(_source);
            // Act
            List<File> result = converter.ToList();
            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void Enumerable_ItemName_ShouldBe_SourceName()
        {
            // Arrange
            string[] fileNames = { "bug-report-test.txt", "bug-report-test-2.txt", "bug-report-test-3.txt" };
            IEnumerable<Attachment> attachments = fileNames.Select(f => new Attachment($"TestData\\{f}"));
            _source.GetEnumerator().Returns(attachments.GetEnumerator());
            AttachmentsToApiFiles converter = new AttachmentsToApiFiles(_source);
            // Act
            List<File> result = converter.ToList();
            // Assert
            result.Select(i => i.Name).Should().BeEquivalentTo(fileNames);
        }

        [TestMethod]
        public void Enumerable_ItemContent_ShouldBe_BinaryFileContent()
        {
            // Arrange
            Attachment[] attachments = { new("TestData\\bug-report-test-3.txt") };
            _source.GetEnumerator().Returns(attachments.Cast<Attachment>().GetEnumerator());
            byte[] expected = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes("bug-report-test-3")).ToArray();
            AttachmentsToApiFiles converter = new(_source);
            // Act
            List<File> result = converter.ToList();
            // Assert
            result.First().Content.Should().Equal(expected);
        }

        [TestMethod]
        public void Enumerable_ShouldThrow_WhenDirectory_NotExists()
        {
            // Arrange
            Attachment[] attachments = { new("Lost folder\\test.txt") };
            _source.GetEnumerator().Returns(attachments.Cast<Attachment>().GetEnumerator());
            AttachmentsToApiFiles converter = new AttachmentsToApiFiles(_source);
            // Act
            Action action = () => converter.ToList();
            // Assert
            action.Should().Throw<DirectoryNotFoundException>();
        }

        [TestMethod]
        public void Enumerable_ShouldThrow_WhenFile_NotExists()
        {
            // Arrange
            Attachment[] attachments = { new Attachment("TestData\\not-a-test.txt") };
            _source.GetEnumerator().Returns(attachments.Cast<Attachment>().GetEnumerator());
            AttachmentsToApiFiles converter = new AttachmentsToApiFiles(_source);
            // Act
            Action action = () => converter.ToList();
            // Assert
            action.Should().Throw<FileNotFoundException>();
        }
    }
}