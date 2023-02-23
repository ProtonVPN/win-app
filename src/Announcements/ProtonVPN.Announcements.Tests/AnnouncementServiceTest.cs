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
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Announcements.Contracts;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Announcements.Tests
{
    [TestClass]
    public class AnnouncementServiceTest
    {
        private IConfiguration _configuration;
        private IAppSettings _appSettings;
        private IScheduler _scheduler;
        private IApiClient _apiClient;
        private ILogger _logger;
        private IFileDownloadHttpClientFactory _fileDownloadHttpClientFactory;
        private IAnnouncementCache _announcementCache;

        [TestInitialize]
        public void TestInitialize()
        {
            _configuration = Substitute.For<IConfiguration>();
            _appSettings = Substitute.For<IAppSettings>();
            _scheduler = Substitute.For<IScheduler>();
            _apiClient = Substitute.For<IApiClient>();
            _logger = Substitute.For<ILogger>();
            _fileDownloadHttpClientFactory = Substitute.For<IFileDownloadHttpClientFactory>();
            _announcementCache = new AnnouncementCacheMock();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _configuration = null;
            _appSettings = null;
            _scheduler = null;
            _apiClient = null;
            _logger = null;
            _fileDownloadHttpClientFactory = null;
            _announcementCache = null;
        }

        [TestMethod]
        public void ItShouldDeleteAnnouncement()
        {
            // Arrange
            const string idToDelete = "1";
            IReadOnlyList<Announcement> announcements = GetAnnouncements();
            _announcementCache.Store(announcements);
            _configuration.AnnouncementUpdateInterval.Returns(TimeSpan.FromMinutes(10));
            AnnouncementService sut = GetAnnouncementService();

            // Act
            sut.Delete(idToDelete);
            IReadOnlyCollection<Announcement> result = sut.Get();

            // Assert
            result.Count.Should().Be(announcements.Count - 1);
            result.Should().NotContain(announcement => announcement.Id == idToDelete);
        }

        [TestMethod]
        public void ItShouldMarkAnnouncementAsSeen()
        {
            // Arrange
            const string idToMarkAsSeen = "3";
            IReadOnlyList<Announcement> announcements = GetAnnouncements();
            _announcementCache.Store(announcements);
            AnnouncementService sut = GetAnnouncementService();

            // Act
            sut.MarkAsSeen(idToMarkAsSeen);
            IReadOnlyCollection<Announcement> result = sut.Get();

            // Assert
            result.FirstOrDefault(a => a.Id == idToMarkAsSeen)?.Seen.Should().BeTrue();
        }

        [TestMethod]
        public async Task ItShouldClearAnnouncementsIfFeatureIsTurnedOff()
        {
            // Arrange
            IReadOnlyList<Announcement> announcements = GetAnnouncements();
            _announcementCache.Store(announcements);
            _appSettings.FeaturePollNotificationApiEnabled = false;

            AnnouncementService sut = GetAnnouncementService();

            // Act
            await sut.Update();

            // Assert
            _announcementCache.Get().Should().BeEmpty();
        }

        private AnnouncementService GetAnnouncementService()
        {
            _configuration.AnnouncementUpdateInterval.Returns(TimeSpan.FromMinutes(10));
            return new(_appSettings, _scheduler, _apiClient, _announcementCache, _logger,
                _fileDownloadHttpClientFactory, _configuration);
        }

        private IReadOnlyList<Announcement> GetAnnouncements()
        {
            return new List<Announcement>
            {
                new()
                {
                    Id = "1",
                    Type = (int)AnnouncementType.Standard,
                    StartDateTimeUtc = DateTime.UtcNow,
                    EndDateTimeUtc = DateTime.UtcNow.AddDays(1),
                },
                new()
                {
                    Id = "2",
                    Type = (int)AnnouncementType.Standard,
                    StartDateTimeUtc = DateTime.UtcNow,
                    EndDateTimeUtc = DateTime.UtcNow.AddDays(1),
                },
                new()
                {
                    Id = "3",
                    Type = (int)AnnouncementType.OneTime,
                    StartDateTimeUtc = DateTime.UtcNow,
                    EndDateTimeUtc = DateTime.UtcNow.AddDays(1),
                },
                new()
                {
                    Id = "4",
                    Type = (int)AnnouncementType.OneTime,
                    StartDateTimeUtc = DateTime.UtcNow,
                    EndDateTimeUtc = DateTime.UtcNow.AddDays(1),
                }
            };
        }
    }
}