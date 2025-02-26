/*
 * Copyright (c) 2025 Proton AG
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

using System.Net;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Client.Logic.Announcements;
using ProtonVPN.Client.Logic.Announcements.Contracts;
using ProtonVPN.Client.Logic.Announcements.Contracts.Entities;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Integration.Tests;
using RichardSzalay.MockHttp;

namespace ProtonVPN.Integration.Tests.Announcements;

[TestClass]
public class AnnouncementsProviderTest : AuthenticatedUserTests
{
    private const string FULL_SCREEN_IMAGE_PATH = "https://protonvpn.com/full-screen-image.png";

    private IAnnouncementsProvider? _announcementsProvider;

    [TestInitialize]
    public async Task InitializeAsync()
    {
        InitializeContainer();

        _announcementsProvider = Resolve<IAnnouncementsProvider>();

        SetApiResponsesForAuth();
        await MakeUserAuthAsync(CORRECT_PASSWORD);
    }

    [TestCleanup]
    public override void Cleanup()
    {
        ClearImages();

        foreach (Announcement announcement in _announcementsProvider!.GetAllActive())
        {
            _announcementsProvider!.Delete(announcement.Id);
        }

        base.Cleanup();
    }

    [TestMethod]
    public async Task ItShouldStoreAnnouncementsToCacheAsync()
    {
        // Arrange
        InitializeAnnouncementMock("AnnouncementsResponseMock");

        // Act
        await Resolve<IAnnouncementsUpdater>().UpdateAsync();

        // Assert
        AssertAnnouncement();
    }

    [TestMethod]
    public async Task ItShouldDisplayAnnouncementOnlyIfTheOfferFieldIsNotMissingAsync()
    {
        // Arrange
        InitializeAnnouncementMock("MissingOfferAnnouncementsResponseMock");

        // Act
        await Resolve<IAnnouncementsUpdater>().UpdateAsync();

        // Assert
        AssertAnnouncement();
    }

    [TestMethod]
    public async Task ItShouldNotDisplayAnnouncementIfTheDataIsPartiallyCorruptAsync()
    {
        // Arrange
        InitializeAnnouncementMock("InvalidDataAnnouncementsResponseMock");

        // Act
        await Resolve<IAnnouncementsUpdater>().UpdateAsync();

        // Assert
        AssertAnnouncement();
    }

    [TestMethod]
    public async Task ItShouldDownloadFullScreenImageAsync()
    {
        // Arrange
        InitializeFullScreenImageMock();

        // Act
        await Resolve<IAnnouncementsUpdater>().UpdateAsync();

        // Assert
        IReadOnlyList<Announcement> announcements = Resolve<IAnnouncementsProvider>().GetAllActive();
        File.Exists(announcements[0].Panel?.FullScreenImage?.Image?.LocalPath).Should().BeTrue();
    }

    [TestMethod]
    public async Task ItShouldNotDownloadFullScreenImageIfItAlreadyExistsAsync()
    {
        // Arrange
        MockedRequest request = InitializeFullScreenImageMock();

        // Act
        await Resolve<IAnnouncementsUpdater>().UpdateAsync();
        await Resolve<IAnnouncementsUpdater>().UpdateAsync();

        // Assert
        MessageHandler!.GetMatchCount(request).Should().Be(1);
    }

    [TestMethod]
    public async Task ItShouldIgnoreAnnouncementIfFailedToDownloadFullScreenImageAsync()
    {
        // Arrange
        MessageHandler!.When(HttpMethod.Get, FULL_SCREEN_IMAGE_PATH).Throw(new HttpRequestException());
        InitializeAnnouncementMock("FullScreenImageAnnouncementResponseMock");

        // Act
        await Resolve<IAnnouncementsUpdater>().UpdateAsync();

        // Assert
        Resolve<IAnnouncementsProvider>().GetAllActive().Count.Should().Be(0);
    }

    [TestMethod]
    public async Task ItShouldIgnoreUnsupportedFullscreenImageFormatsAsync()
    {
        // Arrange
        InitializeAnnouncementMock("UnsupportedFullScreenImageMock");

        // Act
        await Resolve<IAnnouncementsUpdater>().UpdateAsync();

        // Assert
        Resolve<IAnnouncementsProvider>().GetAllActive().Count.Should().Be(0);
    }

    private MockedRequest InitializeFullScreenImageMock()
    {
        MockedRequest mockedRequest = MessageHandler!.When(HttpMethod.Get, FULL_SCREEN_IMAGE_PATH)
            .Respond(_ => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(string.Empty)
                {
                    Headers =
                    {
                        ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png")
                    }
                }
            });

        InitializeAnnouncementMock("FullScreenImageAnnouncementResponseMock");
        return mockedRequest;
    }

    private void InitializeAnnouncementMock(string nameOfMock)
    {
        MessageHandler!.When(HttpMethod.Get, "/core/v4/notifications").Respond(_ => new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(GetJsonMock(nameOfMock))
        });

        // TODO: no loner needed?
        //Resolve<ISettings>().FeaturePollNotificationApiEnabled = true;
    }

    private void ClearImages()
    {
        string imageCacheFolder = Resolve<IConfiguration>().ImageCacheFolder;
        if (Directory.Exists(imageCacheFolder))
        {
            Directory.Delete(imageCacheFolder, true);
        }
    }

    private void AssertAnnouncement()
    {
        IReadOnlyList<Announcement> announcements = Resolve<IAnnouncementsProvider>().GetAllActive();
        announcements.Count.Should().Be(1);
        Announcement announcement = announcements[0];
        announcement.Seen.Should().BeFalse();
        announcement.Label.Should().Be("VPN Summer Survey");
        // TODO: check if Pill should still exist
        //announcement.Panel.Pill.Should().Be("Win a $50 gift card!");
        announcement.Panel?.Title.Should().Be("VPN Summer Survey");
        announcement.Panel?.Button?.Text.Should().Be("Take the survey");
    }
}