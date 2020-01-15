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
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ProtonVPN.App.Test.BugReporting.Attachments
{
    [TestClass]
    [SuppressMessage("ReSharper", "CoVariantArrayConversion")]
    public class AttachmentErrorEventArgsTest
    {
        [TestMethod]
        public void Attachments_ShouldBeEmpty_WhenNewObject()
        {
            // Arrange
            var attachments = Enumerable.Empty<Attachment>();
            var error = new AttachmentErrorEventArgs(attachments);
            // Act
            var result = error.Attachments;
            // Assert
            result.Should().NotBeNull().And.BeEmpty();
        }

        [TestMethod]
        public void Attachments_ShouldBe_Attachments()
        {
            // Arrange
            var attachments = new[]
            {
                new Attachment("C:\\folder\\file 1.txt").WithError(AttachmentErrorType.TooManyFiles),
                new Attachment("C:\\folder\\file 2.txt").WithError(AttachmentErrorType.FileTooLarge),
                new Attachment("C:\\folder\\file 3.txt").WithError(AttachmentErrorType.FileReadError)
            };
            var error = new AttachmentErrorEventArgs(attachments);
            // Act
            var result = error.Attachments;
            // Assert
            result.Should().ContainInOrder(attachments);
        }
    }
}
