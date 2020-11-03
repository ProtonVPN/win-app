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

namespace ProtonVPN.Core.Announcements
{
    public class AnnouncementItem
    {
        public AnnouncementItem(string id, string label, string url, string icon, bool seen)
        {
            Id = id;
            Label = label;
            Url = url;
            Icon = icon;
            Seen = seen;
        }

        public string Id { get; set; }

        public string Label { get; set; }

        public string Url { get; set; }

        public string Icon { get; set; }

        public bool Seen { get; set; }
    }
}
