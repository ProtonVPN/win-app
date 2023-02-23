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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.Announcements.Contracts;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Windows.Popups;
using ProtonVPN.Windows.Popups.Offers;

namespace ProtonVPN.Sidebar.Announcements
{
    public class AnnouncementsViewModel : Screen, IAnnouncementsAware, ILoggedInAware
    {
        private readonly IAnnouncementService _announcementService;
        private readonly IPopupWindows _popupWindows;
        private readonly ILogger _logger;
        private readonly OfferPopupViewModel _offerPopupViewModel;

        public AnnouncementsViewModel(IAnnouncementService announcementService,
            IPopupWindows popupWindows,
            ILogger logger,
            OfferPopupViewModel offerPopupViewModel)
        {
            _announcementService = announcementService;
            _popupWindows = popupWindows;
            _logger = logger;
            _offerPopupViewModel = offerPopupViewModel;
            OpenAnnouncementCommand = new RelayCommand(OpenAnnouncement);
        }

        private Announcement _announcement = new();
        public Announcement Announcement
        {
            get => _announcement;
            private set => Set(ref _announcement, value);
        }

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

        public ICommand OpenAnnouncementCommand { get; }

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
            List<Announcement> items = new();
            DateTime currentTimeUtc = DateTime.UtcNow;
            IReadOnlyCollection<Announcement> announcements = _announcementService.Get()
                .Where(a => a.Type == (int)AnnouncementType.Standard)
                .ToList();
            foreach (Announcement announcement in announcements)
            {
                if (currentTimeUtc >= announcement.EndDateTimeUtc)
                {
                    _announcementService.Delete(announcement.Id);
                    continue;
                }
                if (currentTimeUtc < announcement.StartDateTimeUtc)
                {
                    // Schedule future announcements, but not too far in the future because Task.Delay receives milliseconds in int, hence being limited to 24.8 days
                    if (currentTimeUtc.AddDays(3) >= announcement.StartDateTimeUtc)
                    {
                        ScheduleReload((announcement.StartDateTimeUtc - currentTimeUtc).Add(TimeSpan.FromSeconds(1)));
                    }
                    continue;
                }
                if (!announcement.Seen)
                {
                    HasUnread = true;
                }

                items.Add(announcement);
            }

            Announcement = items.FirstOrDefault();
            HasAnnouncements = items.Count > 0;
        }

        private void ScheduleReload(TimeSpan nextReloadInterval)
        {
            _logger.Info<AppLog>($"Announcement reload scheduled to be done in '{nextReloadInterval}'.");
            Task waitTask = Task.Delay((int)nextReloadInterval.TotalMilliseconds);
            waitTask.ContinueWith(_ =>
            {
                _logger.Info<AppLog>("Triggering scheduled announcement reload.");
                Load();
            });
        }

        private void OpenAnnouncement()
        {
            Announcement announcement = Announcement;
            if (announcement != null)
            {
                HasUnread = false;
                OpenAnnouncement(announcement);
            }
        }

        private void OpenAnnouncement(Announcement announcement)
        {
            _announcementService.MarkAsSeen(announcement.Id);
            _offerPopupViewModel.Panel = announcement.Panel;
            _popupWindows.Show<OfferPopupViewModel>();
        }
    }
}