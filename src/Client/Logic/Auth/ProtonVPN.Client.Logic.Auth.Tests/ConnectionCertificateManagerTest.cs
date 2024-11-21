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
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Certificates;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Legacy.Extensions;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Logic.Auth.Tests;

[TestClass]
public class ConnectionCertificateManagerTest
{
    private MockOfConnectionKeyManager _connectionKeyManager;
    private IApiClient _apiClient;
    private ISettings _appSettings;
    private ILogger _logger;
    private IEventMessageSender _eventMessageSender;
    private ConnectionCertificateManager _certificateManager;

    private DateTimeOffset? _certificateExpirationTime;
    private DateTimeOffset? _certificateRefreshTime;
    private DateTimeOffset? _testStartTime;
    private DateTimeOffset? _testEndTime;
    private CertificateResponse _certificateResponse;
    private IList<string> _usedClientPublicKeys = new List<string>();

    [TestInitialize]
    public void Initialize()
    {
        _certificateExpirationTime = DateTimeOffset.UtcNow.AddHours(24).TruncateToSeconds();
        _certificateRefreshTime = DateTimeOffset.UtcNow.AddHours(18).TruncateToSeconds();
        _testStartTime = DateTimeOffset.UtcNow;

        _connectionKeyManager = new MockOfConnectionKeyManager();
        _apiClient = Substitute.For<IApiClient>();
        _apiClient.RequestConnectionCertificateAsync(Arg.Any<CertificateRequest>())
            .Returns(async (args) => await MockOfRequestCertificateAsync(args.Arg<CertificateRequest>()));
        _appSettings = Substitute.For<ISettings>();
        _logger = Substitute.For<ILogger>();
        _eventMessageSender = Substitute.For<IEventMessageSender>();
        _certificateManager = new ConnectionCertificateManager(_appSettings, _connectionKeyManager, _apiClient, _logger, _eventMessageSender);
    }

    private Task<ApiResponseResult<CertificateResponse>> MockOfRequestCertificateAsync(CertificateRequest arg)
    {
        if (string.IsNullOrEmpty(arg.ClientPublicKey) || _usedClientPublicKeys.Contains(arg.ClientPublicKey))
        {
            return Task.FromResult(ApiResponseResult<CertificateResponse>.Fail(CreateClientPublicKeyConflictCertificateResponseData(),
                new HttpResponseMessage(HttpStatusCode.BadRequest), string.Empty));
        }
        _usedClientPublicKeys.Add(arg.ClientPublicKey);
        _certificateResponse ??= CreateCertificateResponseData();
        return Task.FromResult(ApiResponseResult<CertificateResponse>.Ok(new HttpResponseMessage(), _certificateResponse));
    }

    private CertificateResponse CreateCertificateResponseData()
    {
        return new CertificateResponse
        {
            Certificate = "--Certificate--",
            ExpirationTime = _certificateExpirationTime.Value.ToUnixTimeSeconds(),
            RefreshTime = _certificateRefreshTime.Value.ToUnixTimeSeconds(),
            ServerPublicKey = "--Server Public Key--"
        };
    }

    private CertificateResponse CreateClientPublicKeyConflictCertificateResponseData()
    {
        return new CertificateResponse
        {
            Code = ResponseCodes.ClientPublicKeyConflict
        };
    }

    [TestCleanup]
    public void Cleanup()
    {
        _connectionKeyManager = null;
        _apiClient = null;
        _appSettings = null;
        _logger = null;
        _certificateManager = null;

        _certificateExpirationTime = null;
        _certificateRefreshTime = null;
        _testStartTime = null;
        _testEndTime = null;
        _certificateResponse = null;
        _usedClientPublicKeys = null;
    }

    [TestMethod]
    public async Task TestForceRequestNewCertificateAsync()
    {
        _connectionKeyManager.InitializeTestKeyPair();
        await ForceRequestNewCertificateAsync();

        ValidateClientSettings();
        await _apiClient.Received(1).RequestConnectionCertificateAsync(Arg.Any<CertificateRequest>());
        AssertNumOfCallsToRecreateKeys(0);
    }

    private async Task ForceRequestNewCertificateAsync()
    {
        await _certificateManager.ForceRequestNewCertificateAsync();
        _testEndTime = DateTimeOffset.UtcNow;
    }

