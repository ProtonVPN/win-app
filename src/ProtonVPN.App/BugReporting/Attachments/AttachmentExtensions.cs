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
using System.Linq;

namespace ProtonVPN.BugReporting.Attachments
{
    internal static class AttachmentExtensions
    {
        public static bool HasError(this Attachment attachment) =>
             attachment.ErrorType != AttachmentErrorType.None;

        public static IEnumerable<Attachment> WithError(this IEnumerable<Attachment> items) =>
            items.Where(i => i.ErrorType != AttachmentErrorType.None);

        public static IEnumerable<Attachment> WithoutError(this IEnumerable<Attachment> items) =>
            items.Where(i => i.ErrorType == AttachmentErrorType.None);

        public static IEnumerable<Attachment> TooMany(this IEnumerable<Attachment> items) => 
            items.Where(i => i.ErrorType == AttachmentErrorType.TooManyFiles);

        public static IEnumerable<Attachment> TooLarge(this IEnumerable<Attachment> items) =>
            items.Where(i => i.ErrorType == AttachmentErrorType.FileTooLarge);

        public static IEnumerable<Attachment> FailedToRead(this IEnumerable<Attachment> items) =>
            items.Where(i => i.ErrorType == AttachmentErrorType.FileReadError);
    }
}
