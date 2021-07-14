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
using System.Windows.Input;
using Caliburn.Micro;
using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.Common.OS.Processes;
using ProtonVPN.Config.Url;
using ProtonVPN.Core.Announcements;
using ProtonVPN.Core.Auth;

namespace ProtonVPN.Sidebar.Announcements
{
    public class AnnouncementsViewModel : Screen, IAnnouncementsAware, ILoggedInAware
    {
        private readonly IAnnouncements _announcements;
        private readonly IOsProcesses _processes;

        public AnnouncementsViewModel(IAnnouncements announcements, IOsProcesses processes)
        {
            _processes = processes;
            _announcements = announcements;
            OpenAnnouncementCommand = new RelayCommand<Announcement>(OpenAnnouncement);
        }

        private List<Announcement> _items = new List<Announcement>();

        public List<Announcement> Items
        {
            get => _items;
            private set => Set(ref _items, value);
        }

        public ICommand OpenAnnouncementCommand { get; }

        private bool _showAnnouncements;

        public bool ShowAnnouncements
        {
            get => _showAnnouncements;
            set => Set(ref _showAnnouncements, value);
        }

        private bool _hasUnread;

        public bool HasUnread
        {
            get => _hasUnread;
            private set => Set(ref _hasUnread, value);
        }

        private bool _hasAnnouncements;

        public bool HasAnnouncements
        {
            get => _hasAnnouncements;
            private set => Set(ref _hasAnnouncements, value);
        }

        public void OnAnnouncementsChanged()
        {
            Load();
        }

        public void OnUserLoggedIn()
        {
            Load();
        }

        private void Load()
        {
            HasUnread = false;
            var items = new List<Announcement>();
            foreach (var announcement in _announcements.Get())
            {
                if (!announcement.Seen)
                {
                    HasUnread = true;
                }

                items.Add(Map(announcement));
            }

            Items = items;
            HasAnnouncements = items.Count > 0;
        }

        private void OpenAnnouncement(Announcement announcement)
        {
            var url = new ActiveUrl(_processes, announcement.Url)
                .WithQueryParams(new Dictionary<string, string> {{"utm_source", "windowsvpn"}});
            url.Open();
            _announcements.MarkAsSeen(announcement.Id);
        }

        private Announcement Map(AnnouncementItem item)
        {
            return new Announcement
            {
                Id = item.Id,
                Label = item.Label,
                Url = item.Url,
                Icon = item.Icon,
                Seen = item.Seen
            };
        }
    }
}
