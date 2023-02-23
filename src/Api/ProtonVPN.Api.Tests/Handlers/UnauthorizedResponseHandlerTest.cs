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
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Auth;
using ProtonVPN.Api.Handlers;
using ProtonVPN.Api.Tests.Mocks;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.UserLogs;
using ProtonVPN.Core.Models;
using ProtonVPN.Core.Settings;
using ProtonVPN.Tests.Common.Breakpoints;
using RichardSzalay.MockHttp;

namespace ProtonVPN.Api.Tests.Handlers
{
    [TestClass]
    public class UnauthorizedResponseHandlerTest
    {
        private const string BASE_API_URL = "https://api.protonvpn.ch";
        private const string LOGICALS_ENDPOINT = "/logicals";
        private const string VPN_INFO_ENDPOINT = "/vpn";
        private const string PROFILES_ENDPOINT = "/profiles";
        private const string LOGICALS_API_URL = BASE_API_URL + LOGICALS_ENDPOINT;
        private const string VPN_INFO_API_URL = BASE_API_URL + VPN_INFO_ENDPOINT;
        private const string PROFILES_API_URL = BASE_API_URL + PROFILES_ENDPOINT;

        private const string ACCESS_TOKEN = "Access token";
        private const string REFRESH_TOKEN = "Refresh token";
        private const string NEW_ACCESS_TOKEN = "New access token";
        private const string NEW_REFRESH_TOKEN = "New refresh token";
        private const string AUTH_HEADER_KEY = "Authorization";
        private const string AUTH_HEADER_VALUE = "Bearer " + NEW_ACCESS_TOKEN;

        private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(5);
        private readonly Uri _baseAddress = new(BASE_API_URL);

        private ITokenClient _tokenClient;
        private IAppSettings _appSettings;
        private IUserStorage _userStorage;
        private ILogger _logger;
        private MockHttpMessageHandler _innerHandler;

        [TestInitialize]
        public void TestInitialize()
        {
            _tokenClient = Substitute.For<ITokenClient>();
            _tokenClient.RefreshTokenAsync(Arg.Any<CancellationToken>())
                .Returns(ApiResponseResult<RefreshTokenResponse>.Ok(new HttpResponseMessage(), new()));

            _appSettings = Substitute.For<IAppSettings>();
            _appSettings.AccessToken.Returns(ACCESS_TOKEN);
            _appSettings.RefreshToken.Returns(REFRESH_TOKEN);
            _appSettings.Uid.Returns("User ID");

            _userStorage = Substitute.For<IUserStorage>();
            _userStorage.GetUser().Returns(new User { Username = "test" });

            _logger = Substitute.For<ILogger>();

            _innerHandler = new();
        }

