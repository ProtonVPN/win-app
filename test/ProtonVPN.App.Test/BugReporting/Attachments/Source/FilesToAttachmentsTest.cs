/*
 * Copyright (c) 2022 Proton Technologies AG
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
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.BugReporting.Attachments.Source;
using System.Linq;
using ProtonVPN.BugReporting.Attachments;

namespace ProtonVPN.App.Test.BugReporting.Attachments.Source
{
    [TestClass]
    public class FilesToAttachmentsTest
    {
        [TestMethod]
        public void Enumerable_ShouldBe_Empty_WhenSource_IsEmpty()
        {
            // Arrange
            IEnumerable<string> source = Enumerable.Empty<string>();
            FilesToAttachments attachments = new(source);
            // Act
            List<Attachment> result = attachments.ToList();
            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void Enumerable_ItemPath_ShouldBe_Source()
        {
            // Arrange
            string[] source = {
                "TestData\\bug-report-test.txt",
                "TestData\\bug-report-test-2.txt",
                "TestData\\bug-report-test-3.txt"
            };
            FilesToAttachments attachments = new(source);
            // Act
            List<Attachment> result = attachments.ToList();
            // assert
            result.Select(a => a.Path).Should().BeEquivalentTo(source);
        }
    }
}