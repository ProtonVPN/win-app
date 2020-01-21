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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.OS.Net.Http;
using ProtonVPN.Update.Config;
using ProtonVPN.Update.Files.Launchable;
using ProtonVPN.Update.Files.Validatable;
using ProtonVPN.Update.Updates;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable ObjectCreationAsStatement

namespace ProtonVPN.Update.Test.Updates
{
    [TestClass]
    [DeploymentItem("TestData", "TestData")]
    public class AppUpdateTest
    {
        private ILaunchableFile _launchableFile;
        private IHttpClient _httpClient;
        private DefaultAppUpdateConfig _config;

        #region Initialization

        [TestInitialize]
        public void TestInitialize()
        {
            _launchableFile = Substitute.For<ILaunchableFile>();
            _httpClient = Substitute.For<IHttpClient>();
            _config = new DefaultAppUpdateConfig
            {
                HttpClient = _httpClient,
                FeedUri = new Uri("http://127.0.0.1/win-update.json"),
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
            _config.UpdatesPath = updatesPath;
            _httpClient.GetAsync(_config.FeedUri).Returns(httpResponse);
            return AppUpdate();
        }

        private IAppUpdate AppUpdate(IHttpResponseMessage httpResponse, [CallerMemberName] string updatesPath = null)
        {
            _config.UpdatesPath = updatesPath;
            _httpClient.GetAsync(_config.FeedUri).Returns(httpResponse);
            return AppUpdate();
        }

        private IAppUpdate AppUpdate()
        {
            return new AppUpdate(new AppUpdates(_config, _launchableFile));
        }

        #endregion

        #region Test: ReleaseHistory

        [TestMethod]
        public void ReleaseHistory_ShouldBe_Empty_Initially()
        {
            var update = AppUpdate();

            update.ReleaseHistory().Should().BeEmpty();
        }

        [TestMethod]
        public async Task ReleaseHistory_ShouldReturn_Stable_Releases()
        {
            var update = AppUpdate(HttpResponseFromFile("win-update.json"));
            update = await update.Latest(false);

            var result = update.ReleaseHistory();

            result.Should()
                .HaveCount(3).And
                .Match(r => r.All(x => !x.EarlyAccess));
        }

        [TestMethod]
        public async Task ReleaseHistory_ShouldReturn_StableAndEarlyAccess_Releases()
        {
            var update = AppUpdate(HttpResponseFromFile("win-update.json"));
            update = await update.Latest(true);

            var result = update.ReleaseHistory();

            result.Should()
                .HaveCount(5).And
                .Match(r => r.Count(x => x.EarlyAccess) == 2);
        }

        [TestMethod]
        public async Task ReleaseHistory_ShouldReturn_StableAndEarlyAccess_Releases_UpToCurrentVersion()
        {
            var update = AppUpdate(new Version(1, 5, 2), HttpResponseFromFile("win-update.json"));
            update = await update.Latest(false);

            var result = update.ReleaseHistory();

            result.Should()
                .HaveCount(4).And
                .Match(r => r.First().Version == _config.CurrentVersion).And
                .Match(r => r.First().EarlyAccess);
        }

        [TestMethod]
        public async Task ReleaseHistory_ShouldReturn_Releases_WithChangeLog()
        {
            var update = AppUpdate(HttpResponseFromFile("win-update.json"));
            update = await update.Latest(false);

            var result = update.ReleaseHistory();

            result[0].ChangeLog.Should()
                .HaveCount(2).And
                .Match(l => l.All(x => !string.IsNullOrEmpty(x)));
        }

        [TestMethod]
        public async Task ReleaseHistory_ShouldReturn_Releases_OrderedByVersion()
        {
            var update = AppUpdate(HttpResponseFromFile("win-update.json"));
            update = await update.Latest(true);

            var result = update.ReleaseHistory();
            var expected = result.OrderByDescending(r => r.Version);

            result.Should().ContainInOrder(expected);
        }

        #endregion

        #region Test: Available

        [TestMethod]
        public void Available_ShouldBeFalse_Initially()
        {
            var update = AppUpdate();
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
            var update = AppUpdate(new Version(major, minor, build), HttpResponseFromFile("win-update.json"));

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
            var update = AppUpdate(new Version(major, minor, build), HttpResponseFromFile("win-update.json"));
            update = await update.Latest(false);

            update = update.CachedLatest(earlyAccess);

            update.Available.Should().Be(result);
        }

        [TestMethod]
        public async Task Available_ShouldBe_False_WhenLatestRelease_HasNoFile()
        {
            const string json = "{\"Categories\": [{\"Name\": \"Stable\", \"Releases\": [{\"Version\": \"2.0.0\", \"ChangeLog\": [\"line 1\"] }] }] }";
            var update = AppUpdate(new Version(1, 0, 0), HttpResponseFromString(json));

            update = await update.Latest(false);

            update.Available.Should().BeFalse();
        }

        [TestMethod]
        public async Task Available_ShouldBe_False_WhenLatestRelease_FileHasNoUrl()
        {
            const string json = "{\"Categories\": [{\"Name\": \"Stable\", \"Releases\": [{\"Version\": \"2.0.0\", \"ChangeLog\": [\"line 1\"], \"File\": {\"Sha512CheckSum\": \"a b c d e f g h\"}} ] }] }";
            var update = AppUpdate(new Version(1, 0, 0), HttpResponseFromString(json));

            update = await update.Latest(false);

            update.Available.Should().BeFalse();
        }

        [TestMethod]
        public async Task Available_ShouldBe_False_WhenLatestRelease_FileHasNoChecksum()
        {
            const string json = "{\"Categories\": [{\"Name\": \"Stable\", \"Releases\": [{\"Version\": \"2.0.0\", \"ChangeLog\": [\"line 1\"], \"File\": {\"Url\": \"https://protonvpn.com/download/ProtonVPN_win_v1.5.2.exe\"}} ] }] }";
            var update = AppUpdate(new Version(1, 0, 0), HttpResponseFromString(json));

            update = await update.Latest(false);

            update.Available.Should().BeFalse();
        }

        [TestMethod]
        public async Task Available_ShouldNotChange_AfterDownload_WhenItWasFalse()
        {
            var update = AppUpdate(new Version(1, 5, 5), HttpResponseFromFile("win-update.json"));
            update = await update.Latest(false);
            update.Available.Should().BeFalse();

            update = await update.Downloaded();

            update.Available.Should().BeFalse();
        }

        [TestMethod]
        public async Task Available_ShouldNotChange_AfterDownload_WhenItWasTrue()
        {
            const string fileUri = "https://protonvpn.com/download/ProtonVPN_win_v2.0.0.exe";
            var httpResponse = HttpResponseFromFile("ProtonVPN_win_v2.0.0.exe");
            _httpClient.GetAsync(fileUri).Returns(httpResponse);

            var update = AppUpdate(new Version(1, 5, 5), HttpResponseFromFile("win-update.json"));
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
            var update = AppUpdate();

            update.Ready.Should().BeFalse();
        }

        [TestMethod]
        public async Task Ready_ShouldBeFalse_AfterLatest_WhenUpdateNotAvailable()
        {
            var update = AppUpdate(new Version(1, 5, 5), HttpResponseFromFile("win-update.json"));
            update = await update.Latest(true);
            update.Available.Should().BeTrue();

            var response = HttpResponseFromFile("win-update.json");
            _httpClient.GetAsync(_config.FeedUri).Returns(response);

            update = await update.Latest(false);
            update.Available.Should().BeFalse();

            update.Ready.Should().BeFalse();
        }

        [TestMethod]
        public async Task Ready_ShouldBeFalse_AfterCachedLatest_WhenUpdateNotAvailable()
        {
            var update = AppUpdate(new Version(1, 5, 5), HttpResponseFromFile("win-update.json"));
            update = await update.Latest(true);
            update.Available.Should().BeTrue();

            update = update.CachedLatest(false);
            update.Available.Should().BeFalse();

            update.Ready.Should().BeFalse();
        }

        [TestMethod]
        public async Task Ready_ShouldNotChange_AfterDownloaded_WhenNotAvailable()
        {
            var update = AppUpdate(new Version(1, 5, 5), HttpResponseFromFile("win-update.json"));
            update = await update.Latest(false);
            update.Available.Should().BeFalse();

            update = await update.Downloaded();

            update.Ready.Should().BeFalse();
        }

        [TestMethod]
        public async Task Ready_ShouldNotChange_AfterDownloaded_WhenFalse()
        {
            var httpResponse = HttpResponseFromFile("ProtonVPN_win_v2.0.0.exe");
            _httpClient.GetAsync("https://protonvpn.com/download/ProtonVPN_win_v2.0.0.exe").Returns(httpResponse);

            var update = AppUpdate(new Version(1, 5, 5), HttpResponseFromFile("win-update.json"));
            update = await update.Latest(true);
            update.Ready.Should().BeFalse();

            update = await update.Downloaded();

            update.Ready.Should().BeFalse();
        }

        [TestMethod]
        public async Task Ready_ShouldNotChange_AfterDownloaded_WhenTrue()
        {
            var httpResponse = HttpResponseFromFile("ProtonVPN_win_v2.0.0.exe");
            _httpClient.GetAsync("https://protonvpn.com/download/ProtonVPN_win_v2.0.0.exe").Returns(httpResponse);

            var update = AppUpdate(new Version(1, 5, 5), HttpResponseFromFile("win-update.json"));
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
            var update = AppUpdate(new Version(1, 2, 0), HttpResponseFromFile("win-update.json"), downloadsPath);
            update = await update.Latest(false);
            update.Available.Should().BeTrue();

            CopyFile("ProtonVPN_win_v1.5.1.exe", downloadsPath);

            update = await update.Validated();

            update.Ready.Should().BeTrue();
        }

        [TestMethod]
        public async Task Ready_ShouldBeFalse_AfterValidated_WhenFileCheckSum_IsNotValid()
        {
            var httpResponse = HttpResponseFromFile("ProtonVPN_win_v1.0.0.exe");
            _httpClient.GetAsync("https://protonvpn.com/download/ProtonVPN_win_v1.5.1.exe").Returns(httpResponse);

            var update = AppUpdate(new Version(1, 2, 0), HttpResponseFromFile("win-update.json"));
            update = await update.Latest(false);
            update.Available.Should().BeTrue();

            update = await update.Downloaded();
            update = await update.Validated();

            update.Ready.Should().BeFalse();
        }

        [TestMethod]
        public async Task Ready_ShouldBeTrue_AfterValidated_WhenFileCheckSum_IsValid()
        {
            var httpResponse = HttpResponseFromFile("ProtonVPN_win_v2.0.0.exe");
            _httpClient.GetAsync("https://protonvpn.com/download/ProtonVPN_win_v2.0.0.exe").Returns(httpResponse);

            var update = AppUpdate(new Version(1, 5, 1), HttpResponseFromFile("win-update.json"));
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
            _config.FeedUri = new Uri("http://protonvpn.com/update.json");
            var update = AppUpdate(HttpResponseFromFile("win-update.json"));

            await update.Latest(false);

            await _httpClient.Received().GetAsync(_config.FeedUri);
        }

        [TestMethod]
        public void Latest_ShouldThrow_WhenHttpResponse_IsNotSuccess()
        {
            var httpResponse = Substitute.For<IHttpResponseMessage>();
            httpResponse.IsSuccessStatusCode.Returns(false);
            var update = AppUpdate(httpResponse);

            Func<Task> action = () => update.Latest(false);

            action.Should().Throw<AppUpdateException>();
        }

        [TestMethod]
        public void Latest_ShouldThrow_WhenHttpResponse_IsEmpty()
        {
            var httpResponse = HttpResponseFromFile("Empty file.txt");
            var update = AppUpdate(httpResponse);

            Func<Task> action = () => update.Latest(false);

            action.Should().Throw<AppUpdateException>();
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

            foreach (var exception in exceptions)
            {
                Latest_ShouldThrow_WhenHttpRequest_Throws(exception);
            }
        }

        private void Latest_ShouldThrow_WhenHttpRequest_Throws(Exception exception)
        {
            var update = AppUpdate(FailedHttpRequest(exception));

            Func<Task> action = () => update.Latest(false);

            action.Should().Throw<AppUpdateException>();
        }

        [TestMethod]
        public void Latest_ShouldThrow_WhenHttpRequest_Cancelled()
        {
            var update = AppUpdate(CancelledHttpRequest());

            Func<Task> action = () => update.Latest(false);

            action.Should().Throw<AppUpdateException>();
        }

        [TestMethod]
        public void Latest_ShouldThrow_WhenHttpResponse_IsNotJson()
        {
            var update = AppUpdate(HttpResponseFromString("HTTP"));

            Func<Task> action = () => update.Latest(false);

            action.Should().Throw<AppUpdateException>();
        }

        #endregion

        #region Test: CachedLatest

        [TestMethod]
        public async Task CachedLatest_ShouldNotGet_JsonFile()
        {
            var update = AppUpdate(HttpResponseFromFile("win-update.json"));

            update.CachedLatest(false);

            await _httpClient.DidNotReceiveWithAnyArgs().GetAsync("");
        }

        [TestMethod]
        public async Task CachedLatest_ShouldNotChange_Releases_WhenEarlyAccess_IsFalse()
        {
            var update = AppUpdate(new Version(1, 5, 2), HttpResponseFromFile("win-update.json"));
            update = await update.Latest(false);
            var expected = update.ReleaseHistory().ToList();

            update = update.CachedLatest(false);

            update.ReleaseHistory().Should()
                .ContainInOrder(expected);
        }

        [TestMethod]
        public async Task CachedLatest_ShouldNotChange_Releases_WhenEarlyAccess_IsTrue()
        {
            var update = AppUpdate(new Version(1, 5, 2), HttpResponseFromFile("win-update.json"));
            update = await update.Latest(true);
            var expected = update.ReleaseHistory().ToList();

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
            var httpResponse = HttpResponseFromFile("ProtonVPN_win_v1.5.1.exe");
            _httpClient.GetAsync(fileUri).Returns(httpResponse);

            var update = AppUpdate(new Version(1, 2, 0), HttpResponseFromFile("win-update.json"));
            update = await update.Latest(false);
            update.Available.Should().BeTrue();

            await update.Downloaded();

            await _httpClient.Received().GetAsync(fileUri);
        }

        [TestMethod]
        public async Task Downloaded_ShouldDownloadFile_ToDownloadsDirectory()
        {
            const string downloadsPath = nameof(Downloaded_ShouldDownloadFile_ToDownloadsDirectory);
            var update = AppUpdate(new Version(1, 5, 5), HttpResponseFromFile("win-update.json"), downloadsPath);
            update = await update.Latest(true);
            update.Available.Should().BeTrue();

            var httpResponse = HttpResponseFromFile("ProtonVPN_win_v2.0.0.exe");
            _httpClient.GetAsync("https://protonvpn.com/download/ProtonVPN_win_v2.0.0.exe").Returns(httpResponse);

            var filename = Path.Combine(downloadsPath, "ProtonVPN_win_v2.0.0.exe");
            File.Exists(filename).Should().BeFalse();

            await update.Downloaded();

            var checkSum = await new FileCheckSum(filename).Value();
            checkSum.Should().Be("961103aaf283cd90bfacb73e6cb97e2069bfa5bd9015b8f91ffd0bc1e8c791eb089e07a7df63a7da12dbb461b0777f5106819009f7a16bfaeff45f8ca941dab5");
        }

        [TestMethod]
        public async Task Downloaded_ShouldNotDownload_WhenUpdateAvailable_IsFalse_Initially()
        {
            var update = AppUpdate(new Version(1, 5, 5), HttpResponseFromFile("win-update.json"));
            update.Available.Should().BeFalse();

            await update.Downloaded();

            await _httpClient.DidNotReceiveWithAnyArgs().GetAsync("");
        }

        [TestMethod]
        public async Task Downloaded_ShouldNotDownload_WhenUpdateAvailable_IsFalse_AfterLatest()
        {
            var update = AppUpdate(new Version(2, 2, 0), HttpResponseFromFile("win-update.json"));
            update = await update.Latest(false);
            update.Available.Should().BeFalse();

            await update.Downloaded();

            await _httpClient.DidNotReceiveWithAnyArgs().GetAsync("");
        }

        [TestMethod]
        public async Task Downloaded_ShouldThrow_WhenHttpResponse_IsNotSuccess()
        {
            var update = AppUpdate(new Version(1, 2, 0), HttpResponseFromFile("win-update.json"));
            update = await update.Latest(false);
            update.Available.Should().BeTrue();

            var httpResponse = Substitute.For<IHttpResponseMessage>();
            httpResponse.IsSuccessStatusCode.Returns(false);
            _httpClient.GetAsync("").ReturnsForAnyArgs(httpResponse);

            Func<Task> action = () => update.Downloaded();

            action.Should().Throw<AppUpdateException>();
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

            foreach (var exception in exceptions)
            {
                await Downloaded_ShouldThrow_WhenHttpRequest_Throws(exception);
            }
        }

        private async Task Downloaded_ShouldThrow_WhenHttpRequest_Throws(Exception exception)
        {
            var update = AppUpdate(new Version(1, 2, 0), HttpResponseFromFile("win-update.json"));
            update = await update.Latest(false);
            update.Available.Should().BeTrue();

            _httpClient.GetAsync("").ReturnsForAnyArgs(FailedHttpRequest(exception));

            Func<Task> f = () => update.Downloaded();

            f.Should().Throw<AppUpdateException>();
        }

        [TestMethod]
        public async Task Downloaded_ShouldThrow_WhenHttpRequest_Cancelled()
        {
            var update = AppUpdate(new Version(1, 2, 0), HttpResponseFromFile("win-update.json"));
            update = await update.Latest(false);
            update.Available.Should().BeTrue();

            _httpClient.GetAsync("").ReturnsForAnyArgs(CancelledHttpRequest());

            Func<Task> f = () => update.Downloaded();

            f.Should().Throw<AppUpdateException>();
        }

        #endregion

        #region Helpers

        private static void CopyFile(string sourcePath, string destPath, string newFilename = null)
        {
            if (!string.IsNullOrEmpty(destPath))
                Directory.CreateDirectory(destPath);

            var filename = !string.IsNullOrEmpty(newFilename) ? newFilename : Path.GetFileName(sourcePath);
            var destFullPath = Path.Combine(destPath ?? "", filename ?? "");

            File.Copy(Path.Combine("TestData", sourcePath), destFullPath);
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
            var stream = new MemoryStream();
            using (var inputStream = new FileStream(Path.Combine("TestData", filePath), FileMode.Open))
            {
                inputStream.CopyTo(stream);
                inputStream.Flush();
            }

            stream.Position = 0;
            return HttpResponseFromStream(stream);
        }

        private static IHttpResponseMessage HttpResponseFromString(string content)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            return HttpResponseFromStream(stream);
        }

        private static IHttpResponseMessage HttpResponseFromStream(Stream stream)
        {
            var httpResponse = Substitute.For<IHttpResponseMessage>();
            httpResponse.IsSuccessStatusCode.Returns(true);
            httpResponse.Content.ReadAsStreamAsync().Returns(stream);
            httpResponse.When(x => x.Dispose()).Do(x => stream.Close());

            return httpResponse;
        }

        #endregion
    }
}