        [TestMethod]
        public async Task SendAsync_ShouldBe_InnerHandlerSendAsync()
        {
            // Arrange
            UnauthorizedResponseHandler handler = GetUnauthorizedResponseHandler(new MockOfHumanVerificationHandler(_innerHandler));
            HttpClient client = new(handler) { BaseAddress = _baseAddress };

            HttpResponseMessage response = new(HttpStatusCode.OK);
            _innerHandler.Expect(HttpMethod.Get, LOGICALS_API_URL)
                .Respond(req => response);

            HttpRequestMessage request = new(HttpMethod.Get, LOGICALS_ENDPOINT);

            // Act
            HttpResponseMessage result = await client.SendAsync(request);

            // Assert
            result.Should().BeSameAs(response);
            _innerHandler.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        public async Task SendAsync_ShouldCall_TokenClient_RefreshTokenAsync_WhenUnauthorized()
        {
            // Arrange
            UnauthorizedResponseHandler handler = GetUnauthorizedResponseHandler(new MockOfHumanVerificationHandler(_innerHandler));
            HttpClient client = new(handler) { BaseAddress = _baseAddress };

            _innerHandler.Expect(HttpMethod.Get, LOGICALS_API_URL)
                .Respond(HttpStatusCode.Unauthorized);

            // Act
            HttpRequestMessage request = new(HttpMethod.Get, LOGICALS_ENDPOINT);
            await client.SendAsync(request);

            // Assert
            await _tokenClient.Received(1).RefreshTokenAsync(Arg.Any<CancellationToken>());
        }

        [TestMethod]
        public async Task SendAsync_ShouldNotCall_TokenClient_RefreshTokenAsync_WhenRefreshTokenIsNull()
        {
            _appSettings.RefreshToken.Returns((string)null);

            await SendAsync_ShouldNotCall_TokenClient_RefreshTokenAsync_WhenCurrentTokenIsInvalid();
        }

        private async Task SendAsync_ShouldNotCall_TokenClient_RefreshTokenAsync_WhenCurrentTokenIsInvalid()
        {
            // Arrange
            UnauthorizedResponseHandler handler = GetUnauthorizedResponseHandler(new MockOfHumanVerificationHandler(_innerHandler));
            HttpClient client = new(handler) { BaseAddress = _baseAddress };

            _innerHandler.Expect(HttpMethod.Get, LOGICALS_API_URL)
                .Respond(HttpStatusCode.Unauthorized);

            // Act
            HttpRequestMessage request = new(HttpMethod.Get, LOGICALS_ENDPOINT);
            await client.SendAsync(request);

            // Assert
            await _tokenClient.Received(0).RefreshTokenAsync(Arg.Any<CancellationToken>());
        }

        [TestMethod]
        public async Task SendAsync_ShouldNotCall_TokenClient_RefreshTokenAsync_WhenRefreshTokenIsEmpty()
        {
            _appSettings.RefreshToken.Returns(string.Empty);

            await SendAsync_ShouldNotCall_TokenClient_RefreshTokenAsync_WhenCurrentTokenIsInvalid();
        }

        [TestMethod]
        public async Task SendAsync_ShouldNotCall_TokenClient_RefreshTokenAsync_WhenTokenUserIdIsNull()
        {
            _appSettings.Uid.Returns((string)null);

            await SendAsync_ShouldNotCall_TokenClient_RefreshTokenAsync_WhenCurrentTokenIsInvalid();
        }

        [TestMethod]
        public async Task SendAsync_ShouldNotCall_TokenClient_RefreshTokenAsync_WhenTokenUserIdIsEmpty()
        {
            _appSettings.Uid.Returns(string.Empty);

            await SendAsync_ShouldNotCall_TokenClient_RefreshTokenAsync_WhenCurrentTokenIsInvalid();
        }

        [TestMethod]
        public async Task SendAsync_ShouldNotCall_TokenClient_RefreshTokenAsync_WhenTokenClientRefreshTokenAsyncThrowsArgumentNullException()
        {
            // Arrange
            string exceptionMessage = "The RefreshToken in RefreshTokenData can't be null.";
            ArgumentNullException exception = new(exceptionMessage);
            _tokenClient.RefreshTokenAsync(Arg.Any<CancellationToken>())
                .Throws(exception);

            UnauthorizedResponseHandler handler = GetUnauthorizedResponseHandler(new MockOfHumanVerificationHandler(_innerHandler));
            HttpClient client = new(handler) { BaseAddress = _baseAddress };

            _innerHandler.Expect(HttpMethod.Get, LOGICALS_API_URL)
                .Respond(HttpStatusCode.Unauthorized);

            // Act
            HttpRequestMessage request = new(HttpMethod.Get, LOGICALS_ENDPOINT);
            await client.SendAsync(request);

            // Assert
            await _tokenClient.Received(1).RefreshTokenAsync(Arg.Any<CancellationToken>());
            _logger.Received(1).Error<UserLog>($"An error occurred when refreshing the auth token: {exceptionMessage}",
                null, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        }

        [TestMethod]
        public async Task SendAsync_ShouldRepeatRequest_WithRefreshedAccessToken()
        {
            // Arrange
            _tokenClient.RefreshTokenAsync(Arg.Any<CancellationToken>())
                .Returns(ApiResponseResult<RefreshTokenResponse>.Ok(
                    new HttpResponseMessage(), new() { AccessToken = NEW_ACCESS_TOKEN, RefreshToken = NEW_REFRESH_TOKEN }));
            UnauthorizedResponseHandler handler = GetUnauthorizedResponseHandler(new MockOfHumanVerificationHandler(_innerHandler));
            HttpClient client = new(handler) { BaseAddress = _baseAddress };

            _innerHandler.Expect(HttpMethod.Get, LOGICALS_API_URL)
                .Respond(HttpStatusCode.Unauthorized);
            _innerHandler.Expect(HttpMethod.Get, LOGICALS_API_URL)
                .WithHeaders(AUTH_HEADER_KEY, AUTH_HEADER_VALUE)
                .Respond(HttpStatusCode.OK);

            // Act
            HttpRequestMessage request = new(HttpMethod.Get, LOGICALS_ENDPOINT);
            await client.SendAsync(request);

            // Assert
            _innerHandler.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        public async Task SendAsync_ShouldSet_TokenStorage_Tokens()
        {
            // Arrange
            _tokenClient.RefreshTokenAsync(Arg.Any<CancellationToken>())
                .Returns(ApiResponseResult<RefreshTokenResponse>.Ok(
                    new HttpResponseMessage(), new() { AccessToken = NEW_ACCESS_TOKEN, RefreshToken = NEW_REFRESH_TOKEN }));
            UnauthorizedResponseHandler handler = GetUnauthorizedResponseHandler(new MockOfHumanVerificationHandler(_innerHandler));
            HttpClient client = new(handler) { BaseAddress = _baseAddress };

            _innerHandler.Expect(HttpMethod.Get, LOGICALS_API_URL)
                .Respond(HttpStatusCode.Unauthorized);

            // Act
            HttpRequestMessage request = new(HttpMethod.Get, LOGICALS_ENDPOINT);
            await client.SendAsync(request);

            // Assert
            _appSettings.AccessToken.Should().Be(NEW_ACCESS_TOKEN);
            _appSettings.RefreshToken.Should().Be(NEW_REFRESH_TOKEN);
        }

        [TestMethod]
        public async Task SendAsync_ShouldBe_InnerHandlerSendAsync_WhenRepeatedRequest()
        {
            // Arrange
            UnauthorizedResponseHandler handler = GetUnauthorizedResponseHandler(new MockOfHumanVerificationHandler(_innerHandler));
            HttpClient client = new(handler) { BaseAddress = _baseAddress };

            HttpResponseMessage response = new(HttpStatusCode.OK);
            _innerHandler.Expect(HttpMethod.Get, LOGICALS_API_URL)
                .Respond(HttpStatusCode.Unauthorized);
            _innerHandler.Expect(HttpMethod.Get, LOGICALS_API_URL)
                .Respond(req => response);

            // Act
            HttpRequestMessage request = new(HttpMethod.Get, LOGICALS_ENDPOINT);
            HttpResponseMessage result = await client.SendAsync(request);

            // Assert
            result.Should().BeSameAs(response);
        }

        [TestMethod]
        public async Task SendAsync_ShouldRaise_SessionExpired_WhenRefreshFailed()
        {
            // Arrange
            _tokenClient.RefreshTokenAsync(Arg.Any<CancellationToken>())
                .Returns(ApiResponseResult<RefreshTokenResponse>.Fail(new HttpResponseMessage(HttpStatusCode.BadRequest), "Refresh failed"));
            UnauthorizedResponseHandler handler = GetUnauthorizedResponseHandler(new MockOfHumanVerificationHandler(_innerHandler));
            HttpClient client = new(handler) { BaseAddress = _baseAddress };

            _innerHandler.Expect(HttpMethod.Get, LOGICALS_API_URL)
                .Respond(HttpStatusCode.Unauthorized);

            using (IMonitor<UnauthorizedResponseHandler> monitoredSubject = handler.Monitor())
            {
                // Act
                HttpRequestMessage request = new(HttpMethod.Get, LOGICALS_ENDPOINT);
                await client.SendAsync(request);

                // Assert
                monitoredSubject.Should().Raise(nameof(UnauthorizedResponseHandler.SessionExpired));
                _innerHandler.VerifyNoOutstandingExpectation();
            }
        }

        [TestMethod]
        public async Task SendAsync_ShouldLimit_RefreshRequests_ToOne()
        {
            // Arrange
            BreakpointHandler breakpointHandler = new() { InnerHandler = _innerHandler };
            Breakpoint requestBreakpoint = breakpointHandler.Breakpoint;
            BreakpointTokenClient breakpointTokenClient = new(_tokenClient);
            Breakpoint tokenClientBreakpoint = breakpointTokenClient.Breakpoint;
            MockOfHumanVerificationHandler humanVerificationHandler =
                new() { InnerHandler = breakpointHandler };
            UnauthorizedResponseHandler handler = new(breakpointTokenClient, _appSettings, _userStorage, _logger) 
                { InnerHandler = humanVerificationHandler };
            HttpClient client = new(handler) { BaseAddress = _baseAddress };

            _tokenClient.RefreshTokenAsync(Arg.Any<CancellationToken>())
                .Returns(ApiResponseResult<RefreshTokenResponse>.Ok(
                    new HttpResponseMessage(),
                    new() { AccessToken = NEW_ACCESS_TOKEN, RefreshToken = NEW_REFRESH_TOKEN }));

            HttpResponseMessage response = new(HttpStatusCode.OK);

            // Act
            Task task1 = Task.CompletedTask;
            Task task2 = Task.CompletedTask;
            try
            {
                // Sending first request and pause it
                HttpRequestMessage request1 = new(HttpMethod.Get, VPN_INFO_ENDPOINT);
                task1 = client.SendAsync(request1);
                BreakpointHit request1Hit = await requestBreakpoint.WaitForHit().TimeoutAfter(TestTimeout);

                // Sending second request and pause it
                HttpRequestMessage request2 = new(HttpMethod.Get, VPN_INFO_ENDPOINT);
                task2 = client.SendAsync(request2);
                BreakpointHit request2Hit = await requestBreakpoint.WaitForHit().TimeoutAfter(TestTimeout);

                // Continue first and second requests and get Unauthorized
                _innerHandler.Expect(HttpMethod.Get, VPN_INFO_API_URL)
                    .Respond(HttpStatusCode.Unauthorized);
                _innerHandler.Expect(HttpMethod.Get, VPN_INFO_API_URL)
                    .Respond(HttpStatusCode.Unauthorized);
                request1Hit.Continue();
                request2Hit.Continue();

                // Token refresh
                await tokenClientBreakpoint.WaitForHitAndContinue().TimeoutAfter(TestTimeout);

                // First and second requests retried with new access token
                _innerHandler.Expect(HttpMethod.Get, VPN_INFO_API_URL)
                    .WithHeaders(AUTH_HEADER_KEY, AUTH_HEADER_VALUE)
                    .Respond(req => response);
                _innerHandler.Expect(HttpMethod.Get, VPN_INFO_API_URL)
                    .WithHeaders(AUTH_HEADER_KEY, AUTH_HEADER_VALUE)
                    .Respond(req => response);
                await requestBreakpoint.WaitForHitAndContinue().TimeoutAfter(TestTimeout);
                await requestBreakpoint.WaitForHitAndContinue().TimeoutAfter(TestTimeout);
            }
            finally
            {
                await task1.TimeoutAfter(TestTimeout);
                await task2.TimeoutAfter(TestTimeout);
            }

            // Assert
            await _tokenClient.Received(1).RefreshTokenAsync(Arg.Any<CancellationToken>());
            _innerHandler.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        public async Task SendAsync_ShouldSuppressRequest_WhenRefreshingTokens()
        {
            // Arrange
            BreakpointHandler breakpointHandler = new() { InnerHandler = _innerHandler };
            Breakpoint requestBreakpoint = breakpointHandler.Breakpoint;
            BreakpointTokenClient breakpointTokenClient = new(_tokenClient);
            Breakpoint tokenClientBreakpoint = breakpointTokenClient.Breakpoint;
            UnauthorizedResponseHandler handler =
                GetUnauthorizedResponseHandlerWithBreakpoint(breakpointHandler, breakpointTokenClient);
            HttpClient client = new(handler) { BaseAddress = _baseAddress };

            _tokenClient.RefreshTokenAsync(Arg.Any<CancellationToken>())
                .Returns(ApiResponseResult<RefreshTokenResponse>.Ok(
                    new HttpResponseMessage(),
                    new() { AccessToken = NEW_ACCESS_TOKEN, RefreshToken = NEW_REFRESH_TOKEN }));

            HttpResponseMessage response = new(HttpStatusCode.OK);

            // Act
            Task task1 = Task.CompletedTask;
            Task task2 = Task.CompletedTask;
            try
            {
                // Sending first request
                HttpRequestMessage request1 = new(HttpMethod.Get, VPN_INFO_ENDPOINT);
                task1 = client.SendAsync(request1);

                // First request continues and gets Unauthorized
                _innerHandler.Expect(HttpMethod.Get, VPN_INFO_API_URL)
                    .Respond(HttpStatusCode.Unauthorized);
                await requestBreakpoint.WaitForHitAndContinue().TimeoutAfter(TestTimeout);

                // First request initiated token refresh, pausing it
                BreakpointHit tokenClientHit = await tokenClientBreakpoint.WaitForHit().TimeoutAfter(TestTimeout);

                // Sending second request, it is waiting for token refresh to finish
                HttpRequestMessage request2 = new(HttpMethod.Get, VPN_INFO_ENDPOINT);
                task2 = client.SendAsync(request2);

                // Continue token refresh
                tokenClientHit.Continue();

                // First and second requests retried with new access token
                _innerHandler.Expect(HttpMethod.Get, VPN_INFO_API_URL)
                    .WithHeaders(AUTH_HEADER_KEY, AUTH_HEADER_VALUE)
                    .Respond(req => response);
                _innerHandler.Expect(HttpMethod.Get, VPN_INFO_API_URL)
                    .WithHeaders(AUTH_HEADER_KEY, AUTH_HEADER_VALUE)
                    .Respond(req => response);
                await requestBreakpoint.WaitForHitAndContinue().TimeoutAfter(TestTimeout);
                await requestBreakpoint.WaitForHitAndContinue().TimeoutAfter(TestTimeout);
            }
            finally
            {
                await task1.TimeoutAfter(TestTimeout);
                await task2.TimeoutAfter(TestTimeout);
            }

            // Assert
            await _tokenClient.Received(1).RefreshTokenAsync(Arg.Any<CancellationToken>());
            _innerHandler.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        public async Task SendAsync_ShouldRetryWithNewToken_WhenRefreshedWhileRequesting()
        {
            // Arrange
            BreakpointHandler breakpointHandler = new() { InnerHandler = _innerHandler };
            Breakpoint requestBreakpoint = breakpointHandler.Breakpoint;
            BreakpointTokenClient breakpointTokenClient = new(_tokenClient);
            Breakpoint tokenClientBreakpoint = breakpointTokenClient.Breakpoint;
            UnauthorizedResponseHandler handler =
                GetUnauthorizedResponseHandlerWithBreakpoint(breakpointHandler, breakpointTokenClient);
            HttpClient client = new(handler) { BaseAddress = _baseAddress };

            _tokenClient.RefreshTokenAsync(Arg.Any<CancellationToken>())
                .Returns(ApiResponseResult<RefreshTokenResponse>.Ok(
                    new HttpResponseMessage(),
                    new() { AccessToken = NEW_ACCESS_TOKEN, RefreshToken = NEW_REFRESH_TOKEN }));

            HttpResponseMessage response = new(HttpStatusCode.OK);

            // Act
            Task task1 = Task.CompletedTask;
            Task task2 = Task.CompletedTask;
            try
            {
                // Sending first request and pausing it
                HttpRequestMessage request1 = new(HttpMethod.Get, VPN_INFO_ENDPOINT);
                task1 = client.SendAsync(request1);
                BreakpointHit request1Hit = await requestBreakpoint.WaitForHit().TimeoutAfter(TestTimeout);

                // Sending second request and pausing it
                HttpRequestMessage request2 = new(HttpMethod.Get, PROFILES_ENDPOINT);
                task2 = client.SendAsync(request2);
                BreakpointHit request2Hit = await requestBreakpoint.WaitForHit().TimeoutAfter(TestTimeout);

                // Continue first request and get Unauthorized
                _innerHandler.Expect(HttpMethod.Get, VPN_INFO_API_URL)
                    .Respond(HttpStatusCode.Unauthorized);
                request1Hit.Continue();

                // First request initiated token refresh
                await tokenClientBreakpoint.WaitForHitAndContinue().TimeoutAfter(TestTimeout);

                // First request retried with new tokens
                request1Hit = await requestBreakpoint.WaitForHit().TimeoutAfter(TestTimeout);
                _innerHandler.Expect(HttpMethod.Get, VPN_INFO_API_URL)
                    .WithHeaders(AUTH_HEADER_KEY, AUTH_HEADER_VALUE)
                    .Respond(req => response);
                request1Hit.Continue();
                await task1.TimeoutAfter(TestTimeout);

                // Second request continues and gets Unauthorized
                _innerHandler.Expect(HttpMethod.Get, PROFILES_API_URL)
                    .Respond(HttpStatusCode.Unauthorized);
                request2Hit.Continue();

                // Second request retried with new access token
                request2Hit = await requestBreakpoint.WaitForHit().TimeoutAfter(TestTimeout);
                _innerHandler.Expect(HttpMethod.Get, PROFILES_API_URL)
                    .WithHeaders(AUTH_HEADER_KEY, AUTH_HEADER_VALUE)
                    .Respond(req => response);
                request2Hit.Continue();
            }
            finally
            {
                await task1.TimeoutAfter(TestTimeout);
                await task2.TimeoutAfter(TestTimeout);
            }

            // Assert
            await _tokenClient.Received(1).RefreshTokenAsync(Arg.Any<CancellationToken>());
            _innerHandler.VerifyNoOutstandingExpectation();
        }

        private UnauthorizedResponseHandler GetUnauthorizedResponseHandler(HumanVerificationHandlerBase handler)
        {
            return new(_tokenClient, _appSettings, _userStorage, _logger) { InnerHandler = handler };
        }

        private UnauthorizedResponseHandler GetUnauthorizedResponseHandlerWithBreakpoint(BreakpointHandler breakpointHandler, ITokenClient tokenClient)
        {
            MockOfHumanVerificationHandler handler = new() { InnerHandler = breakpointHandler };
            return new(tokenClient, _appSettings, _userStorage, _logger) { InnerHandler = handler };
        }

        #region Helpers

        private class BreakpointTokenClient : ITokenClient
        {
            private readonly ITokenClient _origin;
            public BreakpointTokenClient(ITokenClient origin)
            {
                _origin = origin;
                Breakpoint = new();
            }

            public Breakpoint Breakpoint { get; }

            public event EventHandler<ActionableFailureApiResultEventArgs> OnActionableFailureResult;

            public async Task<ApiResponseResult<RefreshTokenResponse>> RefreshTokenAsync(CancellationToken token)
            {
                await Breakpoint.Hit().WaitForContinue();
                return await _origin.RefreshTokenAsync(token);
            }
        }

        private class BreakpointHandler : DelegatingHandler
        {
            public BreakpointHandler()
            {
                Breakpoint = new();
            }

            public Breakpoint Breakpoint { get; }

            protected override async Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                await Breakpoint.Hit().WaitForContinue();
                return await base.SendAsync(request, cancellationToken);
            }

            protected override void Dispose(bool disposing)
            {
                Breakpoint.Dispose();
            }
        }

        #endregion
    }
}