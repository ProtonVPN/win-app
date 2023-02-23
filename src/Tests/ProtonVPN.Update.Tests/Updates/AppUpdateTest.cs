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
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.OS.Net.Http;
using ProtonVPN.Tests.Common;
using ProtonVPN.Update.Config;
using ProtonVPN.Update.Files.Launchable;
using ProtonVPN.Update.Files.Validatable;
using ProtonVPN.Update.Updates;

// ReSharper disable ObjectCreationAsStatement

namespace ProtonVPN.Update.Tests.Updates
{
    [TestClass]
    public class AppUpdateTest
    {
        private ILogger _logger;
        private ILaunchableFile _launchableFile;
        private IHttpClient _httpClient;
        private IFeedUrlProvider _feedUrlProvider;
        private DefaultAppUpdateConfig _config;
        private IEnumerable<Uri> _feedUrls = new List<Uri>()
        {
            new Uri("http://127.0.0.1/windows-releases.json"),
            new Uri("http://127.0.0.1/win-update.json")
        };

        #region Initialization

        [TestInitialize]
        public void TestInitialize()
        {
            _logger = Substitute.For<ILogger>();
            _launchableFile = Substitute.For<ILaunchableFile>();
            _httpClient = Substitute.For<IHttpClient>();
            _feedUrlProvider = Substitute.For<IFeedUrlProvider>();
            _feedUrlProvider.GetFeedUrls().Returns(_feedUrls);
            _config = new DefaultAppUpdateConfig
            {
                HttpClient = _httpClient,
                FeedUriProvider = _feedUrlProvider,
                UpdatesPath = "Updates",
                CurrentVersion = new Version(),
                EarlyAccessCategoryName = "EarlyAccess"
            };
        }

        private IAppUpdate AppUpdate(Version version, IHttpResponseMessage httpResponse = null, [CallerMemberName] string updatesPath = null)
        {
            _config.CurrentVersion = version;
            return AppUpdate(httpResponse, updatesPath);
        }

        private IAppUpdate AppUpdate(Task<IHttpResponseMessage> httpResponse, [CallerMemberName] string updatesPath = null)
        {
            _config.UpdatesPath = TestConfig.GetFolderPath(updatesPath);
            _httpClient.GetAsync(_config.FeedUriProvider.GetFeedUrls().First()).Returns(httpResponse);
            return AppUpdate();
        }

        private IAppUpdate AppUpdate(IHttpResponseMessage httpResponse, [CallerMemberName] string updatesPath = null)
        {
            _config.UpdatesPath = TestConfig.GetFolderPath(updatesPath);
            _httpClient.GetAsync(_config.FeedUriProvider.GetFeedUrls().First()).Returns(httpResponse);
            return AppUpdate();
        }

        private IAppUpdate AppUpdate()
        {
            return new AppUpdate(new AppUpdates(_config, _launchableFile, _logger));
        }

        #endregion

        #region Test: ReleaseHistory

        [TestMethod]
        public void ReleaseHistory_ShouldBe_Empty_Initially()
        {
            IAppUpdate update = AppUpdate();

            update.ReleaseHistory().Should().BeEmpty();
        }

        [TestMethod]
        public async Task ReleaseHistory_ShouldReturn_Stable_Releases()
        {
            IAppUpdate update = AppUpdate(HttpResponseFromFile("win-update.json"));
            update = await update.Latest(false);

            IReadOnlyList<IRelease> result = update.ReleaseHistory();

            result.Should()
                .HaveCount(3).And
                .Match(r => r.All(x => !x.EarlyAccess));
        }

        [TestMethod]
        public async Task ReleaseHistory_ShouldReturn_StableAndEarlyAccess_Releases()
        {
            IAppUpdate update = AppUpdate(HttpResponseFromFile("win-update.json"));
            update = await update.Latest(true);

            IReadOnlyList<IRelease> result = update.ReleaseHistory();

            result.Should()
                .HaveCount(5).And
                .Match(r => r.Count(x => x.EarlyAccess) == 2);
        }

        [TestMethod]
        public async Task ReleaseHistory_ShouldReturn_StableAndEarlyAccess_Releases_UpToCurrentVersion()
        {
            IAppUpdate update = AppUpdate(new Version(1, 5, 2), HttpResponseFromFile("win-update.json"));
            update = await update.Latest(false);

            IReadOnlyList<IRelease> result = update.ReleaseHistory();

            result.Should()
                .HaveCount(4).And
                .Match(r => r.First().Version == _config.CurrentVersion).And
                .Match(r => r.First().EarlyAccess);
        }

