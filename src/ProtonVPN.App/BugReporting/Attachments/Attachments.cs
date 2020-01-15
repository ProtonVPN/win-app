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

using ProtonVPN.BugReporting.Attachments.Filters;
using ProtonVPN.Common.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ProtonVPN.BugReporting.Attachments
{
    public class Attachments
    {
        private readonly IEnumerable<Attachment> _logFileSource;
        private readonly IEnumerable<Attachment> _filteredItems;

        public Attachments(ILogger logger, Common.Configuration.Config appConfig, IEnumerable<Attachment> logFileSource, IEnumerable<Attachment> selectFileSource)
        {
            _logFileSource = logFileSource;

            _filteredItems = 
                new TooLargeAttachmentFilter(appConfig.ReportBugMaxFileSize,
                    new FileLengthAttachmentFilter(logger,
                        new TooManyAttachmentFilter(Items, appConfig.ReportBugMaxFiles,
                            new ExistingAttachmentFilter(Items,
                                selectFileSource))));
        }

        public EventHandler<AttachmentErrorEventArgs> OnErrorOccured;

        public void Load()
        {
            Items.Clear();
            Add(_logFileSource);
        }

        public ObservableCollection<Attachment> Items { get; } = new ObservableCollection<Attachment>();

        public void SelectFiles()
        {
            var items = new List<Attachment>();
            foreach (var item in _filteredItems)
            {
                // Items without errors should be added to Items collection during the enumeration, not after.
                // The TooManyAttachmentFilter depends on this functionality.
                AddWithoutError(item);
                items.Add(item);
            }

            var errorItems = items.WithError().ToList();
            if (errorItems.Any())
                OnErrorOccured?.Invoke(this, new AttachmentErrorEventArgs(errorItems));
        }

        public void Remove(Attachment item)
        {
            Items.Remove(item);
        }

        private void Add(IEnumerable<Attachment> items)
        {
            foreach (var item in items)
            {
                Items.Add(item);
            }
        }

        private void AddWithoutError(Attachment item)
        {
            if (!item.HasError())
                Items.Add(item);
        }
    }
}
