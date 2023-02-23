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
using ProtonVPN.Tests.Common;
using ProtonVPN.Update.Files.Downloadable;
using ProtonVPN.Update.Files.Validatable;

namespace ProtonVPN.Update.Tests.Files.Downloadable
{
    [TestClass]
    public class DownloadableFileTest
    {
        private const string APP_FILENAME = "ProtonVPN.exe";
        private const string INSTALLER_FILENAME = "ProtonVPN_win_v1.5.1.exe";
        private const string INSTALLER_DOWNLOAD_URL = "https://protonvpn.com/download/" + INSTALLER_FILENAME;
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
            string filename = Path.Combine(DownloadsPath(), APP_FILENAME);
            IDownloadableFile downloadable = DownloadableFile(INSTALLER_DOWNLOAD_URL, HttpResponseFromFile(INSTALLER_FILENAME));

            await downloadable.Download(INSTALLER_DOWNLOAD_URL, filename);
            File.Delete(filename);

            await _httpClient.Received().GetAsync(INSTALLER_DOWNLOAD_URL);
        }

        [TestMethod]
        public async Task Download_ShouldDownloadFile_ToFilename()
        {
            string filename = Path.Combine(DownloadsPath(), APP_FILENAME);
            File.Exists(filename).Should().BeFalse();
            IDownloadableFile downloadable = DownloadableFile(INSTALLER_DOWNLOAD_URL, HttpResponseFromFile(INSTALLER_FILENAME));

            await downloadable.Download(INSTALLER_DOWNLOAD_URL, filename);

            string checkSum = await new FileCheckSum(filename).Value();
            File.Delete(filename);
            checkSum.Should().Be("c011146ae24f5a49ef86ff6199ec0bd42223b408e1dce3ffef9a2ef4b9c1806b1c823ce427d7473378b7d8c427ba6cb3701320740523ad79fc9ec8cfeb907875");
        }

        [TestMethod]
        public void Download_ShouldThrow_WhenHttpResponse_IsNotSuccess()
        {
            string filename = Path.Combine(DownloadsPath(), APP_FILENAME);

            IHttpResponseMessage httpResponse = Substitute.For<IHttpResponseMessage>();
            httpResponse.IsSuccessStatusCode.Returns(false);
            IDownloadableFile downloadable = DownloadableFile(INSTALLER_DOWNLOAD_URL, httpResponse);

            Func<Task> action = () => downloadable.Download(INSTALLER_DOWNLOAD_URL, filename);
            File.Delete(filename);

            action.Should().ThrowAsync<AppUpdateException>();
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

            foreach (Exception exception in exceptions)
            {
                Download_ShouldThrow_WhenHttpRequest_Throws(exception);
                Download_ShouldThrow_WhenHttpResponse_Throws(exception);
            }
        }

        private void Download_ShouldThrow_WhenHttpRequest_Throws<TE>(TE exception) where TE: Exception
        {
            string filename = Path.Combine(DownloadsPath(), APP_FILENAME);

            IDownloadableFile downloadable = DownloadableFile(INSTALLER_DOWNLOAD_URL, FailedHttpRequest(exception));

            Func<Task> action = () => downloadable.Download(INSTALLER_DOWNLOAD_URL, filename);
            File.Delete(filename);

            action.Should().ThrowAsync<TE>();
        }

        private void Download_ShouldThrow_WhenHttpResponse_Throws<TE>(TE exception) where TE : Exception
        {
            string filename = Path.Combine(DownloadsPath(), APP_FILENAME);

            IDownloadableFile downloadable = DownloadableFile(INSTALLER_DOWNLOAD_URL, FailedHttpResponse(exception));

            Func<Task> action = () => downloadable.Download(INSTALLER_DOWNLOAD_URL, filename);
            File.Delete(filename);

            action.Should().ThrowAsync<TE>();
        }

        [TestMethod]
        public void Download_ShouldThrow_WhenHttpRequest_Cancelled()
        {
            string filename = Path.Combine(DownloadsPath(), APP_FILENAME);

            IDownloadableFile downloadable = DownloadableFile(INSTALLER_DOWNLOAD_URL, CancelledHttpRequest());

            Func<Task> action = () => downloadable.Download(INSTALLER_DOWNLOAD_URL, filename);
            File.Delete(filename);

            action.Should().ThrowAsync<TaskCanceledException>();
        }

        [TestMethod]
        public void Download_ShouldThrow_WhenHttpResponse_Cancelled()
        {
            string filename = Path.Combine(DownloadsPath(), APP_FILENAME);

            IDownloadableFile downloadable = DownloadableFile(INSTALLER_DOWNLOAD_URL, CancelledHttpResponse());

            Func<Task> action = () => downloadable.Download(INSTALLER_DOWNLOAD_URL, filename);
            File.Delete(filename);

            action.Should().ThrowAsync<TaskCanceledException>();
        }

        #region Helpers

        private string DownloadsPath([CallerMemberName] string path = null)
        {
            Contract.Assume(path != null);
            path = TestConfig.GetFolderPath(path);
            Directory.CreateDirectory(path);
            return path;
        }

        private static Task<IHttpResponseMessage> CancelledHttpRequest()
        {
            return Task.FromCanceled<IHttpResponseMessage>(new CancellationToken(true));
        }

        private static Task<IHttpResponseMessage> CancelledHttpResponse()
        {
            IHttpResponseMessage httpResponse = Substitute.For<IHttpResponseMessage>();
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
            IHttpResponseMessage httpResponse = Substitute.For<IHttpResponseMessage>();
            httpResponse.IsSuccessStatusCode.Returns(true);
            httpResponse.Content.ReadAsStreamAsync().Returns(Task.FromException<Stream>(e));

            return Task.FromResult(httpResponse);
        }

        private static IHttpResponseMessage HttpResponseFromFile(string filePath)
        {
            MemoryStream stream = new();
            using (FileStream inputStream = new(TestConfig.GetFolderPath(filePath), FileMode.Open))
            {
                inputStream.CopyTo(stream);
                inputStream.Flush();
            }

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