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
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;

namespace ProtonVPN.Api.Tests
{
    [TestClass]
    public class ApiHostProviderTest
    {
        private const string API_HOST = "api.protonvpn.ch";
        private const string API_URL = "https://" + API_HOST;
        private const string PROXY_HOST = "alternative.api.url";
        private IConfiguration _config;
        private IAppSettings _appSettings;

        [TestInitialize]
        public void Initialize()
        {
            _config = new Config() { Urls = { ApiUrl = API_URL } };
            _appSettings = Substitute.For<IAppSettings>();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _config = null;
            _appSettings = null;
        }

        [TestMethod]
        public async Task ItShouldReturnAlternativeApiHostWhenProxyIsActivated()
        {
            // Arrange
            _appSettings.DoHEnabled.Returns(true);
            _appSettings.LastPrimaryApiFailDateUtc.Returns(DateTime.UtcNow.Subtract(TimeSpan.FromHours(2)));
            _appSettings.ActiveAlternativeApiBaseUrl.Returns(PROXY_HOST);
            ApiHostProvider sut = new(_appSettings, _config);
            await sut.OnVpnStateChanged(new VpnStateChangedEventArgs(new VpnState(VpnStatus.Disconnected), VpnError.None,
                false));

            // Assert
            sut.IsProxyActive().Should().BeTrue();
        }

        [TestMethod]
        public async Task ProxyShouldBeDisabledWhenConnectedToVpn()
        {
            // Arrange
            _appSettings.DoHEnabled.Returns(true);
            _appSettings.LastPrimaryApiFailDateUtc.Returns(DateTime.UtcNow.Subtract(TimeSpan.FromHours(2)));
            _appSettings.ActiveAlternativeApiBaseUrl.Returns(PROXY_HOST);
            ApiHostProvider sut = new(_appSettings, _config);
            await sut.OnVpnStateChanged(new VpnStateChangedEventArgs(new VpnState(VpnStatus.Connected), VpnError.None,
                false));

            // Assert
            sut.IsProxyActive().Should().BeFalse();
        }

        [TestMethod]
        public async Task ProxyShouldBeDisabledAfter24Hours()
        {
            // Arrange
            _appSettings.DoHEnabled.Returns(true);
            _appSettings.LastPrimaryApiFailDateUtc.Returns(DateTime.UtcNow.Subtract(TimeSpan.FromHours(25)));
            _appSettings.ActiveAlternativeApiBaseUrl.Returns(PROXY_HOST);
            ApiHostProvider sut = new(_appSettings, _config);
            await sut.OnVpnStateChanged(new VpnStateChangedEventArgs(new VpnState(VpnStatus.Disconnected), VpnError.None,
                false));

            // Assert
            sut.IsProxyActive().Should().BeFalse();
        }

        [TestMethod]
        public async Task ItShouldReturnAlternativeHostWhenProxyIsActive()
        {
            // Arrange
            _config.Urls.ApiUrl = API_URL;
            _appSettings.ActiveAlternativeApiBaseUrl.Returns(PROXY_HOST);
            _appSettings.DoHEnabled.Returns(true);
            _appSettings.LastPrimaryApiFailDateUtc.Returns(DateTime.UtcNow.Subtract(TimeSpan.FromHours(2)));
            ApiHostProvider sut = new(_appSettings, _config);
            await sut.OnVpnStateChanged(new VpnStateChangedEventArgs(new VpnState(VpnStatus.Disconnected), VpnError.None,
                false));

            // Assert
            sut.GetHost().Should().Be(PROXY_HOST);
        }

        [TestMethod]
        public void ItShouldReturnApiHostWhenProxyIsDisabled()
        {
            // Arrange
            _config.Urls.ApiUrl = API_URL;
            _appSettings.ActiveAlternativeApiBaseUrl.Returns(PROXY_HOST);
            _appSettings.DoHEnabled.Returns(false);
            ApiHostProvider sut = new(_appSettings, _config);

            // Assert
            sut.GetHost().Should().Be(API_HOST);
        }
    }
}