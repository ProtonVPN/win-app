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
using Caliburn.Micro;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Certificates;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Core.Tests.Auth
{
    [TestClass]
    public class AuthCertificateManagerTest
    {
        private MockOfAuthKeyManager _authKeyManager;
        private IApiClient _apiClient;
        private IAppSettings _appSettings;
        private IEventAggregator _eventAggregator;
        private ILogger _logger;
        private AuthCertificateManager _certificateManager;

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

            _authKeyManager = new MockOfAuthKeyManager();
            _apiClient = Substitute.For<IApiClient>();
            _apiClient.RequestAuthCertificateAsync(Arg.Any<CertificateRequest>())
                .Returns(async (args) => await MockOfRequestAuthCertificateAsync(args.Arg<CertificateRequest>()));
            _appSettings = Substitute.For<IAppSettings>();
            _logger = Substitute.For<ILogger>();
            _eventAggregator = Substitute.For<IEventAggregator>();
            _certificateManager = new AuthCertificateManager(_authKeyManager, _apiClient, _appSettings, _logger, _eventAggregator);
        }

        private async Task<ApiResponseResult<CertificateResponse>> MockOfRequestAuthCertificateAsync(CertificateRequest arg)
        {
            if (arg.ClientPublicKey.IsNullOrEmpty() || _usedClientPublicKeys.Contains(arg.ClientPublicKey))
            {
                return ApiResponseResult<CertificateResponse>.Fail(CreateClientPublicKeyConflictCertificateResponseData(),
                    new HttpResponseMessage(HttpStatusCode.BadRequest), string.Empty);
            }
            _usedClientPublicKeys.Add(arg.ClientPublicKey);
            _certificateResponse ??= CreateCertificateResponseData();
            return ApiResponseResult<CertificateResponse>.Ok(new HttpResponseMessage(), _certificateResponse);
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
            _authKeyManager = null;
            _apiClient = null;
            _appSettings = null;
            _eventAggregator = null;
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
            _authKeyManager.InitializeTestKeyPair();
            await ForceRequestNewCertificateAsync();

            ValidateAppSettings();
            await _apiClient.Received(1).RequestAuthCertificateAsync(Arg.Any<CertificateRequest>());
            AssertNumOfCallsToRecreateKeys(0);
        }

        private async Task ForceRequestNewCertificateAsync()
        {
            await _certificateManager.ForceRequestNewCertificateAsync();
            _testEndTime = DateTimeOffset.UtcNow;
        }

        private void ValidateAppSettings()
        {
            Assert.IsTrue(_testStartTime <= _appSettings.AuthenticationCertificateRequestUtcDate);
            Assert.IsTrue(_testEndTime >= _appSettings.AuthenticationCertificateRequestUtcDate);
            Assert.AreEqual(_certificateResponse.Certificate, _appSettings.AuthenticationCertificatePem);
            Assert.AreEqual(_certificateExpirationTime, _appSettings.AuthenticationCertificateExpirationUtcDate);
            Assert.AreEqual(_certificateRefreshTime, _appSettings.AuthenticationCertificateRefreshUtcDate);
            Assert.AreEqual(_certificateResponse.ServerPublicKey, _appSettings.CertificationServerPublicKey);
        }

        private void AssertNumOfCallsToRecreateKeys(int expectedNumOfCalls)
        {
            Assert.AreEqual(expectedNumOfCalls, _authKeyManager.MethodCalls[nameof(_authKeyManager.RegenerateKeyPair)]);
        }

        [TestMethod]
        public async Task TestForceRequestNewCertificateAsync_WhenNoClientPublicKeyExists()
        {
            await ForceRequestNewCertificateAsync();

            ValidateAppSettings();
            await _apiClient.Received(1).RequestAuthCertificateAsync(Arg.Any<CertificateRequest>());
            AssertNumOfCallsToRecreateKeys(1);
        }

        [TestMethod]
        public async Task TestForceRequestNewCertificateAsync_WhenPublicKeyWasAlreadyUsed()
        {
            _authKeyManager.InitializeTestKeyPair();
            _usedClientPublicKeys.Add(_authKeyManager.GetPublicKey().Pem);

            await ForceRequestNewCertificateAsync();

            ValidateAppSettings();
            await _apiClient.Received(2).RequestAuthCertificateAsync(Arg.Any<CertificateRequest>());
            AssertNumOfCallsToRecreateKeys(1);
        }

        [TestMethod]
        public async Task TestForceRequestNewCertificateAsync_WhenCalledMultipleTimes()
        {
            _authKeyManager.InitializeTestKeyPair();

            await ForceRequestNewCertificateAsync();
            await ForceRequestNewCertificateAsync();
            await ForceRequestNewCertificateAsync();

            ValidateAppSettings();
            await _apiClient.Received(5).RequestAuthCertificateAsync(Arg.Any<CertificateRequest>());
            AssertNumOfCallsToRecreateKeys(2);
        }

        [TestMethod]
        public async Task TestRequestNewCertificateAsync()
        {
            _authKeyManager.InitializeTestKeyPair();

            await RequestNewCertificateAsync();

            ValidateAppSettings();
            await _apiClient.Received(1).RequestAuthCertificateAsync(Arg.Any<CertificateRequest>());
            AssertNumOfCallsToRecreateKeys(0);
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

            ValidateAppSettings();
            await _apiClient.Received(1).RequestAuthCertificateAsync(Arg.Any<CertificateRequest>());
            AssertNumOfCallsToRecreateKeys(1);
        }

        [TestMethod]
        public async Task TestRequestNewCertificateAsync_WhenPublicKeyWasAlreadyUsed()
        {
            _authKeyManager.InitializeTestKeyPair();
            _usedClientPublicKeys.Add(_authKeyManager.GetPublicKey().Pem);

            await RequestNewCertificateAsync();

            ValidateAppSettings();
            await _apiClient.Received(2).RequestAuthCertificateAsync(Arg.Any<CertificateRequest>());
            AssertNumOfCallsToRecreateKeys(1);
        }

        [TestMethod]
        public async Task TestRequestNewCertificateAsync_WhenCalledMultipleTimes()
        {
            _authKeyManager.InitializeTestKeyPair();

            await RequestNewCertificateAsync();
            await RequestNewCertificateAsync();
            await RequestNewCertificateAsync();

            ValidateAppSettings();
            await _apiClient.Received(1).RequestAuthCertificateAsync(Arg.Any<CertificateRequest>());
            AssertNumOfCallsToRecreateKeys(0);
        }

        [TestMethod]
        public async Task TestRequestNewCertificateAsync_WhenCertificateNotExpired()
        {
            _appSettings.AuthenticationCertificateRefreshUtcDate = DateTimeOffset.UtcNow.AddHours(10);
            _authKeyManager.InitializeTestKeyPair();

            await RequestNewCertificateAsync();

            await _apiClient.Received(0).RequestAuthCertificateAsync(Arg.Any<CertificateRequest>());
        }

        [TestMethod]
        public async Task ItShouldNotDeleteOldCertIfCertRequestFailed()
        {
            _appSettings.AuthenticationCertificateRefreshUtcDate = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromHours(1));
            _authKeyManager.InitializeTestKeyPair();

            await RequestNewCertificateAsync();
            ValidateAppSettings();
            _apiClient.RequestAuthCertificateAsync(Arg.Any<CertificateRequest>())
                .Returns((args) => Task.FromResult(ApiResponseResult<CertificateResponse>.Fail(new HttpResponseMessage(HttpStatusCode.BadGateway), string.Empty)));

            _appSettings.AuthenticationCertificatePem.Should().NotBeEmpty();
        }
    }
}