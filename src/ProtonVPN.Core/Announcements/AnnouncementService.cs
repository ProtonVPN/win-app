/*
 * Copyright (c) 2022 Proton Technologies AG
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
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.Api;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.User;
using ApiAnnouncement = ProtonVPN.Core.Api.Contracts.Announcement;

namespace ProtonVPN.Core.Announcements
{
    public class AnnouncementService : IAnnouncementService, ILoggedInAware, ILogoutAware, ISettingsAware, IVpnPlanAware
    {
        private const string INCENTIVE_PRICE_TAG = "%IncentivePrice%";

        private readonly IAppSettings _appSettings;
        private readonly IApiClient _apiClient;
        private readonly IAnnouncementCache _announcementCache;
        private readonly ISchedulerTimer _timer;
        private readonly SingleAction _updateAction;
        private readonly ILogger _logger;

        public AnnouncementService(
            IAppSettings appSettings,
            IScheduler scheduler,
            IApiClient apiClient,
            IAnnouncementCache announcementCache,
            ILogger logger,
            TimeSpan updateInterval)
        {
            _appSettings = appSettings;
            _announcementCache = announcementCache;
            _apiClient = apiClient;
            _logger = logger;
            _timer = scheduler.Timer();
            _timer.Interval = updateInterval.RandomizedWithDeviation(0.2);
            _timer.Tick += Timer_OnTick;
            _updateAction = new(Fetch);
        }

        public event EventHandler AnnouncementsChanged;

        public IReadOnlyCollection<Announcement> Get()
        {
            return _announcementCache.Get();
        }

        public async Task Update()
        {
            await _updateAction.Run();
        }

        public void Delete(string id)
        {
            IReadOnlyList<Announcement> oldAnnouncements = _announcementCache.Get();
            IReadOnlyList<Announcement> newAnnouncements = oldAnnouncements.Where(a => a.Id != id).ToList();

            int numOfAnnouncementsToDelete = oldAnnouncements.Count - newAnnouncements.Count;
            _logger.Info<AppLog>($"Deleting {numOfAnnouncementsToDelete} announcements with ID '{id}'.");

            _announcementCache.Store(newAnnouncements);

            AnnouncementsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void MarkAsSeen(string id)
        {
            IReadOnlyList<Announcement> announcements = _announcementCache.Get();
            foreach (Announcement announcement in announcements)
            {
                if (announcement.Id == id)
                {
                    announcement.Seen = true;
                }
            }

            _announcementCache.Store(announcements);

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
            if (!_appSettings.FeaturePollNotificationApiEnabled)
            {
                ClearCache();
                return;
            }

            try
            {
                ApiResponseResult<AnnouncementsResponse> response = await _apiClient.GetAnnouncementsAsync();
                if (response.Success)
                {
                    IReadOnlyList<Announcement> announcements = Map(response.Value.Announcements);
                    _announcementCache.Store(announcements);

                    AnnouncementsChanged?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (HttpRequestException)
            {
                // Ignored
            }
        }

        private void ClearCache()
        {
            if (_announcementCache.Get().Count == 0)
            {
                return;
            }

            _announcementCache.Store(new List<Announcement>());
            AnnouncementsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void Timer_OnTick(object sender, EventArgs eventArgs)
        {
            _updateAction.Run();
        }

        private IReadOnlyList<Announcement> Map(IList<ApiAnnouncement> announcements)
        {
            return announcements
                .Where(apiAnnouncement => apiAnnouncement?.Offer != null)
                .Select(MapAnnouncement)
                .Where(announcement => announcement != null)
                .ToList();
        }

        private Announcement MapAnnouncement(ApiAnnouncement announcement)
        {
            Announcement result;
            try
            {
                result = new()
                {
                    Id = announcement.Id,
                    StartDateTimeUtc = MapTimestampToDateTimeUtc(announcement.StartTimestamp),
                    EndDateTimeUtc = MapTimestampToDateTimeUtc(announcement.EndTimestamp),
                    Url = announcement.Offer.Url,
                    Icon = announcement.Offer.Icon,
                    Label = announcement.Offer.Label,
                    Panel = MapPanel(announcement.Offer.Panel),
                    Seen = false
                };
            }
            catch (Exception e)
            {
                _logger.Error<AppLog>("Failed to map announcement.", e);
                result = null;
            }

            return result;
        }

        private DateTime MapTimestampToDateTimeUtc(long timestamp)
        {
            return DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;
        }

        private Panel MapPanel(OfferPanel offerPanel)
        {
            string incentive = offerPanel.Incentive;
            string incentiveSuffix = null;
            int indexOfIncentivePriceTag = offerPanel.Incentive.IndexOf(INCENTIVE_PRICE_TAG, StringComparison.InvariantCulture);
            if (indexOfIncentivePriceTag >= 0)
            {
                incentive = offerPanel.Incentive.Substring(0, indexOfIncentivePriceTag).TrimEnd();
                incentiveSuffix = offerPanel.Incentive.Substring(indexOfIncentivePriceTag + INCENTIVE_PRICE_TAG.Length).TrimStart();
            }
            return new()
            {
                Incentive = incentive,
                IncentivePrice = offerPanel.IncentivePrice,
                IncentiveSuffix = incentiveSuffix,
                Pill = offerPanel.Pill,
                PictureUrl = offerPanel.PictureUrl,
                Title = offerPanel.Title,
                Features = MapFeatures(offerPanel.Features),
                FeaturesFooter = offerPanel.FeaturesFooter,
                Button = MapButton(offerPanel.Button),
                PageFooter = offerPanel.PageFooter
            };
        }

        private IList<PanelFeature> MapFeatures(IList<OfferPanelFeature> offerPanelFeatures)
        {
            return offerPanelFeatures.Select(MapFeature).ToList();
        }

        private PanelFeature MapFeature(OfferPanelFeature offerPanelFeature)
        {
            return new()
            {
                IconUrl = offerPanelFeature.IconUrl,
                Text = offerPanelFeature.Text
            };
        }

        private PanelButton MapButton(OfferPanelButton offerPanelButton)
        {
            return new()
            {
                Url = offerPanelButton.Url,
                Text = offerPanelButton.Text
            };
        }

        public async void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(IAppSettings.Language)))
            {
                await _updateAction.Run();
            }
        }

        public async Task OnVpnPlanChangedAsync(VpnPlanChangedEventArgs e)
        {
            await _updateAction.Run();
        }
    }
}
