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
using System.Diagnostics.Contracts;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.OS.Net.Http;
using ProtonVPN.Update.Files.Downloadable;
using ProtonVPN.Update.Files.Validatable;

namespace ProtonVPN.Update.Test.Files.Downloadable
{
    [TestClass]
    [DeploymentItem("TestData", "TestData")]
    public class DownloadableFileTest
    {
        private IHttpClient _httpClient;

        #region Initialization

        [TestInitialize]
        public void TestInitialize()
        {
            _httpClient = Substitute.For<IHttpClient>();
        }

        private IDownloadableFile DownloadableFile(string uri, Task<IHttpResponseMessage> httpResponse)
        {
            _httpClient.GetAsync(uri).Returns(httpResponse);
            return DownloadableFile();
        }

        private IDownloadableFile DownloadableFile(string uri, IHttpResponseMessage httpResponse)
        {
            _httpClient.GetAsync(uri).Returns(httpResponse);
            return DownloadableFile();
        }

        private IDownloadableFile DownloadableFile()
        {
            return new DownloadableFile(_httpClient);
        }

        #endregion

        [TestMethod]
        public async Task Download_ShouldDownload_FromFileUri()
        {
            var filename = Path.Combine(DownloadsPath(), "ProtonVPN.exe");
            const string fileUri = "https://protonvpn.com/download/ProtonVPN_win_v1.5.1.exe";
            var downloadable = DownloadableFile(fileUri, HttpResponseFromFile("ProtonVPN_win_v1.5.1.exe"));

            await downloadable.Download(fileUri, filename);

            await _httpClient.Received().GetAsync(fileUri);
        }

        [TestMethod]
        public async Task Download_ShouldDownloadFile_ToFilename()
        {
            var filename = Path.Combine(DownloadsPath(), "ProtonVPN.exe");
            const string fileUri = "https://protonvpn.com/download/ProtonVPN_win_v1.5.1.exe";
            File.Exists(filename).Should().BeFalse();
            var downloadable = DownloadableFile(fileUri, HttpResponseFromFile("ProtonVPN_win_v1.5.1.exe"));

            await downloadable.Download(fileUri, filename);

            var checkSum = await new FileCheckSum(filename).Value();
            checkSum.Should().Be("ba6b5ca2db65ff7817e3336a386e7525c01dc639");
        }

        [TestMethod]
        public void Download_ShouldThrow_WhenHttpResponse_IsNotSuccess()
        {
            var filename = Path.Combine(DownloadsPath(), "ProtonVPN.exe");
            const string fileUri = "https://protonvpn.com/download/ProtonVPN_win_v1.5.1.exe";

            var httpResponse = Substitute.For<IHttpResponseMessage>();
            httpResponse.IsSuccessStatusCode.Returns(false);
            var downloadable = DownloadableFile(fileUri, httpResponse);

            Func<Task> action = () => downloadable.Download(fileUri, filename);

            action.Should().Throw<AppUpdateException>();
        }

        [TestMethod]
        public void Download_ShouldThrow_WhenHttpRequest_Throws()
        {
            Exception[] exceptions =
            {
                new HttpRequestException(),
                new OperationCanceledException(),
                new SocketException()
            };

            foreach (var exception in exceptions)
            {
                Download_ShouldThrow_WhenHttpRequest_Throws(exception);
                Download_ShouldThrow_WhenHttpResponse_Throws(exception);
            }
        }

        private void Download_ShouldThrow_WhenHttpRequest_Throws<TE>(TE exception) where TE: Exception
        {
            var filename = Path.Combine(DownloadsPath(), "ProtonVPN.exe");
            const string fileUri = "https://protonvpn.com/download/ProtonVPN_win_v1.5.1.exe";

            var downloadable = DownloadableFile(fileUri, FailedHttpRequest(exception));

            Func<Task> action = () => downloadable.Download(fileUri, filename);

            action.Should().Throw<TE>();
        }

        private void Download_ShouldThrow_WhenHttpResponse_Throws<TE>(TE exception) where TE : Exception
        {
            var filename = Path.Combine(DownloadsPath(), "ProtonVPN.exe");
            const string fileUri = "https://protonvpn.com/download/ProtonVPN_win_v1.5.1.exe";

            var downloadable = DownloadableFile(fileUri, FailedHttpResponse(exception));

            Func<Task> action = () => downloadable.Download(fileUri, filename);

            action.Should().Throw<TE>();
        }

        [TestMethod]
        public void Download_ShouldThrow_WhenHttpRequest_Cancelled()
        {
            var filename = Path.Combine(DownloadsPath(), "ProtonVPN.exe");
            const string fileUri = "https://protonvpn.com/download/ProtonVPN_win_v1.5.1.exe";

            var downloadable = DownloadableFile(fileUri, CancelledHttpRequest());

            Func<Task> action = () => downloadable.Download(fileUri, filename);

            action.Should().Throw<TaskCanceledException>();
        }

        [TestMethod]
        public void Download_ShouldThrow_WhenHttpResponse_Cancelled()
        {
            var filename = Path.Combine(DownloadsPath(), "ProtonVPN.exe");
            const string fileUri = "https://protonvpn.com/download/ProtonVPN_win_v1.5.1.exe";

            var downloadable = DownloadableFile(fileUri, CancelledHttpResponse());

            Func<Task> action = () => downloadable.Download(fileUri, filename);

            action.Should().Throw<TaskCanceledException>();
        }

        #region Helpers

        private string DownloadsPath([CallerMemberName] string path = null)
        {
            Contract.Assume(path != null);

            Directory.CreateDirectory(path);
            return path;
        }

        private static Task<IHttpResponseMessage> CancelledHttpRequest()
        {
            return Task.FromCanceled<IHttpResponseMessage>(new CancellationToken(true));
        }

        private static Task<IHttpResponseMessage> CancelledHttpResponse()
        {
            var httpResponse = Substitute.For<IHttpResponseMessage>();
            httpResponse.IsSuccessStatusCode.Returns(true);
            httpResponse.Content.ReadAsStreamAsync().Returns(Task.FromCanceled<Stream>(new CancellationToken(true)));

            return Task.FromResult(httpResponse);
        }

        private static Task<IHttpResponseMessage> FailedHttpRequest(Exception e)
        {
            return Task.FromException<IHttpResponseMessage>(e);
        }

        private static Task<IHttpResponseMessage> FailedHttpResponse(Exception e)
        {
            var httpResponse = Substitute.For<IHttpResponseMessage>();
            httpResponse.IsSuccessStatusCode.Returns(true);
            httpResponse.Content.ReadAsStreamAsync().Returns(Task.FromException<Stream>(e));

            return Task.FromResult(httpResponse);
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
