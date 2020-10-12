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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.Api;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Config;

namespace ProtonVPN.Core.Announcements
{
    public class AnnouncementService : IAnnouncements, ILoggedInAware, ILogoutAware
    {
        private readonly IClientConfig _clientConfig;
        private readonly IApiClient _apiClient;
        private readonly IAnnouncementCache _announcementCache;
        private readonly ISchedulerTimer _timer;
        private readonly SingleAction _updateAction;

        public AnnouncementService(
            IClientConfig clientConfig,
            IScheduler scheduler,
            IApiClient apiClient,
            IAnnouncementCache announcementCache,
            TimeSpan updateInterval)
        {
            _clientConfig = clientConfig;
            _announcementCache = announcementCache;
            _apiClient = apiClient;
            _timer = scheduler.Timer();
            _timer.Interval = updateInterval.RandomizedWithDeviation(0.2);
            _timer.Tick += Timer_OnTick;
            _updateAction = new SingleAction(Fetch);
        }

        public event EventHandler AnnouncementsChanged;

        public IReadOnlyCollection<AnnouncementItem> Get()
        {
            return _announcementCache.Get();
        }

        public async Task Update()
        {
            await _updateAction.Run();
        }

        public void MarkAsSeen(string id)
        {
            var items = _announcementCache.Get();
            foreach (var item in items)
            {
                if (item.Id == id)
                {
                    item.Seen = true;
                }
            }

            _announcementCache.Store(items);

            AnnouncementsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void OnUserLoggedIn()
        {
            _timer.Start();
        }

        public void OnUserLoggedOut()
        {
            _timer.Stop();
        }

        private async Task Fetch()
        {
            if (!_clientConfig.PollNotificationApiEnabled)
            {
                ClearCache();
                return;
            }

            try
            {
                var response = await _apiClient.GetAnnouncementsAsync();
                if (response.Success)
                {
                    _announcementCache.Store(Map(response.Value.Announcements));

                    AnnouncementsChanged?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (HttpRequestException)
            {
                // ignored
            }
        }

        private void ClearCache()
        {
            if (_announcementCache.Get().Count == 0)
            {
                return;
            }

            _announcementCache.Store(new List<AnnouncementItem>());
            AnnouncementsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void Timer_OnTick(object sender, EventArgs eventArgs)
        {
            _updateAction.Run();
        }

        private IReadOnlyList<AnnouncementItem> Map(List<Api.Contracts.Announcement> announcements)
        {
            return announcements.Where(a => a.Offer != null).Select(a =>
                    new AnnouncementItem(a.Id,
                        a.Offer.Label,
                        a.Offer.Url,
                        a.Offer.Icon,
                        false))
                .ToList();
        }
    }
}
