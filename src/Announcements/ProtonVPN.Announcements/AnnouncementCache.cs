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

using System.Collections.Generic;
using System.Linq;
using ProtonVPN.Announcements.Contracts;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Announcements
{
    public class AnnouncementCache : IAnnouncementCache
    {
        private readonly IAppSettings _appSettings;

        public AnnouncementCache(IAppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public IReadOnlyList<Announcement> Get()
        {
            return _appSettings.Announcements;
        }

        public void Store(IReadOnlyList<Announcement> announcements)
        {
            foreach (Announcement announcement in announcements)
            {
                announcement.Seen = Seen(announcement.Id);
            }

            _appSettings.Announcements = announcements;
        }

        private bool Seen(string id)
        {
            return _appSettings.Announcements.Any(announcement => announcement.Id == id && announcement.Seen);
        }
    }
}