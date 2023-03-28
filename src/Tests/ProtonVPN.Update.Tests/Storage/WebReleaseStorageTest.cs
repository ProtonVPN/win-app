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
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.OS.Net.Http;
using ProtonVPN.Tests.Common;
using ProtonVPN.Update.Config;
using ProtonVPN.Update.Releases;
using ProtonVPN.Update.Storage;

namespace ProtonVPN.Update.Tests.Storage
{
    [TestClass]
    public class WebReleaseStorageTest
    {
        private ILogger _logger;
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

        private IReleaseStorage WebReleaseStorage(Task<IHttpResponseMessage> httpResponse)
        {
            _httpClient.GetAsync(_config.FeedUriProvider.GetFeedUrls().First()).Returns(httpResponse);
            return WebReleaseStorage();
        }

        private IReleaseStorage WebReleaseStorage(IHttpResponseMessage httpResponse)
        {
            _httpClient.GetAsync(_config.FeedUriProvider.GetFeedUrls().First()).Returns(httpResponse);
            return WebReleaseStorage();
        }

        private IReleaseStorage WebReleaseStorage()
        {
            return new WebReleaseStorage(_config, _logger);
        }

        #endregion

        [TestMethod]
        public async Task Releases_ShouldGet_FromFeedUri()
        {
            Uri feedUri = new Uri("http://127.0.0.1/windows-releases.json");
            _feedUrlProvider.GetFeedUrls().Returns(_feedUrls);
            IReleaseStorage storage = WebReleaseStorage(HttpResponseFromFile("win-update.json"));

            await storage.Releases();

            await _httpClient.Received().GetAsync(feedUri);
        }

        [TestMethod]
        public async Task Releases_ShouldBe_AllFromSource()
        {
            IReleaseStorage storage = WebReleaseStorage(HttpResponseFromFile("win-update.json"));

            IEnumerable<Release> result = await storage.Releases();

            result.Should().HaveCount(5);
        }

        [TestMethod]
        public void Releases_ShouldThrow_WhenHttpResponse_IsNotSuccess()
        {
            IHttpResponseMessage httpResponse = Substitute.For<IHttpResponseMessage>();
            httpResponse.IsSuccessStatusCode.Returns(false);
            IReleaseStorage storage = WebReleaseStorage(httpResponse);

            Func<Task> action = () => storage.Releases();

            action.Should().ThrowAsync<HttpRequestException>();
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

            foreach (Exception exception in exceptions)
            {
                Releases_ShouldThrow_WhenHttpRequest_Throws(exception);
                Releases_ShouldThrow_WhenHttpResponse_Throws(exception);
            }
        }

        private void Releases_ShouldThrow_WhenHttpRequest_Throws<TE>(TE exception) where TE : Exception
        {
            IReleaseStorage storage = WebReleaseStorage(FailedHttpRequest(exception));

            Func<Task> action = () => storage.Releases();

            action.Should().ThrowAsync<TE>();
        }

        private void Releases_ShouldThrow_WhenHttpResponse_Throws<TE>(TE exception) where TE : Exception
        {
            IReleaseStorage storage = WebReleaseStorage(FailedHttpResponse(exception));

            Func<Task> action = () => storage.Releases();

            action.Should().ThrowAsync<TE>();
        }

        [TestMethod]
        public void Releases_ShouldThrow_WhenHttpRequest_Cancelled()
        {
            IReleaseStorage storage = WebReleaseStorage(CancelledHttpRequest());

            Func<Task> action = () => storage.Releases();

            action.Should().ThrowAsync<TaskCanceledException>();
        }

        [TestMethod]
        public void Releases_ShouldThrow_WhenHttpResponse_Cancelled()
        {
            IReleaseStorage storage = WebReleaseStorage(CancelledHttpResponse());

            Func<Task> action = () => storage.Releases();

            action.Should().ThrowAsync<TaskCanceledException>();
        }

        #region Helpers

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