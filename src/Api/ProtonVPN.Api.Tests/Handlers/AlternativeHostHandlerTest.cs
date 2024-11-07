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
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Polly.Timeout;
using ProtonVPN.Api.Contracts.Exceptions;
using ProtonVPN.Api.Handlers;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.GuestHole;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Configurations.Contracts.Entities;
using ProtonVPN.Dns.Contracts;
using ProtonVPN.Dns.Contracts.AlternativeRouting;
using ProtonVPN.Dns.Contracts.Exceptions;
using ProtonVPN.Logging.Contracts;
using RichardSzalay.MockHttp;

namespace ProtonVPN.Api.Tests.Handlers;

[TestClass]
public class AlternativeHostHandlerTest
{
    private const string HTTPS = "https://";
    private const string HOST = "api.protonvpn.ch";
    private const string PATH = "/auth";
    private const string REQUEST_URL = HTTPS + HOST + PATH;

    private const string CONFIG_API_HOST = "configapi.protonvpn.ch";
    private const string CONFIG_API_BASE_URL = HTTPS + CONFIG_API_HOST + "/";
    private const string CONFIG_API_PING_URL = CONFIG_API_BASE_URL + AlternativeHostHandler.API_PING_TEST_PATH;

    private const string IP_ADDRESS_1 = "192.168.1.1";
    private const string IP_ADDRESS_2 = "192.168.2.2";
    private const string IP_ADDRESS_3 = "192.168.3.3";

    private const string ALTERNATIVE_URL_1 = HTTPS + IP_ADDRESS_1 + PATH;
    private const string ALTERNATIVE_URL_2 = HTTPS + IP_ADDRESS_2 + PATH;
    private const string ALTERNATIVE_URL_3 = HTTPS + IP_ADDRESS_3 + PATH;

    private const string ALTERNATIVE_HOST_1 = "abc.protonvpn.com";
    private const string ALTERNATIVE_HOST_2 = "def.protonvpn.com";
    private const string ALTERNATIVE_HOST_3 = "ghi.protonvpn.com";

    private const string UNIQUE_SESSION_ID = "SessionID";

    private readonly static TimeSpan ALTERNATIVE_ROUTING_CHECK_INTERVAL = TimeSpan.FromMinutes(30);

    private ILogger _logger;
    private IDnsManager _dnsManager;
    private IAlternativeRoutingHostGenerator _alternativeRoutingHostGenerator;
    private IAlternativeHostsManager _alternativeHostsManager;
    private ISettings _settings;
    private IGuestHoleManager _guestHoleManager;
    private IConfiguration _configuration;
    private MockHttpMessageHandler _mockHttpMessageHandler;
    private AlternativeHostHandler _alternativeHostHandler;
    private HttpClient _httpClient;
    private int _originalRequestCount;

    [TestInitialize]
    public void TestInitialize()
    {
        _logger = Substitute.For<ILogger>();
        _dnsManager = Substitute.For<IDnsManager>();
        _alternativeRoutingHostGenerator = Substitute.For<IAlternativeRoutingHostGenerator>();
        _alternativeHostsManager = Substitute.For<IAlternativeHostsManager>();
        _settings = Substitute.For<ISettings>();
        _settings.UniqueSessionId.Returns(UNIQUE_SESSION_ID);
        _guestHoleManager = Substitute.For<IGuestHoleManager>();
        _configuration = CreateConfiguration();
        _mockHttpMessageHandler = new MockHttpMessageHandler();
        _alternativeHostHandler = new(_logger, _dnsManager, _alternativeRoutingHostGenerator,
            _alternativeHostsManager, _settings, _configuration)
        { InnerHandler = _mockHttpMessageHandler };
        _httpClient = new(_alternativeHostHandler);
        _originalRequestCount = 0;
    }

    private IConfiguration CreateConfiguration()
    {
        IUrlsConfiguration urlsConfig = Substitute.For<IUrlsConfiguration>();
        urlsConfig.ApiUrl.Returns(CONFIG_API_BASE_URL);
        IConfiguration config = Substitute.For<IConfiguration>();
        config.Urls.Returns(urlsConfig);
        config.AlternativeRoutingCheckInterval.Returns(ALTERNATIVE_ROUTING_CHECK_INTERVAL);
        return config;
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _logger = null;
        _dnsManager = null;
        _alternativeRoutingHostGenerator = null;
        _alternativeHostsManager = null;
        _settings = null;
        _guestHoleManager = null;
        _configuration = null;
        _mockHttpMessageHandler = null;
        _alternativeHostHandler = null;
        _httpClient = null;
        _originalRequestCount = 0;
    }

    [TestMethod]
    public async Task Test_Success()
    {
        // Arrange
        HttpRequestMessage request = new(HttpMethod.Get, REQUEST_URL);
        MockedRequest mockedRequest = _mockHttpMessageHandler
            .When(REQUEST_URL)
            .Respond(_ => new(HttpStatusCode.OK));

        // Act
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(REQUEST_URL, request.RequestUri.ToString());
        _mockHttpMessageHandler.GetMatchCount(mockedRequest).Should().Be(1);
        await NoDependenciesAreCalledAsync();
    }

