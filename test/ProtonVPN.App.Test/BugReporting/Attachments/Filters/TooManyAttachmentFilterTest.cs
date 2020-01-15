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
using ProtonVPN.BugReporting.Attachments.Filters;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ProtonVPN.App.Test.BugReporting.Attachments.Filters
{
    [TestClass]
    [SuppressMessage("ReSharper", "CoVariantArrayConversion")]
    [SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed")]
    public class TooManyAttachmentFilterTest
    {
        [TestMethod]
        public void Enumerable_ShouldBeEmpty_WhenSource_IsEmpty()
        {
            // Arrange
            var existing = new[]
            {
                new Attachment("A"),
                new Attachment("B"),
                new Attachment("C")
            };
            var source = Enumerable.Empty<Attachment>();
            var filter = new TooManyAttachmentFilter(existing, 3, source);
            // Act
            var result = filter.ToList();
            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void Enumerable_ShouldBe_SourceItems_WithNoError()
        {
            // Arrange
            var existing = new[]
            {
                new Attachment("A"),
                new Attachment("B")
            };
            var source = new[]
            {
                new Attachment("C"),
                new Attachment("D"),
                new Attachment("E")
            };
            var filter = new TooManyAttachmentFilter(existing, 3, source);
            // Act
            var result = filter.ToList();
            // Assert
            result.WithoutError().Should().BeEquivalentTo(source);
        }

        [TestMethod]
        public void Enumerable_ShouldBe_SourceItems_WithError_Skipped()
        {
            // Arrange
            var existing = new[]
            {
                new Attachment("A"),
                new Attachment("B"),
                new Attachment("C")
            };
            var source = new[]
            {
                new Attachment("D"),
                new Attachment("E")
            };
            var filter = new TooManyAttachmentFilter(existing, 3, source);
            // Act
            var result = filter.ToList();
            // Assert
            result.TooMany().Should().BeEquivalentTo(source);
        }

        [TestMethod]
        public void Enumerable_ShouldBe_SourceItems_WithError_WhenSkipped()
        {
            // Arrange
            var existing = new List<Attachment> { new Attachment("A") };
            var source = new[]
            {
                new Attachment("B"), 
                new Attachment("C"),
                new Attachment("D")
            };
            var expected = new[] { new Attachment("D") };
            var filter = new TooManyAttachmentFilter(existing, 3, source);
            // Act
            var result = new List<Attachment>();
            foreach (var item in filter)
            {
                existing.Add(item);
                result.Add(item);
            }
            // Assert
            result.TooMany().Should().BeEquivalentTo(expected);
        }
    }
}