    private void ValidateClientSettings()
    {
        ConnectionCertificate? connectionCertificate = _appSettings.ConnectionCertificate;

        Assert.IsTrue(_testStartTime <= connectionCertificate.Value.RequestUtcDate);
        Assert.IsTrue(_testEndTime >= connectionCertificate.Value.RequestUtcDate);
        Assert.AreEqual(_certificateResponse.Certificate, connectionCertificate.Value.Pem);
        Assert.AreEqual(_certificateExpirationTime, connectionCertificate.Value.ExpirationUtcDate);
        Assert.AreEqual(_certificateRefreshTime, connectionCertificate.Value.RefreshUtcDate);
    }

    private void AssertNumOfCallsToRecreateKeys(int expectedNumOfCalls)
    {
        Assert.AreEqual(expectedNumOfCalls, _connectionKeyManager.MethodCalls[nameof(_connectionKeyManager.RegenerateKeyPair)]);
    }

    [TestMethod]
    public async Task TestForceRequestNewCertificateAsync_WhenNoClientPublicKeyExists()
    {
        await ForceRequestNewCertificateAsync();

        ValidateClientSettings();
        await _apiClient.Received(1).RequestConnectionCertificateAsync(Arg.Any<CertificateRequest>());
        AssertNumOfCallsToRecreateKeys(1);
    }

    [TestMethod]
    public async Task TestForceRequestNewCertificateAsync_WhenPublicKeyWasAlreadyUsed()
    {
        _connectionKeyManager.InitializeTestKeyPair();
        _usedClientPublicKeys.Add(_connectionKeyManager.GetPublicKey().Pem);

        await ForceRequestNewCertificateAsync();

        ValidateClientSettings();
        await _apiClient.Received(2).RequestConnectionCertificateAsync(Arg.Any<CertificateRequest>());
        AssertNumOfCallsToRecreateKeys(1);
    }

    [TestMethod]
    public async Task TestForceRequestNewCertificateAsync_WhenCalledMultipleTimes()
    {
        _connectionKeyManager.InitializeTestKeyPair();

        await ForceRequestNewCertificateAsync();
        await ForceRequestNewCertificateAsync();
        await ForceRequestNewCertificateAsync();

        ValidateClientSettings();
        await _apiClient.Received(5).RequestConnectionCertificateAsync(Arg.Any<CertificateRequest>());
        AssertNumOfCallsToRecreateKeys(2);
    }

    [TestMethod]
    public async Task TestRequestNewCertificateAsync_WhenNoCertificateWasEverSet()
    {
        _appSettings.ConnectionCertificate = null;

        await TestRequestNewCertificateAsync();
    }

    private async Task TestRequestNewCertificateAsync()
    {
        _connectionKeyManager.InitializeTestKeyPair();

        await RequestNewCertificateAsync();

        ValidateClientSettings();
        await _apiClient.Received(1).RequestConnectionCertificateAsync(Arg.Any<CertificateRequest>());
        AssertNumOfCallsToRecreateKeys(0);
    }

    [TestMethod]
    public async Task TestRequestNewCertificateAsync_WhenCertificateIsExpired()
    {
        ConnectionCertificate connectionCertificate = new()
        {
            Pem = "TestCert",
            RequestUtcDate = DateTimeOffset.UtcNow,
            RefreshUtcDate = DateTimeOffset.UtcNow.AddDays(1),
            ExpirationUtcDate = DateTimeOffset.MinValue,
        };
        _appSettings.ConnectionCertificate = connectionCertificate;

        await TestRequestNewCertificateAsync();
    }

    [TestMethod]
    public async Task TestRequestNewCertificateAsync_WhenCertificateIsToBeRefreshed()
    {
        ConnectionCertificate connectionCertificate = new()
        {
            Pem = "TestCert",
            RequestUtcDate = DateTimeOffset.UtcNow,
            RefreshUtcDate = DateTimeOffset.MinValue,
            ExpirationUtcDate = DateTimeOffset.UtcNow.AddDays(1)
        };
        _appSettings.ConnectionCertificate = connectionCertificate;

        await TestRequestNewCertificateAsync();
    }

