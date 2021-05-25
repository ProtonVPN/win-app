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
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Caliburn.Micro;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Configuration.Source;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Core.Api;
using ProtonVPN.Core.Api.Certificates;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Core.Test.Auth
{
    [TestClass]
    public class AuthCertificateManagerTest
    {
        private Common.Configuration.Config _config;
        private MockOfAuthKeyManager _authKeyManager;
        private IApiClient _apiClient;
        private IAppSettings _appSettings;
        private IEventAggregator _eventAggregator;
        private ILogger _logger;
        private IAuthCertificateManager _certificateManager;

        private DateTimeOffset? _certificateExpirationTime;
        private DateTimeOffset? _certificateRefreshTime;
        private DateTimeOffset? _testStartTime;
        private DateTimeOffset? _testEndTime;
        private CertificateResponseData _certificateResponseData;
        private IList<string> _usedClientPublicKeys = new List<string>();

        [TestInitialize]  
        public void Initialize()  
        {  
            _certificateExpirationTime = DateTimeOffset.UtcNow.AddHours(24).TruncateToSeconds();
            _certificateRefreshTime = DateTimeOffset.UtcNow.AddHours(18).TruncateToSeconds();
            _testStartTime = DateTimeOffset.UtcNow;
            
            _config = new DefaultConfig().Value();
            _authKeyManager = new MockOfAuthKeyManager();
            _apiClient = Substitute.For<IApiClient>();
            _apiClient.RequestAuthCertificateAsync(Arg.Any<CertificateRequestData>())
                .Returns(async (args) => await MockOfRequestAuthCertificateAsync(args.Arg<CertificateRequestData>()));
            _appSettings = Substitute.For<IAppSettings>();
            _logger = Substitute.For<ILogger>();
            _eventAggregator = Substitute.For<IEventAggregator>();
            _certificateManager = new AuthCertificateManager(_config, _authKeyManager, _apiClient, _appSettings, _logger, _eventAggregator);
        }

        private async Task<ApiResponseResult<CertificateResponseData>> MockOfRequestAuthCertificateAsync(CertificateRequestData arg)
        {
            if (arg.ClientPublicKey.IsNullOrEmpty() || _usedClientPublicKeys.Contains(arg.ClientPublicKey))
            {
                return ApiResponseResult<CertificateResponseData>.Fail(CreateClientPublicKeyConflictCertificateResponseData(),
                    HttpStatusCode.BadRequest, string.Empty);
            }
            _usedClientPublicKeys.Add(arg.ClientPublicKey);
            _certificateResponseData ??= CreateCertificateResponseData();
            return ApiResponseResult<CertificateResponseData>.Ok(_certificateResponseData);
        }

        private CertificateResponseData CreateCertificateResponseData()
        {
            return new CertificateResponseData()
            {
                Certificate = "--Certificate--",
                ExpirationTime = _certificateExpirationTime.Value.ToUnixTimeSeconds(),
                RefreshTime = _certificateRefreshTime.Value.ToUnixTimeSeconds(),
                ServerPublicKey = "--Server Public Key--"
            };
        }

        private CertificateResponseData CreateClientPublicKeyConflictCertificateResponseData()
        {
            return new CertificateResponseData()
            {
                Code = ResponseCodes.ClientPublicKeyConflict
            };
        }

        [TestCleanup]  
        public void Cleanup()  
        {  
            _config = null;
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
            _certificateResponseData = null;
            _usedClientPublicKeys = null;
        }

        [TestMethod]
        public async Task TestForceRequestNewCertificateAsync()
        {
            _authKeyManager.InitializeTestKeyPair();
            await ForceRequestNewCertificateAsync();
            
            ValidateAppSettings();
            await _apiClient.Received(1).RequestAuthCertificateAsync(Arg.Any<CertificateRequestData>());
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
            Assert.AreEqual(_certificateResponseData.Certificate, _appSettings.AuthenticationCertificatePem);
            Assert.AreEqual(_certificateExpirationTime, _appSettings.AuthenticationCertificateExpirationUtcDate);
            Assert.AreEqual(_certificateRefreshTime, _appSettings.AuthenticationCertificateRefreshUtcDate);
            Assert.AreEqual(_certificateResponseData.ServerPublicKey, _appSettings.CertificationServerPublicKey);
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
            await _apiClient.Received(1).RequestAuthCertificateAsync(Arg.Any<CertificateRequestData>());
            AssertNumOfCallsToRecreateKeys(1);
        }

        [TestMethod]
        public async Task TestForceRequestNewCertificateAsync_WhenPublicKeyWasAlreadyUsed()
        {
            _authKeyManager.InitializeTestKeyPair();
            _usedClientPublicKeys.Add(_authKeyManager.GetPublicKey().Pem);

            await ForceRequestNewCertificateAsync();

            ValidateAppSettings();
            await _apiClient.Received(2).RequestAuthCertificateAsync(Arg.Any<CertificateRequestData>());
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
            await _apiClient.Received(5).RequestAuthCertificateAsync(Arg.Any<CertificateRequestData>());
            AssertNumOfCallsToRecreateKeys(2);
        }

        [TestMethod]
        public async Task TestRequestNewCertificateAsync()
        {
            _authKeyManager.InitializeTestKeyPair();

            await RequestNewCertificateAsync();
            
            ValidateAppSettings();
            await _apiClient.Received(1).RequestAuthCertificateAsync(Arg.Any<CertificateRequestData>());
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
            await _apiClient.Received(1).RequestAuthCertificateAsync(Arg.Any<CertificateRequestData>());
            AssertNumOfCallsToRecreateKeys(1);
        }

        [TestMethod]
        public async Task TestRequestNewCertificateAsync_WhenPublicKeyWasAlreadyUsed()
        {
            _authKeyManager.InitializeTestKeyPair();
            _usedClientPublicKeys.Add(_authKeyManager.GetPublicKey().Pem);

            await RequestNewCertificateAsync();

            ValidateAppSettings();
            await _apiClient.Received(2).RequestAuthCertificateAsync(Arg.Any<CertificateRequestData>());
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
            await _apiClient.Received(1).RequestAuthCertificateAsync(Arg.Any<CertificateRequestData>());
            AssertNumOfCallsToRecreateKeys(0);
        }
    }
}