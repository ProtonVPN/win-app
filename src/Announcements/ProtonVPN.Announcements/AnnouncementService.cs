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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ProtonVPN.Announcements.Contracts;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Announcements;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;
using ProtonVPN.Common.OS.Net.Http;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Users;

namespace ProtonVPN.Announcements
{
    public class AnnouncementService : IAnnouncementService, ILoggedInAware, ILogoutAware, ISettingsAware, IVpnPlanAware
    {
        public const string SUPPORTED_IMAGE_FORMAT = "png";
        private const string INCENTIVE_PRICE_TAG = "%IncentivePrice%";

        private readonly IAppSettings _appSettings;
        private readonly IApiClient _apiClient;
        private readonly IAnnouncementCache _announcementCache;
        private readonly ISchedulerTimer _timer;
        private readonly SingleAction _updateAction;
        private readonly ILogger _logger;
        private readonly IHttpClient _httpClient;
        private readonly IConfiguration _config;

        private bool _isUserLoggedIn;

        public AnnouncementService(
            IAppSettings appSettings,
            IScheduler scheduler,
            IApiClient apiClient,
            IAnnouncementCache announcementCache,
            ILogger logger,
            IFileDownloadHttpClientFactory fileDownloadHttpClientFactory,
            IConfiguration config)
        {
            _appSettings = appSettings;
            _announcementCache = announcementCache;
            _apiClient = apiClient;
            _logger = logger;
            _httpClient = fileDownloadHttpClientFactory.GetFileDownloadHttpClient();
            _config = config;
            _timer = scheduler.Timer();
            _timer.Interval = config.AnnouncementUpdateInterval.RandomizedWithDeviation(0.2);
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
            _isUserLoggedIn = true;
            _timer.Start();
        }

        public void OnUserLoggedOut()
        {
            _isUserLoggedIn = false;
            _timer.Stop();
        }

        public async void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            if (_isUserLoggedIn && e.PropertyName.Equals(nameof(IAppSettings.Language)))
            {
                await _updateAction.Run();
            }
        }