        [TestMethod]
        public async Task ReleaseHistory_ShouldReturn_Releases_WithChangeLog()
        {
            IAppUpdate update = AppUpdate(HttpResponseFromFile("win-update.json"));
            update = await update.Latest(false);

            IReadOnlyList<IRelease> result = update.ReleaseHistory();

            result[0].ChangeLog.Should()
                .HaveCount(2).And
                .Match(l => l.All(x => !string.IsNullOrEmpty(x)));
        }

        [TestMethod]
        public async Task ReleaseHistory_ShouldReturn_Releases_OrderedByVersion()
        {
            IAppUpdate update = AppUpdate(HttpResponseFromFile("win-update.json"));
            update = await update.Latest(true);

            IReadOnlyList<IRelease> result = update.ReleaseHistory();
            IOrderedEnumerable<IRelease> expected = result.OrderByDescending(r => r.Version);

            result.Should().ContainInOrder(expected);
        }

        #endregion

        #region Test: Available

        [TestMethod]
        public void Available_ShouldBeFalse_Initially()
        {
            IAppUpdate update = AppUpdate();
            update.Available.Should().BeFalse();
        }

        [DataTestMethod]
        [DataRow(1, 5, 0, false, true)]
        [DataRow(1, 5, 1, false, false)]
        [DataRow(1, 5, 2, false, false)]
        [DataRow(1, 5, 0, true, true)]
        [DataRow(1, 5, 1, true, true)]
        [DataRow(2, 0, 0, true, false)]
        [DataRow(2, 0, 1, true, false)]
        public async Task Available_ShouldReflect_LatestRelease_AfterLatest(int major, int minor, int build, bool earlyAccess, bool result)
        {
            IAppUpdate update = AppUpdate(new Version(major, minor, build), HttpResponseFromFile("win-update.json"));

            update = await update.Latest(earlyAccess);

            update.Available.Should().Be(result);
        }

        [DataTestMethod]
        [DataRow(1, 5, 0, false, true)]
        [DataRow(1, 5, 1, false, false)]
        [DataRow(1, 5, 2, false, false)]
        [DataRow(1, 5, 0, true, true)]
        [DataRow(1, 5, 1, true, true)]
        [DataRow(2, 0, 0, true, false)]
        [DataRow(2, 0, 1, true, false)]
        public async Task Available_ShouldReflect_LatestRelease_AfterCachedLatest(int major, int minor, int build, bool earlyAccess, bool result)
        {
            IAppUpdate update = AppUpdate(new Version(major, minor, build), HttpResponseFromFile("win-update.json"));
            update = await update.Latest(false);

            update = update.CachedLatest(earlyAccess);

            update.Available.Should().Be(result);
        }

        [TestMethod]
        public async Task Available_ShouldBe_False_WhenLatestRelease_HasNoFile()
        {
            const string json = "{\"Categories\": [{\"Name\": \"Stable\", \"Releases\": [{\"Version\": \"2.0.0\", \"ChangeLog\": [\"line 1\"] }] }] }";
            IAppUpdate update = AppUpdate(new Version(1, 0, 0), HttpResponseFromString(json));

            update = await update.Latest(false);

            update.Available.Should().BeFalse();
        }

        [TestMethod]
        public async Task Available_ShouldBe_False_WhenLatestRelease_FileHasNoUrl()
        {
            const string json = "{\"Categories\": [{\"Name\": \"Stable\", \"Releases\": [{\"Version\": \"2.0.0\", \"ChangeLog\": [\"line 1\"], \"File\": {\"Sha512CheckSum\": \"a b c d e f g h\"}} ] }] }";
            IAppUpdate update = AppUpdate(new Version(1, 0, 0), HttpResponseFromString(json));

            update = await update.Latest(false);

            update.Available.Should().BeFalse();
        }

        [TestMethod]
        public async Task Available_ShouldBe_False_WhenLatestRelease_FileHasNoChecksum()
        {
            const string json = "{\"Categories\": [{\"Name\": \"Stable\", \"Releases\": [{\"Version\": \"2.0.0\", \"ChangeLog\": [\"line 1\"], \"File\": {\"Url\": \"https://protonvpn.com/download/ProtonVPN_win_v1.5.2.exe\"}} ] }] }";
            IAppUpdate update = AppUpdate(new Version(1, 0, 0), HttpResponseFromString(json));

            update = await update.Latest(false);

            update.Available.Should().BeFalse();
        }