    private async Task NoDependenciesAreCalledAsync()
    {
        await _dnsManager.Received(0).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(0).ResolveWithoutCacheAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        _dnsManager.Received(0).GetFromCache(Arg.Any<string>());
        _alternativeRoutingHostGenerator.Received(0).Generate(Arg.Any<string>());
        await _alternativeHostsManager.Received(0).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task Test_WithUnusualException()
    {
        // Arrange
        HttpRequestMessage request = new(HttpMethod.Get, REQUEST_URL);
        MockedRequest mockedRequest = _mockHttpMessageHandler
            .When(REQUEST_URL)
            .Respond(_ => throw new ArgumentNullException());

        // Act
        Exception exception = await Assert.ThrowsExceptionAsync<ArgumentNullException>(
            async () => await _httpClient.SendAsync(request));

        // Assert
        Assert.AreEqual(REQUEST_URL, request.RequestUri.ToString());
        _mockHttpMessageHandler.GetMatchCount(mockedRequest).Should().Be(1);
        await NoDependenciesAreCalledAsync();
    }

    [TestMethod]
    [DataRow(ConnectionStatus.Disconnected, true, true)]
    [DataRow(ConnectionStatus.Disconnected, false, false)]
    [DataRow(ConnectionStatus.Connecting, false, true)]
    [DataRow(ConnectionStatus.Connected, false, true)]
    public async Task Test_WithBlockingExceptionButAlternativeRoutingNotAllowed(
        ConnectionStatus connectionStatus, bool isGuestHoleActive, bool isAlternativeRoutingEnabled)
    {
        // Arrange
        SetConnectionStatus(connectionStatus);
        SetGuestHoleState(isGuestHoleActive);
        _settings.IsAlternativeRoutingEnabled = isAlternativeRoutingEnabled;
        HttpRequestMessage request = new(HttpMethod.Get, REQUEST_URL);
        MockedRequest mockedRequest = _mockHttpMessageHandler
                                      .When(REQUEST_URL)
                                      .Respond(_ => throw new TimeoutException());

        // Act
        Exception exception = await Assert.ThrowsExceptionAsync<TimeoutException>(
            async () => await _httpClient.SendAsync(request));

        // Assert
        Assert.AreEqual(REQUEST_URL, request.RequestUri.ToString());
        _mockHttpMessageHandler.GetMatchCount(mockedRequest).Should().Be(1);
        await NoDependenciesAreCalledAsync();
    }

    [TestMethod]
    [DataRow(typeof(TimeoutException))]
    [DataRow(typeof(TimeoutRejectedException))]
    [DataRow(typeof(DnsException))]
    [DataRow(typeof(AuthenticationException))]
    public async Task Test_NoAlternativeHosts(Type exceptionType)
    {
        // Arrange
        SetConnectionStatus(ConnectionStatus.Disconnected);
        _settings.IsAlternativeRoutingEnabled = true;
        HttpRequestMessage request = new(HttpMethod.Get, REQUEST_URL);
        MockedRequest mockedRequest = _mockHttpMessageHandler
                                      .When(REQUEST_URL)
                                      .Respond(_ => throw CreateException(exceptionType));

        // Act
        Exception exception = await Assert.ThrowsExceptionAsync<AlternativeRoutingException>(
            async () => await _httpClient.SendAsync(request));

        // Assert
        Assert.AreEqual(REQUEST_URL, request.RequestUri.ToString());
        _mockHttpMessageHandler.GetMatchCount(mockedRequest).Should().Be(1);
        await _dnsManager.Received(0).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(CONFIG_API_HOST, Arg.Any<CancellationToken>());
        _dnsManager.Received(1).GetFromCache(Arg.Any<string>());
        _dnsManager.Received(1).GetFromCache(CONFIG_API_HOST);
        _alternativeRoutingHostGenerator.Received(1).Generate(Arg.Any<string>());
        await _alternativeHostsManager.Received(1).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    private Exception CreateException(Type exceptionType)
    {
        return (Exception)Activator.CreateInstance(exceptionType, "Test exception message");
    }

    private void SetConnectionStatus(ConnectionStatus status)
    {
        _alternativeHostHandler.Receive(new ConnectionStatusChangedMessage(status));
    }

    [TestMethod]
    [DataRow(typeof(TimeoutException))]
    [DataRow(typeof(DnsException))]
    [DataRow(typeof(TimeoutRejectedException))]
    [DataRow(typeof(AuthenticationException))]
    public async Task Test_AlternativeHostsHaveNoIpAddresses(Type exceptionType)
    {
        // Arrange
        SetConnectionStatus(ConnectionStatus.Disconnected);
        _settings.IsAlternativeRoutingEnabled = true;
        _alternativeHostsManager.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                                .Returns(CreateAlternativeHostsList());
        HttpRequestMessage request = new(HttpMethod.Get, REQUEST_URL);
        MockedRequest mockedRequest = _mockHttpMessageHandler
                                      .When(REQUEST_URL)
                                      .Respond(_ => throw CreateException(exceptionType));

        // Act
        Exception exception = await Assert.ThrowsExceptionAsync<AlternativeRoutingException>(
            async () => await _httpClient.SendAsync(request));

        // Assert
        Assert.AreEqual(REQUEST_URL, request.RequestUri.ToString());
        _mockHttpMessageHandler.GetMatchCount(mockedRequest).Should().Be(1);
        await _dnsManager.Received(3).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        foreach (string alternativeHosts in CreateAlternativeHostsList())
        {
            await _dnsManager.Received(1).GetAsync(alternativeHosts, Arg.Any<CancellationToken>());
        }
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(CONFIG_API_HOST, Arg.Any<CancellationToken>());
        _dnsManager.Received(1).GetFromCache(Arg.Any<string>());
        _dnsManager.Received(1).GetFromCache(CONFIG_API_HOST);
        _alternativeRoutingHostGenerator.Received(1).Generate(Arg.Any<string>());
        await _alternativeHostsManager.Received(1).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    private IList<string> CreateAlternativeHostsList()
    {
        return new List<string>() { ALTERNATIVE_HOST_1, ALTERNATIVE_HOST_2, ALTERNATIVE_HOST_3 };
    }

    [TestMethod]
    [DataRow(typeof(TimeoutException))]
    [DataRow(typeof(DnsException))]
    [DataRow(typeof(TimeoutRejectedException))]
    [DataRow(typeof(AuthenticationException))]
    public async Task Test_LastIpAddressOfLastAlternativeHostWorks(Type exceptionType)
    {
        // Arrange
        SetConnectionStatus(ConnectionStatus.Disconnected);
        _settings.IsAlternativeRoutingEnabled = true;
        _alternativeHostsManager.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                                .Returns(CreateAlternativeHostsList());
        _dnsManager.GetAsync(ALTERNATIVE_HOST_3, Arg.Any<CancellationToken>())
                                .Returns(CreateIpAddressList());
        HttpRequestMessage request = new(HttpMethod.Get, REQUEST_URL);
        MockedRequest mockedRequestOriginal = _mockHttpMessageHandler
                                              .When(REQUEST_URL)
                                              .Respond(_ => throw CreateException(exceptionType));
        MockedRequest mockedRequestError1 = _mockHttpMessageHandler
                                            .When(ALTERNATIVE_URL_1)
                                            .Respond(_ => throw CreateException(exceptionType));
        MockedRequest mockedRequestError2 = _mockHttpMessageHandler
                                            .When(ALTERNATIVE_URL_2)
                                            .Respond(_ => throw CreateException(exceptionType));
        MockedRequest mockedRequestSuccess = _mockHttpMessageHandler
                                             .When(ALTERNATIVE_URL_3)
                                             .Respond(_ => new(HttpStatusCode.OK));

        // Act
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(REQUEST_URL, request.RequestUri.ToString());
        _mockHttpMessageHandler.GetMatchCount(mockedRequestOriginal).Should().Be(1);
        _mockHttpMessageHandler.GetMatchCount(mockedRequestError1).Should().Be(1);
        _mockHttpMessageHandler.GetMatchCount(mockedRequestError2).Should().Be(1);
        _mockHttpMessageHandler.GetMatchCount(mockedRequestSuccess).Should().Be(1);
        await _dnsManager.Received(3).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        foreach (string alternativeHosts in CreateAlternativeHostsList())
        {
            await _dnsManager.Received(1).GetAsync(alternativeHosts, Arg.Any<CancellationToken>());
        }
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(CONFIG_API_HOST, Arg.Any<CancellationToken>());
        _dnsManager.Received(1).GetFromCache(Arg.Any<string>());
        _dnsManager.Received(1).GetFromCache(CONFIG_API_HOST);
        _alternativeRoutingHostGenerator.Received(1).Generate(Arg.Any<string>());
        await _alternativeHostsManager.Received(1).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    private IList<IpAddress> CreateIpAddressList()
    {
        return new List<IpAddress>()
        {
            new(IPAddress.Parse(IP_ADDRESS_1)),
            new(IPAddress.Parse(IP_ADDRESS_2)),
            new(IPAddress.Parse(IP_ADDRESS_3)),
        };
    }

    [TestMethod]
    [DataRow(typeof(TimeoutException))]
    [DataRow(typeof(DnsException))]
    [DataRow(typeof(TimeoutRejectedException))]
    [DataRow(typeof(AuthenticationException))]
    public async Task Test_AllIpAddressesOfAllAlternativeHostsFail(Type exceptionType)
    {
        // Arrange
        SetConnectionStatus(ConnectionStatus.Disconnected);
        _settings.IsAlternativeRoutingEnabled = true;
        _alternativeHostsManager.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                                .Returns(CreateAlternativeHostsList());
        _dnsManager.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                                .Returns(CreateIpAddressList());
        HttpRequestMessage request = new(HttpMethod.Get, REQUEST_URL);
        MockedRequest mockedRequestOriginal = _mockHttpMessageHandler
                                              .When(REQUEST_URL)
                                              .Respond(_ => throw CreateException(exceptionType));
        MockedRequest mockedRequestError1 = _mockHttpMessageHandler
                                            .When(ALTERNATIVE_URL_1)
                                            .Respond(_ => throw CreateException(exceptionType));
        MockedRequest mockedRequestError2 = _mockHttpMessageHandler
                                            .When(ALTERNATIVE_URL_2)
                                            .Respond(_ => throw CreateException(exceptionType));
        MockedRequest mockedRequestError3 = _mockHttpMessageHandler
                                            .When(ALTERNATIVE_URL_3)
                                            .Respond(_ => throw CreateException(exceptionType));

        // Act
        Exception exception = await Assert.ThrowsExceptionAsync<AlternativeRoutingException>(
            async () => await _httpClient.SendAsync(request));

        // Assert
        Assert.AreEqual(REQUEST_URL, request.RequestUri.ToString());
        _mockHttpMessageHandler.GetMatchCount(mockedRequestOriginal).Should().Be(1);
        _mockHttpMessageHandler.GetMatchCount(mockedRequestError1).Should().Be(3);
        _mockHttpMessageHandler.GetMatchCount(mockedRequestError2).Should().Be(3);
        _mockHttpMessageHandler.GetMatchCount(mockedRequestError3).Should().Be(3);
        await _dnsManager.Received(3).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        foreach (string alternativeHosts in CreateAlternativeHostsList())
        {
            await _dnsManager.Received(1).GetAsync(alternativeHosts, Arg.Any<CancellationToken>());
        }
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(CONFIG_API_HOST, Arg.Any<CancellationToken>());
        _dnsManager.Received(1).GetFromCache(Arg.Any<string>());
        _dnsManager.Received(1).GetFromCache(CONFIG_API_HOST);
        _alternativeRoutingHostGenerator.Received(1).Generate(Arg.Any<string>());
        await _alternativeHostsManager.Received(1).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    [DataRow(typeof(TimeoutException))]
    [DataRow(typeof(DnsException))]
    [DataRow(typeof(TimeoutRejectedException))]
    [DataRow(typeof(AuthenticationException))]
    public async Task Test_AllIpAddressesOfAllAlternativeHostsFailWithErrorCode(Type exceptionType)
    {
        // Arrange
        SetConnectionStatus(ConnectionStatus.Disconnected);
        _settings.IsAlternativeRoutingEnabled = true;
        _alternativeHostsManager.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                                .Returns(CreateAlternativeHostsList());
        _dnsManager.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                                .Returns(CreateIpAddressList());
        HttpRequestMessage request = new(HttpMethod.Get, REQUEST_URL);
        MockedRequest mockedRequestOriginal = _mockHttpMessageHandler
                                              .When(REQUEST_URL)
                                              .Respond(_ => throw CreateException(exceptionType));
        MockedRequest mockedRequestError1 = _mockHttpMessageHandler
                                            .When(ALTERNATIVE_URL_1)
                                            .Respond(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        MockedRequest mockedRequestError2 = _mockHttpMessageHandler
                                            .When(ALTERNATIVE_URL_2)
                                            .Respond(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        MockedRequest mockedRequestError3 = _mockHttpMessageHandler
                                            .When(ALTERNATIVE_URL_3)
                                            .Respond(_ => new HttpResponseMessage(HttpStatusCode.NotFound));

        // Act
        Exception exception = await Assert.ThrowsExceptionAsync<AlternativeRoutingException>(
            async () => await _httpClient.SendAsync(request));

        // Assert
        Assert.AreEqual(REQUEST_URL, request.RequestUri.ToString());
        _mockHttpMessageHandler.GetMatchCount(mockedRequestOriginal).Should().Be(1);
        _mockHttpMessageHandler.GetMatchCount(mockedRequestError1).Should().Be(3);
        _mockHttpMessageHandler.GetMatchCount(mockedRequestError2).Should().Be(3);
        _mockHttpMessageHandler.GetMatchCount(mockedRequestError3).Should().Be(3);
        await _dnsManager.Received(3).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        foreach (string alternativeHosts in CreateAlternativeHostsList())
        {
            await _dnsManager.Received(1).GetAsync(alternativeHosts, Arg.Any<CancellationToken>());
        }
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(CONFIG_API_HOST, Arg.Any<CancellationToken>());
        _dnsManager.Received(1).GetFromCache(Arg.Any<string>());
        _dnsManager.Received(1).GetFromCache(CONFIG_API_HOST);
        _alternativeRoutingHostGenerator.Received(1).Generate(Arg.Any<string>());
        await _alternativeHostsManager.Received(1).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    [DataRow(typeof(TimeoutException))]
    [DataRow(typeof(DnsException))]
    [DataRow(typeof(TimeoutRejectedException))]
    [DataRow(typeof(AuthenticationException))]
    public async Task Test_CacheExists_SucceedsOnSecondAttempt(Type exceptionType)
    {
        // Arrange
        SetConnectionStatus(ConnectionStatus.Disconnected);
        _settings.IsAlternativeRoutingEnabled = true;
        _dnsManager.GetFromCache(CONFIG_API_HOST)
                   .Returns(CreateIpAddressList());
        HttpRequestMessage request = new(HttpMethod.Get, REQUEST_URL);
        MockedRequest mockedRequestOriginal = _mockHttpMessageHandler
                                              .When(REQUEST_URL)
                                              .Respond(_ => ThrowOnFirstHttpResponseMessageThenSucceed(exceptionType));
        MockedRequest mockedPingRequest = _mockHttpMessageHandler
                                          .When(CONFIG_API_PING_URL)
                                          .Respond(_ => new(HttpStatusCode.OK));

        // Act
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(REQUEST_URL, request.RequestUri.ToString());
        _mockHttpMessageHandler.GetMatchCount(mockedRequestOriginal).Should().Be(2);
        _mockHttpMessageHandler.GetMatchCount(mockedPingRequest).Should().Be(1);
        await _dnsManager.Received(0).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(CONFIG_API_HOST, Arg.Any<CancellationToken>());
        _dnsManager.Received(1).GetFromCache(Arg.Any<string>());
        _dnsManager.Received(1).GetFromCache(CONFIG_API_HOST);
        _alternativeRoutingHostGenerator.Received(0).Generate(Arg.Any<string>());
        await _alternativeHostsManager.Received(0).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    private HttpResponseMessage ThrowOnFirstHttpResponseMessageThenSucceed(Type exceptionType)
    {
        _originalRequestCount++;
        return _originalRequestCount <= 1
            ? throw CreateException(exceptionType)
            : new(HttpStatusCode.OK);
    }

    [TestMethod]
    [DataRow(typeof(TimeoutException))]
    [DataRow(typeof(DnsException))]
    [DataRow(typeof(TimeoutRejectedException))]
    [DataRow(typeof(AuthenticationException))]
    public async Task Test_CacheExists_PingSucceeds_FailsOriginalRequest_AlternativeRoutingFails(Type exceptionType)
    {
        // Arrange
        SetConnectionStatus(ConnectionStatus.Disconnected);
        _settings.IsAlternativeRoutingEnabled = true;
        _dnsManager.GetFromCache(CONFIG_API_HOST)
                   .Returns(CreateIpAddressList());
        HttpRequestMessage request = new(HttpMethod.Get, REQUEST_URL);
        MockedRequest mockedRequestOriginal = _mockHttpMessageHandler
                                              .When(REQUEST_URL)
                                              .Respond(_ => throw CreateException(exceptionType));
        MockedRequest mockedPingRequest = _mockHttpMessageHandler
                                          .When(CONFIG_API_PING_URL)
                                          .Respond(_ => new(HttpStatusCode.OK));

        // Act
        Exception exception = await Assert.ThrowsExceptionAsync<AlternativeRoutingException>(
            async () => await _httpClient.SendAsync(request));

        // Assert
        Assert.AreEqual(REQUEST_URL, request.RequestUri.ToString());
        _mockHttpMessageHandler.GetMatchCount(mockedRequestOriginal).Should().Be(2);
        _mockHttpMessageHandler.GetMatchCount(mockedPingRequest).Should().Be(1);
        await _dnsManager.Received(0).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(CONFIG_API_HOST, Arg.Any<CancellationToken>());
        _dnsManager.Received(1).GetFromCache(Arg.Any<string>());
        _dnsManager.Received(1).GetFromCache(CONFIG_API_HOST);
        _alternativeRoutingHostGenerator.Received(1).Generate(Arg.Any<string>());
        await _alternativeHostsManager.Received(1).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    [DataRow(typeof(TimeoutException))]
    [DataRow(typeof(DnsException))]
    [DataRow(typeof(TimeoutRejectedException))]
    [DataRow(typeof(AuthenticationException))]
    public async Task Test_CacheExists_PingSucceeds_FailsOriginalRequest_LastIpAddressOfLastAlternativeHostWorks(Type exceptionType)
    {
        // Arrange
        SetConnectionStatus(ConnectionStatus.Disconnected);
        _settings.IsAlternativeRoutingEnabled = true;
        _alternativeHostsManager.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                                .Returns(CreateAlternativeHostsList());
        _dnsManager.GetAsync(ALTERNATIVE_HOST_3, Arg.Any<CancellationToken>())
                   .Returns(CreateIpAddressList());
        _dnsManager.GetFromCache(CONFIG_API_HOST)
                   .Returns(CreateIpAddressList());
        HttpRequestMessage request = new(HttpMethod.Get, REQUEST_URL);
        MockedRequest mockedRequestOriginal = _mockHttpMessageHandler
                                              .When(REQUEST_URL)
                                              .Respond(_ => throw CreateException(exceptionType));
        MockedRequest mockedPingRequest = _mockHttpMessageHandler
                                          .When(CONFIG_API_PING_URL)
                                          .Respond(_ => new(HttpStatusCode.OK));
        MockedRequest mockedRequestError1 = _mockHttpMessageHandler
                                            .When(ALTERNATIVE_URL_1)
                                            .Respond(_ => throw CreateException(exceptionType));
        MockedRequest mockedRequestError2 = _mockHttpMessageHandler
                                            .When(ALTERNATIVE_URL_2)
                                            .Respond(_ => throw CreateException(exceptionType));
        MockedRequest mockedRequestSuccess = _mockHttpMessageHandler
                                             .When(ALTERNATIVE_URL_3)
                                             .Respond(_ => new(HttpStatusCode.OK));

        // Act
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(REQUEST_URL, request.RequestUri.ToString());
        _mockHttpMessageHandler.GetMatchCount(mockedRequestOriginal).Should().Be(2);
        _mockHttpMessageHandler.GetMatchCount(mockedPingRequest).Should().Be(1);
        _mockHttpMessageHandler.GetMatchCount(mockedRequestError1).Should().Be(1);
        _mockHttpMessageHandler.GetMatchCount(mockedRequestError2).Should().Be(1);
        _mockHttpMessageHandler.GetMatchCount(mockedRequestSuccess).Should().Be(1);
        await _dnsManager.Received(3).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        foreach (string alternativeHosts in CreateAlternativeHostsList())
        {
            await _dnsManager.Received(1).GetAsync(alternativeHosts, Arg.Any<CancellationToken>());
        }
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(CONFIG_API_HOST, Arg.Any<CancellationToken>());
        _dnsManager.Received(1).GetFromCache(Arg.Any<string>());
        _dnsManager.Received(1).GetFromCache(CONFIG_API_HOST);
        _alternativeRoutingHostGenerator.Received(1).Generate(Arg.Any<string>());
        await _alternativeHostsManager.Received(1).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    [DataRow(typeof(TimeoutException))]
    [DataRow(typeof(DnsException))]
    [DataRow(typeof(TimeoutRejectedException))]
    [DataRow(typeof(AuthenticationException))]
    public async Task Test_CacheExists_PingThrows_LastIpAddressOfLastAlternativeHostWorks(Type exceptionType)
    {
        await Test_CacheExists_PingError_LastIpAddressOfLastAlternativeHostWorks(
            exceptionType, () => throw CreateException(exceptionType));
    }

    private async Task Test_CacheExists_PingError_LastIpAddressOfLastAlternativeHostWorks(Type exceptionType,
        Func<HttpResponseMessage> configApiPingResponse)
    {
        // Arrange
        SetConnectionStatus(ConnectionStatus.Disconnected);
        _settings.IsAlternativeRoutingEnabled = true;
        _alternativeHostsManager.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                                .Returns(CreateAlternativeHostsList());
        _dnsManager.GetAsync(ALTERNATIVE_HOST_3, Arg.Any<CancellationToken>())
                   .Returns(CreateIpAddressList());
        _dnsManager.GetFromCache(CONFIG_API_HOST)
                   .Returns(CreateIpAddressList());
        HttpRequestMessage request = new(HttpMethod.Get, REQUEST_URL);
        MockedRequest mockedRequestOriginal = _mockHttpMessageHandler
                                              .When(REQUEST_URL)
                                              .Respond(_ => throw CreateException(exceptionType));
        MockedRequest mockedRequestError1 = _mockHttpMessageHandler
                                            .When(ALTERNATIVE_URL_1)
                                            .Respond(_ => throw CreateException(exceptionType));
        MockedRequest mockedRequestError2 = _mockHttpMessageHandler
                                            .When(ALTERNATIVE_URL_2)
                                            .Respond(_ => throw CreateException(exceptionType));
        MockedRequest mockedRequestSuccess = _mockHttpMessageHandler
                                             .When(ALTERNATIVE_URL_3)
                                             .Respond(_ => new(HttpStatusCode.OK));
        MockedRequest mockedPingRequest = _mockHttpMessageHandler
                                          .When(CONFIG_API_PING_URL)
                                          .Respond(_ => configApiPingResponse());

        // Act
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(REQUEST_URL, request.RequestUri.ToString());
        _mockHttpMessageHandler.GetMatchCount(mockedRequestOriginal).Should().Be(1);
        _mockHttpMessageHandler.GetMatchCount(mockedPingRequest).Should().Be(1);
        _mockHttpMessageHandler.GetMatchCount(mockedRequestError1).Should().Be(1);
        _mockHttpMessageHandler.GetMatchCount(mockedRequestError2).Should().Be(1);
        _mockHttpMessageHandler.GetMatchCount(mockedRequestSuccess).Should().Be(1);
        await _dnsManager.Received(3).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        foreach (string alternativeHosts in CreateAlternativeHostsList())
        {
            await _dnsManager.Received(1).GetAsync(alternativeHosts, Arg.Any<CancellationToken>());
        }
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(CONFIG_API_HOST, Arg.Any<CancellationToken>());
        _dnsManager.Received(1).GetFromCache(Arg.Any<string>());
        _dnsManager.Received(1).GetFromCache(CONFIG_API_HOST);
        _alternativeRoutingHostGenerator.Received(1).Generate(Arg.Any<string>());
        await _alternativeHostsManager.Received(1).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    [DataRow(typeof(TimeoutException))]
    [DataRow(typeof(DnsException))]
    [DataRow(typeof(TimeoutRejectedException))]
    [DataRow(typeof(AuthenticationException))]
    public async Task Test_CacheExists_PingFails_LastIpAddressOfLastAlternativeHostWorks(Type exceptionType)
    {
        await Test_CacheExists_PingError_LastIpAddressOfLastAlternativeHostWorks(
            exceptionType, () => new(HttpStatusCode.BadRequest));
    }

    [TestMethod]
    [DataRow(typeof(TimeoutException))]
    [DataRow(typeof(DnsException))]
    [DataRow(typeof(TimeoutRejectedException))]
    [DataRow(typeof(AuthenticationException))]
    public async Task Test_ResolveSucceeds_SucceedsOnSecondAttempt(Type exceptionType)
    {
        // Arrange
        SetConnectionStatus(ConnectionStatus.Disconnected);
        _settings.IsAlternativeRoutingEnabled = true;
        _dnsManager.ResolveWithoutCacheAsync(CONFIG_API_HOST, Arg.Any<CancellationToken>())
                   .Returns(CreateIpAddressList());
        HttpRequestMessage request = new(HttpMethod.Get, REQUEST_URL);
        MockedRequest mockedRequestOriginal = _mockHttpMessageHandler
                                              .When(REQUEST_URL)
                                              .Respond(_ => ThrowOnFirstHttpResponseMessageThenSucceed(exceptionType));
        MockedRequest mockedPingRequest = _mockHttpMessageHandler
                                          .When(CONFIG_API_PING_URL)
                                          .Respond(_ => new(HttpStatusCode.OK));

        // Act
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(REQUEST_URL, request.RequestUri.ToString());
        _mockHttpMessageHandler.GetMatchCount(mockedRequestOriginal).Should().Be(2);
        _mockHttpMessageHandler.GetMatchCount(mockedPingRequest).Should().Be(1);
        await _dnsManager.Received(0).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(CONFIG_API_HOST, Arg.Any<CancellationToken>());
        _dnsManager.Received(0).GetFromCache(Arg.Any<string>());
        _alternativeRoutingHostGenerator.Received(0).Generate(Arg.Any<string>());
        await _alternativeHostsManager.Received(0).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task Test_AlternativeRoutingEnabled_LastCheckNull_ResolveWorks_ApiIsAvailable_Succeeds()
    {
        // Arrange
        SetActiveAlternativeHost();
        SetConnectionStatus(ConnectionStatus.Disconnected);
        _settings.IsAlternativeRoutingEnabled = true;
        _dnsManager.ResolveWithoutCacheAsync(CONFIG_API_HOST, Arg.Any<CancellationToken>())
                   .Returns(CreateIpAddressList());
        HttpRequestMessage request = new(HttpMethod.Get, REQUEST_URL);
        MockedRequest mockedPingRequest = _mockHttpMessageHandler
                                          .When(CONFIG_API_PING_URL)
                                          .Respond(_ => new(HttpStatusCode.OK));
        MockedRequest mockedRequestOriginal = _mockHttpMessageHandler
                                              .When(REQUEST_URL)
                                              .Respond(_ => new(HttpStatusCode.OK));

        // Act
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(REQUEST_URL, request.RequestUri.ToString());
        _mockHttpMessageHandler.GetMatchCount(mockedRequestOriginal).Should().Be(1);
        _mockHttpMessageHandler.GetMatchCount(mockedPingRequest).Should().Be(1);
        await _dnsManager.Received(0).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(CONFIG_API_HOST, Arg.Any<CancellationToken>());
        _dnsManager.Received(0).GetFromCache(Arg.Any<string>());
        _alternativeRoutingHostGenerator.Received(0).Generate(Arg.Any<string>());
        await _alternativeHostsManager.Received(0).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    private void SetActiveAlternativeHost()
    {
        _settings.ActiveAlternativeApiBaseUrl = ALTERNATIVE_HOST_2;
    }

    [TestMethod]
    public async Task Test_AlternativeRoutingEnabled_LastCheckNull_CacheWorks_ApiIsAvailable_Succeeds()
    {
        // Arrange
        SetActiveAlternativeHost();
        SetConnectionStatus(ConnectionStatus.Disconnected);
        _settings.IsAlternativeRoutingEnabled = true;
        _dnsManager.GetFromCache(CONFIG_API_HOST)
                   .Returns(CreateIpAddressList());
        HttpRequestMessage request = new(HttpMethod.Get, REQUEST_URL);
        MockedRequest mockedPingRequest = _mockHttpMessageHandler
                                          .When(CONFIG_API_PING_URL)
                                          .Respond(_ => new(HttpStatusCode.OK));
        MockedRequest mockedRequestOriginal = _mockHttpMessageHandler
                                              .When(REQUEST_URL)
                                              .Respond(_ => new(HttpStatusCode.OK));

        // Act
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(REQUEST_URL, request.RequestUri.ToString());
        _mockHttpMessageHandler.GetMatchCount(mockedRequestOriginal).Should().Be(1);
        _mockHttpMessageHandler.GetMatchCount(mockedPingRequest).Should().Be(1);
        await _dnsManager.Received(0).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(CONFIG_API_HOST, Arg.Any<CancellationToken>());
        _dnsManager.Received(1).GetFromCache(Arg.Any<string>());
        _dnsManager.Received(1).GetFromCache(CONFIG_API_HOST);
        _alternativeRoutingHostGenerator.Received(0).Generate(Arg.Any<string>());
        await _alternativeHostsManager.Received(0).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task Test_AlternativeRoutingEnabled_LastCheckIsOld_CacheWorks_ApiIsAvailable_Succeeds()
    {
        // Arrange
        SetActiveAlternativeHost();
        SetOldLastAlternativeRoutingCheckDateUtc();
        SetConnectionStatus(ConnectionStatus.Disconnected);
        _settings.IsAlternativeRoutingEnabled = true;
        _dnsManager.GetFromCache(CONFIG_API_HOST)
                   .Returns(CreateIpAddressList());
        HttpRequestMessage request = new(HttpMethod.Get, REQUEST_URL);
        MockedRequest mockedPingRequest = _mockHttpMessageHandler
                                          .When(CONFIG_API_PING_URL)
                                          .Respond(_ => new(HttpStatusCode.OK));
        MockedRequest mockedRequestOriginal = _mockHttpMessageHandler
                                              .When(REQUEST_URL)
                                              .Respond(_ => new(HttpStatusCode.OK));

        // Act
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(REQUEST_URL, request.RequestUri.ToString());
        _mockHttpMessageHandler.GetMatchCount(mockedRequestOriginal).Should().Be(1);
        _mockHttpMessageHandler.GetMatchCount(mockedPingRequest).Should().Be(1);
        await _dnsManager.Received(0).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(CONFIG_API_HOST, Arg.Any<CancellationToken>());
        _dnsManager.Received(1).GetFromCache(Arg.Any<string>());
        _dnsManager.Received(1).GetFromCache(CONFIG_API_HOST);
        _alternativeRoutingHostGenerator.Received(0).Generate(Arg.Any<string>());
        await _alternativeHostsManager.Received(0).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    private void SetOldLastAlternativeRoutingCheckDateUtc()
    {
        FieldInfo field = typeof(AlternativeHostHandler).GetField("_lastAlternativeRoutingCheckDateUtc",
            BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(_alternativeHostHandler, DateTime.UtcNow.AddSeconds(-1) - ALTERNATIVE_ROUTING_CHECK_INTERVAL);
    }

    [TestMethod]
    public async Task Test_AlternativeRoutingEnabled_LastCheckIsOld_NoCache_AlternativeRoutingSucceeds()
    {
        // Arrange
        HttpRequestMessage request = new(HttpMethod.Get, REQUEST_URL);
        MockedRequest mockedRequestError1 = _mockHttpMessageHandler
                                            .When(ALTERNATIVE_URL_1)
                                            .Respond(_ => throw new Exception("Test exception"));
        MockedRequest mockedRequestError2 = _mockHttpMessageHandler
                                            .When(ALTERNATIVE_URL_2)
                                            .Respond(_ => throw new Exception("Test exception"));
        MockedRequest mockedRequestSuccess = _mockHttpMessageHandler
                                             .When(ALTERNATIVE_URL_3)
                                             .Respond(_ => new(HttpStatusCode.OK));

        // Arrange + Act + Assert
        await InternalTest_AlternativeRoutingEnabled_LastCheckIsOld_NoCache_AlternativeRoutingSucceeds(
            request, mockedRequestError1, mockedRequestError2, mockedRequestSuccess);
    }

    private async Task InternalTest_AlternativeRoutingEnabled_LastCheckIsOld_NoCache_AlternativeRoutingSucceeds(
        HttpRequestMessage request, params MockedRequest[] mockedRequests)
    {
        // Arrange
        SetActiveAlternativeHost();
        SetOldLastAlternativeRoutingCheckDateUtc();
        SetConnectionStatus(ConnectionStatus.Disconnected);
        _settings.IsAlternativeRoutingEnabled = true;
        _dnsManager.GetAsync(ALTERNATIVE_HOST_2, Arg.Any<CancellationToken>())
                   .Returns(CreateIpAddressList());

        // Act
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(REQUEST_URL, request.RequestUri.ToString());
        foreach (MockedRequest mockedRequest in mockedRequests)
        {
            _mockHttpMessageHandler.GetMatchCount(mockedRequest).Should().Be(1);
        }
        await _dnsManager.Received(1).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(CONFIG_API_HOST, Arg.Any<CancellationToken>());
        _dnsManager.Received(1).GetFromCache(Arg.Any<string>());
        _dnsManager.Received(1).GetFromCache(CONFIG_API_HOST);
        _alternativeRoutingHostGenerator.Received(0).Generate(Arg.Any<string>());
        await _alternativeHostsManager.Received(0).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task Test_AlternativeRoutingEnabled_LastCheckIsOld_ResolveWorksPingThrows_AlternativeRoutingSucceeds()
    {
        MockedRequest mockedPingRequest = _mockHttpMessageHandler
                                          .When(CONFIG_API_PING_URL)
                                          .Respond(_ => throw new Exception("Test exception"));
        await Test_AlternativeRoutingEnabled_LastCheckIsOld_ResolveWorksPingFails_AlternativeRoutingSucceeds(
            mockedPingRequest);
    }

    private async Task Test_AlternativeRoutingEnabled_LastCheckIsOld_ResolveWorksPingFails_AlternativeRoutingSucceeds(MockedRequest mockedPingRequest)
    {
        // Arrange
        SetActiveAlternativeHost();
        SetOldLastAlternativeRoutingCheckDateUtc();
        SetConnectionStatus(ConnectionStatus.Disconnected);
        _settings.IsAlternativeRoutingEnabled = true;
        _dnsManager.ResolveWithoutCacheAsync(CONFIG_API_HOST, Arg.Any<CancellationToken>())
                   .Returns(CreateIpAddressList());
        _dnsManager.GetAsync(ALTERNATIVE_HOST_2, Arg.Any<CancellationToken>())
                   .Returns(CreateIpAddressList());
        HttpRequestMessage request = new(HttpMethod.Get, REQUEST_URL);
        MockedRequest mockedRequestError1 = _mockHttpMessageHandler
                                            .When(ALTERNATIVE_URL_1)
                                            .Respond(_ => throw new Exception("Test exception"));
        MockedRequest mockedRequestError2 = _mockHttpMessageHandler
                                            .When(ALTERNATIVE_URL_2)
                                            .Respond(_ => throw new Exception("Test exception"));
        MockedRequest mockedRequestSuccess = _mockHttpMessageHandler
                                             .When(ALTERNATIVE_URL_3)
                                             .Respond(_ => new(HttpStatusCode.OK));

        // Act
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(REQUEST_URL, request.RequestUri.ToString());
        _mockHttpMessageHandler.GetMatchCount(mockedPingRequest).Should().Be(1);
        _mockHttpMessageHandler.GetMatchCount(mockedRequestError1).Should().Be(1);
        _mockHttpMessageHandler.GetMatchCount(mockedRequestError2).Should().Be(1);
        _mockHttpMessageHandler.GetMatchCount(mockedRequestSuccess).Should().Be(1);
        await _dnsManager.Received(1).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(CONFIG_API_HOST, Arg.Any<CancellationToken>());
        _dnsManager.Received(0).GetFromCache(Arg.Any<string>());
        _alternativeRoutingHostGenerator.Received(0).Generate(Arg.Any<string>());
        await _alternativeHostsManager.Received(0).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task Test_AlternativeRoutingEnabled_LastCheckIsOld_ResolveWorksPingReturnsError_AlternativeRoutingSucceeds()
    {
        MockedRequest mockedPingRequest = _mockHttpMessageHandler
                                          .When(CONFIG_API_PING_URL)
                                          .Respond(_ => new(HttpStatusCode.BadRequest));
        await Test_AlternativeRoutingEnabled_LastCheckIsOld_ResolveWorksPingFails_AlternativeRoutingSucceeds(
            mockedPingRequest);
    }

    [TestMethod]
    public async Task Test_AlternativeRoutingEnabled_LastCheckIsOld_NoCache_AlternativeRoutingFails_NormalRequestSucceeds()
    {
        // Arrange
        SetActiveAlternativeHost();
        SetOldLastAlternativeRoutingCheckDateUtc();
        SetConnectionStatus(ConnectionStatus.Disconnected);
        _settings.IsAlternativeRoutingEnabled = true;
        _dnsManager.GetAsync(ALTERNATIVE_HOST_2, Arg.Any<CancellationToken>())
                   .Returns(CreateIpAddressList());
        HttpRequestMessage request = new(HttpMethod.Get, REQUEST_URL);
        MockedRequest mockedRequestError1 = _mockHttpMessageHandler
                                            .When(ALTERNATIVE_URL_1)
                                            .Respond(_ => throw new Exception("Test exception"));
        MockedRequest mockedRequestError2 = _mockHttpMessageHandler
                                            .When(ALTERNATIVE_URL_2)
                                            .Respond(_ => throw new Exception("Test exception"));
        MockedRequest mockedRequestError3 = _mockHttpMessageHandler
                                            .When(ALTERNATIVE_URL_3)
                                            .Respond(_ => throw new Exception("Test exception"));
        MockedRequest mockedRequestOriginal = _mockHttpMessageHandler
                                              .When(REQUEST_URL)
                                              .Respond(_ => new(HttpStatusCode.OK));

        // Act
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(REQUEST_URL, request.RequestUri.ToString());
        _mockHttpMessageHandler.GetMatchCount(mockedRequestError1).Should().Be(1);
        _mockHttpMessageHandler.GetMatchCount(mockedRequestError2).Should().Be(1);
        _mockHttpMessageHandler.GetMatchCount(mockedRequestError3).Should().Be(1);
        _mockHttpMessageHandler.GetMatchCount(mockedRequestOriginal).Should().Be(1);
        await _dnsManager.Received(1).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(CONFIG_API_HOST, Arg.Any<CancellationToken>());
        _dnsManager.Received(1).GetFromCache(Arg.Any<string>());
        _dnsManager.Received(1).GetFromCache(CONFIG_API_HOST);
        _alternativeRoutingHostGenerator.Received(0).Generate(Arg.Any<string>());
        await _alternativeHostsManager.Received(0).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task Test_AlternativeRoutingEnabled_LastCheckIsOld_NoCache_NoAlternativeRoutingDns_NormalRequestSucceeds()
    {
        // Arrange
        SetActiveAlternativeHost();
        SetOldLastAlternativeRoutingCheckDateUtc();
        SetConnectionStatus(ConnectionStatus.Disconnected);
        _settings.IsAlternativeRoutingEnabled = true;
        HttpRequestMessage request = new(HttpMethod.Get, REQUEST_URL);
        MockedRequest mockedRequestOriginal = _mockHttpMessageHandler
                                              .When(REQUEST_URL)
                                              .Respond(_ => new(HttpStatusCode.OK));

        // Act
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(REQUEST_URL, request.RequestUri.ToString());
        _mockHttpMessageHandler.GetMatchCount(mockedRequestOriginal).Should().Be(1);
        await _dnsManager.Received(1).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(CONFIG_API_HOST, Arg.Any<CancellationToken>());
        _dnsManager.Received(1).GetFromCache(Arg.Any<string>());
        _dnsManager.Received(1).GetFromCache(CONFIG_API_HOST);
        _alternativeRoutingHostGenerator.Received(0).Generate(Arg.Any<string>());
        await _alternativeHostsManager.Received(0).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task Test_AlternativeRoutingEnabled_LastCheckIsFresh_AlternativeRoutingSucceeds()
    {
        // Arrange
        SetActiveAlternativeHost();
        SetFreshLastAlternativeRoutingCheckDateUtc();
        SetConnectionStatus(ConnectionStatus.Disconnected);
        _settings.IsAlternativeRoutingEnabled = true;
        _dnsManager.GetAsync(ALTERNATIVE_HOST_2, Arg.Any<CancellationToken>())
                   .Returns(CreateIpAddressList());
        HttpRequestMessage request = new(HttpMethod.Get, REQUEST_URL);
        MockedRequest mockedRequestError1 = _mockHttpMessageHandler
                                            .When(ALTERNATIVE_URL_1)
                                            .Respond(_ => throw new Exception("Test exception"));
        MockedRequest mockedRequestError2 = _mockHttpMessageHandler
                                            .When(ALTERNATIVE_URL_2)
                                            .Respond(_ => throw new Exception("Test exception"));
        MockedRequest mockedRequestSuccess = _mockHttpMessageHandler
                                             .When(ALTERNATIVE_URL_3)
                                             .Respond(_ => new(HttpStatusCode.OK));

        // Act
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(REQUEST_URL, request.RequestUri.ToString());
        _mockHttpMessageHandler.GetMatchCount(mockedRequestError1).Should().Be(1);
        _mockHttpMessageHandler.GetMatchCount(mockedRequestError2).Should().Be(1);
        _mockHttpMessageHandler.GetMatchCount(mockedRequestSuccess).Should().Be(1);
        await _dnsManager.Received(1).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(0).ResolveWithoutCacheAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        _dnsManager.Received(0).GetFromCache(Arg.Any<string>());
        _alternativeRoutingHostGenerator.Received(0).Generate(Arg.Any<string>());
        await _alternativeHostsManager.Received(0).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    private void SetFreshLastAlternativeRoutingCheckDateUtc()
    {
        FieldInfo field = typeof(AlternativeHostHandler).GetField("_lastAlternativeRoutingCheckDateUtc",
            BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(_alternativeHostHandler, DateTime.UtcNow);
    }

    [TestMethod]
    public async Task Test_AlternativeRoutingEnabled_LastCheckIsFresh_AlternativeRoutingFails_NormalRequestSucceeds()
    {
        // Arrange
        SetActiveAlternativeHost();
        SetFreshLastAlternativeRoutingCheckDateUtc();
        SetConnectionStatus(ConnectionStatus.Disconnected);
        _settings.IsAlternativeRoutingEnabled = true;
        _dnsManager.GetAsync(ALTERNATIVE_HOST_2, Arg.Any<CancellationToken>())
                   .Returns(CreateIpAddressList());
        HttpRequestMessage request = new(HttpMethod.Get, REQUEST_URL);
        MockedRequest mockedRequestError1 = _mockHttpMessageHandler
                                            .When(ALTERNATIVE_URL_1)
                                            .Respond(_ => throw new Exception("Test exception"));
        MockedRequest mockedRequestError2 = _mockHttpMessageHandler
                                            .When(ALTERNATIVE_URL_2)
                                            .Respond(_ => throw new Exception("Test exception"));
        MockedRequest mockedRequestError3 = _mockHttpMessageHandler
                                            .When(ALTERNATIVE_URL_3)
                                            .Respond(_ => throw new Exception("Test exception"));
        MockedRequest mockedRequestOriginal = _mockHttpMessageHandler
                                              .When(REQUEST_URL)
                                              .Respond(_ => new(HttpStatusCode.OK));

        // Act
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(REQUEST_URL, request.RequestUri.ToString());
        _mockHttpMessageHandler.GetMatchCount(mockedRequestError1).Should().Be(1);
        _mockHttpMessageHandler.GetMatchCount(mockedRequestError2).Should().Be(1);
        _mockHttpMessageHandler.GetMatchCount(mockedRequestError3).Should().Be(1);
        _mockHttpMessageHandler.GetMatchCount(mockedRequestOriginal).Should().Be(1);
        await _dnsManager.Received(1).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(0).ResolveWithoutCacheAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        _dnsManager.Received(0).GetFromCache(Arg.Any<string>());
        _alternativeRoutingHostGenerator.Received(0).Generate(Arg.Any<string>());
        await _alternativeHostsManager.Received(0).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task Test_AlternativeRoutingEnabled_LastCheckIsFresh_NoAlternativeRoutingDns_NormalRequestSucceeds()
    {
        // Arrange
        SetActiveAlternativeHost();
        SetFreshLastAlternativeRoutingCheckDateUtc();
        SetConnectionStatus(ConnectionStatus.Disconnected);
        _settings.IsAlternativeRoutingEnabled = true;
        HttpRequestMessage request = new(HttpMethod.Get, REQUEST_URL);
        MockedRequest mockedRequestOriginal = _mockHttpMessageHandler
                                              .When(REQUEST_URL)
                                              .Respond(_ => new(HttpStatusCode.OK));

        // Act
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(REQUEST_URL, request.RequestUri.ToString());
        _mockHttpMessageHandler.GetMatchCount(mockedRequestOriginal).Should().Be(1);
        await _dnsManager.Received(1).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(0).ResolveWithoutCacheAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        _dnsManager.Received(0).GetFromCache(Arg.Any<string>());
        _alternativeRoutingHostGenerator.Received(0).Generate(Arg.Any<string>());
        await _alternativeHostsManager.Received(0).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    [DataRow(ConnectionStatus.Connecting)]
    [DataRow(ConnectionStatus.Connected)]
    public async Task Test_AlternativeRoutingEnabled_ButNotAllowedDueToConnectionStatus_NormalRequestSucceeds(ConnectionStatus connectionStatus)
    {
        // Arrange
        SetActiveAlternativeHost();
        SetConnectionStatus(connectionStatus);
        _settings.IsAlternativeRoutingEnabled = true;
        HttpRequestMessage request = new(HttpMethod.Get, REQUEST_URL);
        MockedRequest mockedRequestOriginal = _mockHttpMessageHandler
                                              .When(REQUEST_URL)
                                              .Respond(_ => new(HttpStatusCode.OK));

        await Test_AlternativeRoutingEnabled_ButNotAllowed_NormalRequestSucceeds(request, mockedRequestOriginal);
    }

    private async Task Test_AlternativeRoutingEnabled_ButNotAllowed_NormalRequestSucceeds(
        HttpRequestMessage request, MockedRequest mockedRequestOriginal)
    {
        // Act
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(REQUEST_URL, request.RequestUri.ToString());
        _mockHttpMessageHandler.GetMatchCount(mockedRequestOriginal).Should().Be(1);
        await NoDependenciesAreCalledAsync();
    }

    [TestMethod]
    public async Task Test_AlternativeRoutingEnabled_ButNotAllowedDueToDoHDisabled_NormalRequestSucceeds()
    {
        // Arrange
        SetActiveAlternativeHost();
        SetConnectionStatus(ConnectionStatus.Disconnected);
        _settings.IsAlternativeRoutingEnabled = false;
        HttpRequestMessage request = new(HttpMethod.Get, REQUEST_URL);
        MockedRequest mockedRequestOriginal = _mockHttpMessageHandler
                                              .When(REQUEST_URL)
                                              .Respond(_ => new(HttpStatusCode.OK));

        await Test_AlternativeRoutingEnabled_ButNotAllowed_NormalRequestSucceeds(request, mockedRequestOriginal);
    }

    [TestMethod]
    public async Task Test_AlternativeRoutingEnabled_ButNotAllowedDueToGuestHoleActive_NormalRequestSucceeds()
    {
        // Arrange
        SetActiveAlternativeHost();
        SetConnectionStatus(ConnectionStatus.Disconnected);
        _settings.IsAlternativeRoutingEnabled = true;
        SetGuestHoleState(true);
        HttpRequestMessage request = new(HttpMethod.Get, REQUEST_URL);
        MockedRequest mockedRequestOriginal = _mockHttpMessageHandler
                                              .When(REQUEST_URL)
                                              .Respond(_ => new(HttpStatusCode.OK));

        await Test_AlternativeRoutingEnabled_ButNotAllowed_NormalRequestSucceeds(request, mockedRequestOriginal);
    }



    [TestMethod]
    public async Task Test_AlternativeRoutingEnabled_SecondRequestDoesNotRepeatApiAvailabilityCheck()
    {
        // Arrange
        HttpRequestMessage request = new(HttpMethod.Get, REQUEST_URL);
        MockedRequest mockedRequestError1 = _mockHttpMessageHandler
                                            .When(ALTERNATIVE_URL_1)
                                            .Respond(_ => throw new Exception("Test exception"));
        MockedRequest mockedRequestError2 = _mockHttpMessageHandler
                                            .When(ALTERNATIVE_URL_2)
                                            .Respond(_ => throw new Exception("Test exception"));
        MockedRequest mockedRequestSuccess = _mockHttpMessageHandler
                                             .When(ALTERNATIVE_URL_3)
                                             .Respond(_ => new(HttpStatusCode.OK));

        // More arrange + Act + Assert
        await InternalTest_AlternativeRoutingEnabled_LastCheckIsOld_NoCache_AlternativeRoutingSucceeds(
            request, mockedRequestError1, mockedRequestError2, mockedRequestSuccess);

        // Arrange
        HttpRequestMessage request2 = new(HttpMethod.Get, REQUEST_URL);

        // Act
        HttpResponseMessage response = await _httpClient.SendAsync(request2);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(REQUEST_URL, request2.RequestUri.ToString());
        _mockHttpMessageHandler.GetMatchCount(mockedRequestError1).Should().Be(2);
        _mockHttpMessageHandler.GetMatchCount(mockedRequestError2).Should().Be(2);
        _mockHttpMessageHandler.GetMatchCount(mockedRequestSuccess).Should().Be(2);
        await _dnsManager.Received(2).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(CONFIG_API_HOST, Arg.Any<CancellationToken>());
        _dnsManager.Received(1).GetFromCache(Arg.Any<string>());
        _dnsManager.Received(1).GetFromCache(CONFIG_API_HOST);
        _alternativeRoutingHostGenerator.Received(0).Generate(Arg.Any<string>());
        await _alternativeHostsManager.Received(0).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    [DataRow(typeof(TimeoutException))]
    [DataRow(typeof(DnsException))]
    [DataRow(typeof(TimeoutRejectedException))]
    [DataRow(typeof(AuthenticationException))]
    public async Task TestOnUserLoggedOut_LastIpAddressOfLastAlternativeHostWorks(Type exceptionType)
    {
        // Arrange
        _alternativeHostHandler.Receive(new LoggedOutMessage());

        // Arrange + Act + Assert
        await TestOnUserLoggedInOrOut_LastIpAddressOfLastAlternativeHostWorks(exceptionType);

        // Assert
        _alternativeRoutingHostGenerator.Received(1).Generate(null);
    }

    private async Task TestOnUserLoggedInOrOut_LastIpAddressOfLastAlternativeHostWorks(Type exceptionType)
    {
        SetConnectionStatus(ConnectionStatus.Disconnected);
        _settings.IsAlternativeRoutingEnabled = true;
        _alternativeHostsManager.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                                .Returns(CreateAlternativeHostsList());
        _dnsManager.GetAsync(ALTERNATIVE_HOST_3, Arg.Any<CancellationToken>())
                                .Returns(CreateIpAddressList());
        HttpRequestMessage request = new(HttpMethod.Get, REQUEST_URL);
        MockedRequest mockedRequestOriginal = _mockHttpMessageHandler
                                              .When(REQUEST_URL)
                                              .Respond(_ => throw CreateException(exceptionType));
        MockedRequest mockedRequestError1 = _mockHttpMessageHandler
                                            .When(ALTERNATIVE_URL_1)
                                            .Respond(_ => throw CreateException(exceptionType));
        MockedRequest mockedRequestError2 = _mockHttpMessageHandler
                                            .When(ALTERNATIVE_URL_2)
                                            .Respond(_ => throw CreateException(exceptionType));
        MockedRequest mockedRequestSuccess = _mockHttpMessageHandler
                                             .When(ALTERNATIVE_URL_3)
                                             .Respond(_ => new(HttpStatusCode.OK));

        // Act
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(REQUEST_URL, request.RequestUri.ToString());
        _mockHttpMessageHandler.GetMatchCount(mockedRequestOriginal).Should().Be(1);
        _mockHttpMessageHandler.GetMatchCount(mockedRequestError1).Should().Be(1);
        _mockHttpMessageHandler.GetMatchCount(mockedRequestError2).Should().Be(1);
        _mockHttpMessageHandler.GetMatchCount(mockedRequestSuccess).Should().Be(1);
        await _dnsManager.Received(3).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        foreach (string alternativeHosts in CreateAlternativeHostsList())
        {
            await _dnsManager.Received(1).GetAsync(alternativeHosts, Arg.Any<CancellationToken>());
        }
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsManager.Received(1).ResolveWithoutCacheAsync(CONFIG_API_HOST, Arg.Any<CancellationToken>());
        _dnsManager.Received(1).GetFromCache(Arg.Any<string>());
        _dnsManager.Received(1).GetFromCache(CONFIG_API_HOST);
        _alternativeRoutingHostGenerator.Received(1).Generate(Arg.Any<string>());
        await _alternativeHostsManager.Received(1).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    [DataRow(typeof(TimeoutException))]
    [DataRow(typeof(DnsException))]
    [DataRow(typeof(TimeoutRejectedException))]
    [DataRow(typeof(AuthenticationException))]
    public async Task TestOnUserLoggedIn_LastIpAddressOfLastAlternativeHostWorks(Type exceptionType)
    {
        // Arrange
        _alternativeHostHandler.Receive(new LoggedInMessage { IsAutoLogin = false });

        // Arrange + Act + Assert
        await TestOnUserLoggedInOrOut_LastIpAddressOfLastAlternativeHostWorks(exceptionType);

        // Assert
        _alternativeRoutingHostGenerator.Received(1).Generate(UNIQUE_SESSION_ID);
    }

    private void SetGuestHoleState(bool isActive)
    {
        _guestHoleManager.IsActive.Returns(isActive);
        _alternativeHostHandler.Receive(new GuestHoleStatusChangedMessage(isActive));
    }
}