        public async Task OnVpnPlanChangedAsync(VpnPlanChangedEventArgs e)
        {
            await _updateAction.Run();
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
                AnnouncementsRequest request = new AnnouncementsRequest
                {
                    FullScreenImageWidth = 1024,
                    FullScreenImageHeight = 768,
                    FullScreenImageSupport = SUPPORTED_IMAGE_FORMAT,
                };
                ApiResponseResult<AnnouncementsResponse> response = await _apiClient.GetAnnouncementsAsync(request);
                if (response.Success)
                {
                    IReadOnlyList<Announcement> announcements = await Map(response.Value.Announcements);
                    _announcementCache.Store(announcements);
                    AnnouncementsChanged?.Invoke(this, EventArgs.Empty);
                }
            }
            catch
            {
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

        private async Task<IReadOnlyList<Announcement>> Map(IList<AnnouncementResponse> announcementResponses)
        {
            List<Announcement> announcements = new();
            foreach (AnnouncementResponse announcementResponse in announcementResponses)
            {
                if (announcementResponse?.Offer != null)
                {
                    Announcement announcement = await MapAnnouncement(announcementResponse);
                    bool hasFailedFullscreenImage =
                        announcementResponse.Offer.Panel.FullScreenImage?.Source?.Count > 0 &&
                        announcement.Panel.FullScreenImage.Source == null;
                    if (announcement != null && !hasFailedFullscreenImage)
                    {
                        announcements.Add(announcement);
                    }
                }
            }

            return announcements;
        }

        private async Task<Announcement> MapAnnouncement(AnnouncementResponse announcementResponse)
        {
            Announcement result;
            try
            {
                result = new()
                {
                    Id = announcementResponse.Id,
                    Type = announcementResponse.Type,
                    StartDateTimeUtc = MapTimestampToDateTimeUtc(announcementResponse.StartTimestamp),
                    EndDateTimeUtc = MapTimestampToDateTimeUtc(announcementResponse.EndTimestamp),
                    Url = announcementResponse.Offer.Url,
                    Icon = announcementResponse.Offer.Icon,
                    Label = announcementResponse.Offer.Label,
                    Panel = await MapPanel(announcementResponse.Offer.Panel),
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

        private async Task<Panel> MapPanel(OfferPanelResponse offerPanelResponse)
        {
            string incentive = offerPanelResponse?.Incentive;
            string incentiveSuffix = null;
            int? indexOfIncentivePriceTag = offerPanelResponse?.Incentive?.IndexOf(INCENTIVE_PRICE_TAG, StringComparison.InvariantCulture);
            if (indexOfIncentivePriceTag is >= 0)
            {
                incentive = offerPanelResponse.Incentive.Substring(0, indexOfIncentivePriceTag.Value).TrimEnd();
                incentiveSuffix = offerPanelResponse.Incentive.Substring(indexOfIncentivePriceTag.Value + INCENTIVE_PRICE_TAG.Length).TrimStart();
            }
            return new()
            {
                Incentive = incentive,
                IncentivePrice = offerPanelResponse?.IncentivePrice,
                IncentiveSuffix = incentiveSuffix,
                Pill = offerPanelResponse?.Pill,
                PictureUrl = offerPanelResponse?.PictureUrl,
                Title = offerPanelResponse?.Title,
                Features = MapFeatures(offerPanelResponse?.Features),
                FeaturesFooter = offerPanelResponse?.FeaturesFooter,
                Button = MapButton(offerPanelResponse?.Button),
                PageFooter = offerPanelResponse?.PageFooter,
                FullScreenImage = await MapFullScreenImage(offerPanelResponse?.FullScreenImage)
            };
        }

        private async Task<FullScreenImage> MapFullScreenImage(FullScreenImageResponse response)
        {
            string url = response?.Source.FirstOrDefault(s => s.Type.EqualsIgnoringCase(SUPPORTED_IMAGE_FORMAT))?.Url;
            string source = null;

            if (url != null)
            {
                string localImagePath = await StoreImage(url);
                if (localImagePath != null)
                {
                    source = localImagePath;
                }
            }

            return new()
            {
                AlternativeText = response?.AlternativeText,
                Source = source,
            };
        }

        private IList<PanelFeature> MapFeatures(IList<OfferPanelFeatureResponse> offerPanelFeatures)
        {
            return offerPanelFeatures?.Select(MapFeature).ToList();
        }

        private PanelFeature MapFeature(OfferPanelFeatureResponse offerPanelFeatureResponse)
        {
            return new()
            {
                IconUrl = offerPanelFeatureResponse.IconUrl,
                Text = offerPanelFeatureResponse.Text
            };
        }

        private PanelButton MapButton(OfferPanelButtonResponse offerPanelButtonResponse)
        {
            return new()
            {
                Url = offerPanelButtonResponse?.Url,
                Text = offerPanelButtonResponse?.Text,
                Action = offerPanelButtonResponse?.Action,
                Behaviors = offerPanelButtonResponse?.Behaviors,
            };
        }

        private async Task<string> StoreImage(string url)
        {
            string localImagePath = GetLocalImagePath(url);
            if (System.IO.File.Exists(localImagePath))
            {
                return localImagePath;
            }

            Directory.CreateDirectory(_config.ImageCacheFolder);
            bool isImageDownloaded = await DownloadImage(url, localImagePath);
            return isImageDownloaded ? localImagePath : null;
        }

        private async Task<bool> DownloadImage(string url, string localImagePath)
        {
            try
            {
                using IHttpResponseMessage response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    using Stream contentStream = await response.Content.ReadAsStreamAsync();
                    using FileStream fileStream = new(localImagePath, FileMode.Create, FileAccess.Write, FileShare.None);
                    await contentStream.CopyToAsync(fileStream);
                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.Error<AppLog>($"Failed to download image using url {url}", e);
            }

            return false;
        }

        private string GetLocalImagePath(string url)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(url));
                StringBuilder sb = new();
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("X2"));
                }

                return Path.Combine(_config.ImageCacheFolder, sb.ToString());
            }
        }
    }
}