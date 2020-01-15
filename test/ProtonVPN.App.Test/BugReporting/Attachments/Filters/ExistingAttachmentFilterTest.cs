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
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ProtonVPN.App.Test.BugReporting.Attachments.Filters
{
    [TestClass]
    [SuppressMessage("ReSharper", "CoVariantArrayConversion")]
    public class ExistingAttachmentFilterTest
    {
        [TestMethod]
        public void Enumerable_ShouldBeEmpty_WhenSource_IsEmpty()
        {
            // Arrange
            var existing = new[] { new Attachment("A"), new Attachment("B"), new Attachment("C") };
            var filter = new ExistingAttachmentFilter(existing, Enumerable.Empty<Attachment>());
            // Act
            var result = filter.ToList();
            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void Enumerable_ShouldBeEmpty_WhenSource_HasOnlyExistingItems()
        {
            // Arrange
            var existing = new[] { new Attachment("A"), new Attachment("B"), new Attachment("C") };
            var items = new[] { new Attachment("B"), new Attachment("C") };
            var filter = new ExistingAttachmentFilter(existing, items);
            // Act
            var result = filter.ToList();
            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void Enumerable_ShouldBe_SourceItems_ExceptExisting()
        {
            // Arrange
            var existing = new[] { new Attachment("C"), new Attachment("A"), new Attachment("D") };
            var items = new[] { new Attachment("A"), new Attachment("B"), new Attachment("C") };
            var expected = new[] { new Attachment("B") };
            var filter = new ExistingAttachmentFilter(existing, items);
            // Act
            var result = filter.ToList();
            // Assert
            result.Should().BeEquivalentTo(expected);
        }
    }
}
