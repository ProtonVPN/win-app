/*
 * Copyright (c) 2022 Proton Technologies AG
 *
 * This file is part of ProtonVPN.
 *
 * ProtonVPN is free software:
 you can redistribute it and/or modify
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
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Core.Announcements;
using ProtonVPN.Core.Settings;
using RichardSzalay.MockHttp;

namespace ProtonVPN.IntegrationTests.Announcements
{
    [TestClass]
    public class AnnouncementServiceTest : TestBase
    {
        [TestMethod]
        public async Task ItShouldStoreAnnouncementsToCache()
        {
            // Arrange
            InitializeAnnouncementMock("AnnouncementsResponseMock");

            // Act
            await Resolve<IAnnouncementService>().Update();

            // Assert
            AssertAnnouncement();
        }

        [TestMethod]
        public async Task ItShouldDisplayAnnouncementOnlyIfTheOfferFieldIsNotMissing()
        {
            // Arrange
            InitializeAnnouncementMock("MissingOfferAnnouncementsResponseMock");

            // Act
            await Resolve<IAnnouncementService>().Update();

            // Assert
            AssertAnnouncement();
        }

        [TestMethod]
        public async Task ItShouldNotDisplayAnnouncementIfTheDataIsPartiallyCorrupt()
        {
            // Arrange
            InitializeAnnouncementMock("InvalidDataAnnouncementsResponseMock");

            // Act
            await Resolve<IAnnouncementService>().Update();

            // Assert
            AssertAnnouncement();
        }

        private void InitializeAnnouncementMock(string nameOfMock)
        {
            MessageHandler.When(HttpMethod.Get, "/core/v4/notifications").Respond(_ => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(GetJsonMock(nameOfMock))
            });
            InitializeContainer();
            Resolve<IAppSettings>().FeaturePollNotificationApiEnabled = true;
        }

        private void AssertAnnouncement()
        {
            IReadOnlyList<Announcement> announcements = Resolve<IAnnouncementCache>().Get();
            announcements.Count.Should().Be(1);
            Announcement announcement = announcements[0];
            announcement.Seen.Should().BeFalse();
            announcement.Label.Should().Be("VPN Summer Survey");
            announcement.Panel.Pill.Should().Be("Win a $50 gift card!");
            announcement.Panel.Title.Should().Be("VPN Summer Survey");
            announcement.Panel.Button.Text.Should().Be("Take the survey");
        }
    }
}