        [TestMethod]
        public async Task Available_ShouldNotChange_AfterDownload_WhenItWasFalse()
        {
            IAppUpdate update = AppUpdate(new Version(1, 5, 5), HttpResponseFromFile("win-update.json"));
            update = await update.Latest(false);
            update.Available.Should().BeFalse();

            update = await update.Downloaded();

            update.Available.Should().BeFalse();
        }

        [TestMethod]
        public async Task Available_ShouldNotChange_AfterDownload_WhenItWasTrue()
        {
            const string fileUri = "https://protonvpn.com/download/ProtonVPN_win_v2.0.0.exe";
            IHttpResponseMessage httpResponse = HttpResponseFromFile("ProtonVPN_win_v2.0.0.exe");
            _httpClient.GetAsync(fileUri).Returns(httpResponse);

            IAppUpdate update = AppUpdate(new Version(1, 5, 5), HttpResponseFromFile("win-update.json"));
            update = await update.Latest(true);
            update.Available.Should().BeTrue();

            update = await update.Downloaded();

            update.Available.Should().BeTrue();
        }

        #endregion

        #region Test: Ready

        [TestMethod]
        public void Ready_ShouldBeFalse_Initially()
        {
            IAppUpdate update = AppUpdate();

            update.Ready.Should().BeFalse();
        }

        [TestMethod]
        public async Task Ready_ShouldBeFalse_AfterLatest_WhenUpdateNotAvailable()
        {
            IAppUpdate update = AppUpdate(new Version(1, 5, 5), HttpResponseFromFile("win-update.json"));
            update = await update.Latest(true);
            update.Available.Should().BeTrue();

            IHttpResponseMessage response = HttpResponseFromFile("win-update.json");
            _httpClient.GetAsync(_config.FeedUriProvider.GetFeedUrls().First()).Returns(response);

            update = await update.Latest(false);
            update.Available.Should().BeFalse();

            update.Ready.Should().BeFalse();
        }

        [TestMethod]
        public async Task Ready_ShouldBeFalse_AfterCachedLatest_WhenUpdateNotAvailable()
        {
            IAppUpdate update = AppUpdate(new Version(1, 5, 5), HttpResponseFromFile("win-update.json"));
            update = await update.Latest(true);
            update.Available.Should().BeTrue();

            update = update.CachedLatest(false);
            update.Available.Should().BeFalse();

            update.Ready.Should().BeFalse();
        }

        [TestMethod]
        public async Task Ready_ShouldNotChange_AfterDownloaded_WhenNotAvailable()
        {
            IAppUpdate update = AppUpdate(new Version(1, 5, 5), HttpResponseFromFile("win-update.json"));
            update = await update.Latest(false);
            update.Available.Should().BeFalse();

            update = await update.Downloaded();

            update.Ready.Should().BeFalse();
        }

        [TestMethod]
        public async Task Ready_ShouldNotChange_AfterDownloaded_WhenFalse()
        {
            IHttpResponseMessage httpResponse = HttpResponseFromFile("ProtonVPN_win_v2.0.0.exe");
            _httpClient.GetAsync("https://protonvpn.com/download/ProtonVPN_win_v2.0.0.exe").Returns(httpResponse);

            IAppUpdate update = AppUpdate(new Version(1, 5, 5), HttpResponseFromFile("win-update.json"));
            update = await update.Latest(true);
            update.Ready.Should().BeFalse();

            update = await update.Downloaded();

            update.Ready.Should().BeFalse();
        }

        [TestMethod]
        public async Task Ready_ShouldNotChange_AfterDownloaded_WhenTrue()
        {
            IHttpResponseMessage httpResponse = HttpResponseFromFile("ProtonVPN_win_v2.0.0.exe");
            _httpClient.GetAsync("https://protonvpn.com/download/ProtonVPN_win_v2.0.0.exe").Returns(httpResponse);

            IAppUpdate update = AppUpdate(new Version(1, 5, 5), HttpResponseFromFile("win-update.json"));
            update = await update.Latest(true);
            update = await update.Downloaded();
            update = await update.Validated();
            update.Ready.Should().BeTrue();

            update = await update.Downloaded();

            update.Ready.Should().BeTrue();
        }

