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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.BugReporting.Attachments.Source;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ProtonVPN.App.Test.BugReporting.Attachments.Source
{
    [TestClass]
    [DeploymentItem("BugReporting\\Attachments\\TestData", "TestData")]
    [SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed")]
    public class FilesToAttachmentsTest
    {
        [TestMethod]
        public void Enumerable_ShouldBe_Empty_WhenSource_IsEmpty()
        {
            // Arrange
            var source = Enumerable.Empty<string>();
            var attachments = new FilesToAttachments(source);
            // Act
            var result = attachments.ToList();
            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void Enumerable_ItemPath_ShouldBe_Source()
        {
            // Arrange
            var source = new[]
            {
                "TestData\\test.txt",
                "TestData\\test-2.txt",
                "TestData\\test-3.txt"
            };
            var attachments = new FilesToAttachments(source);
            // Act
            var result = attachments.ToList();
            // assert
            result.Select(a => a.Path).Should().BeEquivalentTo(source);
        }
    }
}
