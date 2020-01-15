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
    [SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed")]
    public class TooLargeAttachmentFilterTest
    {
        [TestMethod]
        public void Enumerable_ShouldBe_Empty_WhenSource_IsEmpty()
        {
            // Arrange
            var source = Enumerable.Empty<Attachment>();
            var filter = new TooLargeAttachmentFilter(234, source);
            // Act
            var result = filter.ToList();
            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void Enumerable_ShouldMark_TooLarge_SourceItems()
        {
            // Arrange
            const long maxSize = 1000;
            var source = new[]
            {
                new Attachment("A").WithLength(0),
                new Attachment("B").WithLength(maxSize - 1),
                new Attachment("C").WithLength(maxSize),
                new Attachment("D").WithLength(maxSize + 1),
                new Attachment("E").WithLength(long.MaxValue),
                new Attachment("F").WithLength(-1),
            };
            var expected = new[]
            {
                new Attachment("D"),
                new Attachment("E")
            };
            var filter = new TooLargeAttachmentFilter(maxSize, source);
            // Act
            var result = filter.ToList();
            // Assert
            result.TooLarge().Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void Enumerable_ShouldKeep_SourceItems_WithError()
        {
            // Arrange
            const long maxSize = 3753;
            var source = new[]
            {
                new Attachment("B").WithLength(maxSize).WithError(AttachmentErrorType.FileReadError),
                new Attachment("C").WithLength(maxSize + 1).WithError(AttachmentErrorType.TooManyFiles),
            };
            var filter = new TooLargeAttachmentFilter(maxSize, source);
            // Act
            var result = filter.ToList();
            // Assert
            result
                .WithError()
                .Where(i => i.ErrorType != AttachmentErrorType.FileTooLarge)
                .Should().BeEquivalentTo(source);
        }
    }
}