        [TestMethod]
        public async Task Ready_ShouldBeTrue_AfterValidated_WhenFileAlreadyExists()
        {
            const string downloadsPath = nameof(Downloaded_ShouldDownloadFile_ToDownloadsDirectory);
            IAppUpdate update = AppUpdate(new Version(1, 2, 0), HttpResponseFromFile("win-update.json"), downloadsPath);
            update = await update.Latest(false);
            update.Available.Should().BeTrue();

            CopyFile("ProtonVPN_win_v1.5.1.exe", TestConfig.GetFolderPath(downloadsPath));
            update = await update.Validated();
            File.Delete(Path.Combine(TestConfig.GetFolderPath(downloadsPath), "ProtonVPN_win_v1.5.1.exe"));

            update.Ready.Should().BeTrue();
        }

        [TestMethod]
        public async Task Ready_ShouldBeFalse_AfterValidated_WhenFileCheckSum_IsNotValid()
        {
            IHttpResponseMessage httpResponse = HttpResponseFromFile("ProtonVPN_win_v1.0.0.exe");
            _httpClient.GetAsync("https://protonvpn.com/download/ProtonVPN_win_v1.5.1.exe").Returns(httpResponse);

            IAppUpdate update = AppUpdate(new Version(1, 2, 0), HttpResponseFromFile("win-update.json"));
            update = await update.Latest(false);
            update.Available.Should().BeTrue();

            update = await update.Downloaded();
            update = await update.Validated();

            update.Ready.Should().BeFalse();
        }

        [TestMethod]
        public async Task Ready_ShouldBeTrue_AfterValidated_WhenFileCheckSum_IsValid()
        {
            IHttpResponseMessage httpResponse = HttpResponseFromFile("ProtonVPN_win_v2.0.0.exe");
            _httpClient.GetAsync("https://protonvpn.com/download/ProtonVPN_win_v2.0.0.exe").Returns(httpResponse);

            IAppUpdate update = AppUpdate(new Version(1, 5, 1), HttpResponseFromFile("win-update.json"));
            update = await update.Latest(true);
            update.Available.Should().BeTrue();

            update = await update.Downloaded();
            update = await update.Validated();

            update.Ready.Should().BeTrue();
        }

        #endregion

        #region Test: Latest

        [TestMethod]
        public async Task Latest_ShouldGet_JsonFile_UsingFeedUri()
        {
            _feedUrlProvider.GetFeedUrls().Returns(_feedUrls);
            IAppUpdate update = AppUpdate(HttpResponseFromFile("win-update.json"));

            await update.Latest(false);

            await _httpClient.Received().GetAsync(_config.FeedUriProvider.GetFeedUrls().First());
        }

        [TestMethod]
        public void Latest_ShouldThrow_WhenHttpResponse_IsNotSuccess()
        {
            IHttpResponseMessage httpResponse = Substitute.For<IHttpResponseMessage>();
            httpResponse.IsSuccessStatusCode.Returns(false);
            IAppUpdate update = AppUpdate(httpResponse);

            Func<Task> action = () => update.Latest(false);

            action.Should().ThrowAsync<AppUpdateException>();
        }

        [TestMethod]
        public void Latest_ShouldThrow_WhenHttpResponse_IsEmpty()
        {
            IHttpResponseMessage httpResponse = HttpResponseFromFile("Empty file.txt");
            IAppUpdate update = AppUpdate(httpResponse);

            Func<Task> action = () => update.Latest(false);

            action.Should().ThrowAsync<AppUpdateException>();
        }

        [TestMethod]
        public void Latest_ShouldThrow_WhenHttpRequest_Throws()
        {
            Exception[] exceptions =
            {
                new HttpRequestException(),
                new OperationCanceledException(),
                new SocketException()
            };

            foreach (Exception exception in exceptions)
            {
                Latest_ShouldThrow_WhenHttpRequest_Throws(exception);
            }
        }

        private void Latest_ShouldThrow_WhenHttpRequest_Throws(Exception exception)
        {
            IAppUpdate update = AppUpdate(FailedHttpRequest(exception));

            Func<Task> action = () => update.Latest(false);

            action.Should().ThrowAsync<AppUpdateException>();
        }