    [TestMethod]
    public async Task TestRequestNewCertificateAsync_WhenCertificateIsEmpty()
    {
        ConnectionCertificate connectionCertificate = new()
        {
            Pem = string.Empty,
            RequestUtcDate = DateTimeOffset.UtcNow,
            RefreshUtcDate = DateTimeOffset.UtcNow.AddDays(1),
            ExpirationUtcDate = DateTimeOffset.UtcNow.AddDays(1)
        };
        _appSettings.ConnectionCertificate = connectionCertificate;

        await TestRequestNewCertificateAsync();
    }

    [TestMethod]
    public async Task TestRequestNewCertificateAsync_WhenCertificateIsNull()
    {
        ConnectionCertificate connectionCertificate = new()
        {
            Pem = null,
            RequestUtcDate = DateTimeOffset.UtcNow,
            RefreshUtcDate = DateTimeOffset.UtcNow.AddDays(1),
            ExpirationUtcDate = DateTimeOffset.UtcNow.AddDays(1)
        };
        _appSettings.ConnectionCertificate = connectionCertificate;

        await TestRequestNewCertificateAsync();
    }

    private async Task RequestNewCertificateAsync()
    {
        await _certificateManager.RequestNewCertificateAsync();
        _testEndTime = DateTimeOffset.UtcNow;
    }

    [TestMethod]
    public async Task TestRequestNewCertificateAsync_WhenNoClientPublicKeyExists()
    {
        await RequestNewCertificateAsync();

        ValidateClientSettings();
        await _apiClient.Received(1).RequestConnectionCertificateAsync(Arg.Any<CertificateRequest>());
        AssertNumOfCallsToRecreateKeys(1);
    }

    [TestMethod]
    public async Task TestRequestNewCertificateAsync_WhenPublicKeyWasAlreadyUsed()
    {
        _connectionKeyManager.InitializeTestKeyPair();
        _usedClientPublicKeys.Add(_connectionKeyManager.GetPublicKey().Pem);

        await RequestNewCertificateAsync();

        ValidateClientSettings();
        await _apiClient.Received(2).RequestConnectionCertificateAsync(Arg.Any<CertificateRequest>());
        AssertNumOfCallsToRecreateKeys(1);
    }

    [TestMethod]
    public async Task TestRequestNewCertificateAsync_WhenCalledMultipleTimes()
    {
        _connectionKeyManager.InitializeTestKeyPair();

        await RequestNewCertificateAsync();
        await RequestNewCertificateAsync();
        await RequestNewCertificateAsync();

        ValidateClientSettings();
        await _apiClient.Received(1).RequestConnectionCertificateAsync(Arg.Any<CertificateRequest>());
        AssertNumOfCallsToRecreateKeys(0);
    }

    [TestMethod]
    public async Task TestRequestNewCertificateAsync_WhenCertificateNotExpired()
    {
        ConnectionCertificate connectionCertificate = new()
        {
            Pem = "TestCert",
            RequestUtcDate = DateTimeOffset.UtcNow,
            RefreshUtcDate = DateTimeOffset.UtcNow.AddHours(10),
            ExpirationUtcDate = DateTimeOffset.UtcNow.AddHours(12)
        };
        _appSettings.ConnectionCertificate = connectionCertificate;
        _connectionKeyManager.InitializeTestKeyPair();

        await RequestNewCertificateAsync();

        await _apiClient.Received(0).RequestConnectionCertificateAsync(Arg.Any<CertificateRequest>());
    }

    [TestMethod]
    public async Task ItShouldNotDeleteOldCertIfCertRequestFailed()
    {
        ConnectionCertificate connectionCertificate = new()
        {
            Pem = "TestCert",
            RequestUtcDate = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(1)),
            RefreshUtcDate = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromHours(6)),
            ExpirationUtcDate = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromHours(2))
        };
        _appSettings.ConnectionCertificate = connectionCertificate;
        _connectionKeyManager.InitializeTestKeyPair();

        _apiClient.RequestConnectionCertificateAsync(Arg.Any<CertificateRequest>())
            .Returns((args) => Task.FromResult(ApiResponseResult<CertificateResponse>.Fail(new HttpResponseMessage(HttpStatusCode.BadGateway), string.Empty)));
        await RequestNewCertificateAsync();

        _appSettings.ConnectionCertificate.Should().NotBeNull();
        _appSettings.ConnectionCertificate.Should().Be(connectionCertificate);
    }
}