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
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.OS.Net.Http;
using ProtonVPN.Update.Config;
using ProtonVPN.Update.Storage;

namespace ProtonVPN.Update.Test.Storage
{
    [TestClass]
    public class WebReleaseStorageTest
    {
        private IHttpClient _httpClient;
        private DefaultAppUpdateConfig _config;

        #region Initialization

        [TestInitialize]
        public void TestInitialize()
        {
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

        private IReleaseStorage WebReleaseStorage(Task<IHttpResponseMessage> httpResponse)
        {
            _httpClient.GetAsync(_config.FeedUri).Returns(httpResponse);
            return WebReleaseStorage();
        }

        private IReleaseStorage WebReleaseStorage(IHttpResponseMessage httpResponse)
        {
            _httpClient.GetAsync(_config.FeedUri).Returns(httpResponse);
            return WebReleaseStorage();
        }

        private IReleaseStorage WebReleaseStorage()
        {
            return new WebReleaseStorage(_config);
        }

        #endregion

        [TestMethod]
        public async Task Releases_ShouldGet_FromFeedUri()
        {
            var feedUri = new Uri("https://protonvpn.com/download/win-update.json");
            _config.FeedUri = feedUri;
            var storage = WebReleaseStorage(HttpResponseFromFile("win-update.json"));

            await storage.Releases();

            await _httpClient.Received().GetAsync(feedUri);
        }

        [TestMethod]
        public async Task Releases_ShouldBe_AllFromSource()
        {
            var storage = WebReleaseStorage(HttpResponseFromFile("win-update.json"));

            var result = await storage.Releases();

            result.Should().HaveCount(5);
        }

        [TestMethod]
        public void Releases_ShouldThrow_WhenHttpResponse_IsNotSuccess()
        {
            var httpResponse = Substitute.For<IHttpResponseMessage>();
            httpResponse.IsSuccessStatusCode.Returns(false);
            var storage = WebReleaseStorage(httpResponse);

            Func<Task> action = () => storage.Releases();

            action.Should().Throw<HttpRequestException>();
        }

        [TestMethod]
        public void Releases_ShouldThrow_WhenHttpRequest_Throws()
        {
            Exception[] exceptions =
            {
                new HttpRequestException(),
                new OperationCanceledException(),
                new SocketException()
            };

            foreach (var exception in exceptions)
            {
                Releases_ShouldThrow_WhenHttpRequest_Throws(exception);
                Releases_ShouldThrow_WhenHttpResponse_Throws(exception);
            }
        }

        private void Releases_ShouldThrow_WhenHttpRequest_Throws<TE>(TE exception) where TE : Exception
        {
            var storage = WebReleaseStorage(FailedHttpRequest(exception));

            Func<Task> action = () => storage.Releases();

            action.Should().Throw<TE>();
        }

        private void Releases_ShouldThrow_WhenHttpResponse_Throws<TE>(TE exception) where TE : Exception
        {
            var storage = WebReleaseStorage(FailedHttpResponse(exception));

            Func<Task> action = () => storage.Releases();

            action.Should().Throw<TE>();
        }

        [TestMethod]
        public void Releases_ShouldThrow_WhenHttpRequest_Cancelled()
        {
            var storage = WebReleaseStorage(CancelledHttpRequest());

            Func<Task> action = () => storage.Releases();

            action.Should().Throw<TaskCanceledException>();
        }

        [TestMethod]
        public void Releases_ShouldThrow_WhenHttpResponse_Cancelled()
        {
            var storage = WebReleaseStorage(CancelledHttpResponse());

            Func<Task> action = () => storage.Releases();

            action.Should().Throw<TaskCanceledException>();
        }

        #region Helpers

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