        [TestMethod]
        public void Latest_ShouldThrow_WhenHttpRequest_Cancelled()
        {
            IAppUpdate update = AppUpdate(CancelledHttpRequest());

            Func<Task> action = () => update.Latest(false);

            action.Should().ThrowAsync<AppUpdateException>();
        }

        [TestMethod]
        public void Latest_ShouldThrow_WhenHttpResponse_IsNotJson()
        {
            IAppUpdate update = AppUpdate(HttpResponseFromString("HTTP"));

            Func<Task> action = () => update.Latest(false);

            action.Should().ThrowAsync<AppUpdateException>();
        }

        #endregion

        #region Test: CachedLatest

        [TestMethod]
        public async Task CachedLatest_ShouldNotGet_JsonFile()
        {
            IAppUpdate update = AppUpdate(HttpResponseFromFile("win-update.json"));

            update.CachedLatest(false);

            await _httpClient.DidNotReceiveWithAnyArgs().GetAsync("");
        }

        [TestMethod]
        public async Task CachedLatest_ShouldNotChange_Releases_WhenEarlyAccess_IsFalse()
        {
            IAppUpdate update = AppUpdate(new Version(1, 5, 2), HttpResponseFromFile("win-update.json"));
            update = await update.Latest(false);
            List<IRelease> expected = update.ReleaseHistory().ToList();

            update = update.CachedLatest(false);

            update.ReleaseHistory().Should()
                .ContainInOrder(expected);
        }

        [TestMethod]
        public async Task CachedLatest_ShouldNotChange_Releases_WhenEarlyAccess_IsTrue()
        {
            IAppUpdate update = AppUpdate(new Version(1, 5, 2), HttpResponseFromFile("win-update.json"));
            update = await update.Latest(true);
            List<IRelease> expected = update.ReleaseHistory().ToList();

            update = update.CachedLatest(true);

            update.ReleaseHistory().Should()
                .ContainInOrder(expected);
        }

        #endregion

        #region Test: Downloaded

        [TestMethod]
        public async Task Downloaded_ShouldDownload_FromFileUri()
        {
            const string fileUri = "https://protonvpn.com/download/ProtonVPN_win_v1.5.1.exe";
            IHttpResponseMessage httpResponse = HttpResponseFromFile("ProtonVPN_win_v1.5.1.exe");
            _httpClient.GetAsync(fileUri).Returns(httpResponse);

            IAppUpdate update = AppUpdate(new Version(1, 2, 0), HttpResponseFromFile("win-update.json"));
            update = await update.Latest(false);
            update.Available.Should().BeTrue();

            await update.Downloaded();

            await _httpClient.Received().GetAsync(fileUri);
        }

        [TestMethod]
        public async Task Downloaded_ShouldDownloadFile_ToDownloadsDirectory()
        {
            const string downloadsPath = nameof(Downloaded_ShouldDownloadFile_ToDownloadsDirectory);
            IAppUpdate update = AppUpdate(new Version(1, 5, 5), HttpResponseFromFile("win-update.json"), downloadsPath);
            update = await update.Latest(true);
            update.Available.Should().BeTrue();

            IHttpResponseMessage httpResponse = HttpResponseFromFile("ProtonVPN_win_v2.0.0.exe");
            _httpClient.GetAsync("https://protonvpn.com/download/ProtonVPN_win_v2.0.0.exe").Returns(httpResponse);

            string filename = Path.Combine(TestConfig.GetFolderPath(downloadsPath), "ProtonVPN_win_v2.0.0.exe");
            File.Exists(filename).Should().BeFalse();

            await update.Downloaded();

            string checkSum = await new FileCheckSum(filename).Value();
            File.Delete(filename);
            checkSum.Should().Be("961103aaf283cd90bfacb73e6cb97e2069bfa5bd9015b8f91ffd0bc1e8c791eb089e07a7df63a7da12dbb461b0777f5106819009f7a16bfaeff45f8ca941dab5");
        }

        [TestMethod]
        public async Task Downloaded_ShouldNotDownload_WhenUpdateAvailable_IsFalse_Initially()
        {
            IAppUpdate update = AppUpdate(new Version(1, 5, 5), HttpResponseFromFile("win-update.json"));
            update.Available.Should().BeFalse();

            await update.Downloaded();

            await _httpClient.DidNotReceiveWithAnyArgs().GetAsync("");
        }

        [TestMethod]
        public async Task Downloaded_ShouldNotDownload_WhenUpdateAvailable_IsFalse_AfterLatest()
        {
            IAppUpdate update = AppUpdate(new Version(2, 2, 0), HttpResponseFromFile("win-update.json"));
            update = await update.Latest(false);
            update.Available.Should().BeFalse();

            await update.Downloaded();

            await _httpClient.DidNotReceiveWithAnyArgs().GetAsync("");
        }

        [TestMethod]
        public async Task Downloaded_ShouldThrow_WhenHttpResponse_IsNotSuccess()
        {
            IAppUpdate update = AppUpdate(new Version(1, 2, 0), HttpResponseFromFile("win-update.json"));
            update = await update.Latest(false);
            update.Available.Should().BeTrue();

            IHttpResponseMessage httpResponse = Substitute.For<IHttpResponseMessage>();
            httpResponse.IsSuccessStatusCode.Returns(false);
            _httpClient.GetAsync("").ReturnsForAnyArgs(httpResponse);

            Func<Task> action = () => update.Downloaded();

            await action.Should().ThrowAsync<AppUpdateException>();
        }

        [TestMethod]
        public async Task Downloaded_ShouldThrow_WhenHttpRequest_Throws()
        {
            Exception[] exceptions =
            {
                new HttpRequestException(),
                new OperationCanceledException(),
                new SocketException()
            };

            foreach (Exception exception in exceptions)
            {
                await Downloaded_ShouldThrow_WhenHttpRequest_Throws(exception);
            }
        }

        private async Task Downloaded_ShouldThrow_WhenHttpRequest_Throws(Exception exception)
        {
            IAppUpdate update = AppUpdate(new Version(1, 2, 0), HttpResponseFromFile("win-update.json"));
            update = await update.Latest(false);
            update.Available.Should().BeTrue();

            _httpClient.GetAsync("").ReturnsForAnyArgs(FailedHttpRequest(exception));

            Func<Task> f = () => update.Downloaded();

            await f.Should().ThrowAsync<AppUpdateException>();
        }

        [TestMethod]
        public async Task Downloaded_ShouldThrow_WhenHttpRequest_Cancelled()
        {
            IAppUpdate update = AppUpdate(new Version(1, 2, 0), HttpResponseFromFile("win-update.json"));
            update = await update.Latest(false);
            update.Available.Should().BeTrue();

            _httpClient.GetAsync("").ReturnsForAnyArgs(CancelledHttpRequest());

            Func<Task> f = () => update.Downloaded();

            await f.Should().ThrowAsync<AppUpdateException>();
        }

        #endregion

        #region Helpers

        private static void CopyFile(string sourcePath, string destPath, string newFilename = null)
        {
            if (!string.IsNullOrEmpty(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            string filename = !string.IsNullOrEmpty(newFilename) ? newFilename : Path.GetFileName(sourcePath);
            string destFullPath = Path.Combine(destPath ?? "", filename ?? "");

            File.Copy(TestConfig.GetFolderPath(sourcePath), destFullPath);
        }

        private static Task<IHttpResponseMessage> CancelledHttpRequest()
        {
            return Task.FromCanceled<IHttpResponseMessage>(new CancellationToken(true));
        }

        private static Task<IHttpResponseMessage> FailedHttpRequest(Exception e)
        {
            return Task.FromException<IHttpResponseMessage>(e);
        }

        private static IHttpResponseMessage HttpResponseFromFile(string filePath)
        {
            MemoryStream stream = new();
            using (FileStream inputStream = new FileStream(TestConfig.GetFolderPath(filePath), FileMode.Open))
            {
                inputStream.CopyTo(stream);
                inputStream.Flush();
            }

            stream.Position = 0;
            return HttpResponseFromStream(stream);
        }

        private static IHttpResponseMessage HttpResponseFromString(string content)
        {
            MemoryStream stream = new();
            StreamWriter writer = new(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            return HttpResponseFromStream(stream);
        }

        private static IHttpResponseMessage HttpResponseFromStream(Stream stream)
        {
            IHttpResponseMessage httpResponse = Substitute.For<IHttpResponseMessage>();
            httpResponse.IsSuccessStatusCode.Returns(true);
            httpResponse.Content.ReadAsStreamAsync().Returns(stream);
            httpResponse.When(x => x.Dispose()).Do(x => stream.Close());

            return httpResponse;
        }

        #endregion
    }
}