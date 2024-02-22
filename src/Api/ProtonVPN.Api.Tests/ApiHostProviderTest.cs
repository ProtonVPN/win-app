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
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Settings;

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
        private IVpnStatusNotifier _vpnStatusNotifier;

        [TestInitialize]
        public void Initialize()
        {
            _config = new Config { Urls = { ApiUrl = API_URL } };
            _appSettings = Substitute.For<IAppSettings>();
            _vpnStatusNotifier = Substitute.For<IVpnStatusNotifier>();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _config = null;
            _appSettings = null;
            _vpnStatusNotifier = null;
        }

        [TestMethod]
        public void ItShouldReturnAlternativeApiHostWhenProxyIsActivated()
        {
            // Arrange
            _appSettings.DoHEnabled.Returns(true);
            _appSettings.LastPrimaryApiFailDateUtc.Returns(DateTime.UtcNow.Subtract(TimeSpan.FromHours(2)));
            _appSettings.ActiveAlternativeApiBaseUrl.Returns(PROXY_HOST);

            ApiHostProvider sut = new(_appSettings, _config, _vpnStatusNotifier);
            RaiseVpnStatusEvent(VpnStatus.Disconnected);

            // Assert
            sut.IsProxyActive().Should().BeTrue();
        }

        [TestMethod]
        public void ProxyShouldBeDisabledWhenConnectedToVpn()
        {
            // Arrange
            _appSettings.DoHEnabled.Returns(true);
            _appSettings.LastPrimaryApiFailDateUtc.Returns(DateTime.UtcNow.Subtract(TimeSpan.FromHours(2)));
            _appSettings.ActiveAlternativeApiBaseUrl.Returns(PROXY_HOST);
            ApiHostProvider sut = new(_appSettings, _config, _vpnStatusNotifier);
            RaiseVpnStatusEvent(VpnStatus.Connected);

            // Assert
            sut.IsProxyActive().Should().BeFalse();
        }

        [TestMethod]
        public void ProxyShouldBeDisabledAfter24Hours()
        {
            // Arrange
            _appSettings.DoHEnabled.Returns(true);
            _appSettings.LastPrimaryApiFailDateUtc.Returns(DateTime.UtcNow.Subtract(TimeSpan.FromHours(25)));
            _appSettings.ActiveAlternativeApiBaseUrl.Returns(PROXY_HOST);
            ApiHostProvider sut = new(_appSettings, _config, _vpnStatusNotifier);
            RaiseVpnStatusEvent(VpnStatus.Disconnected);

            // Assert
            sut.IsProxyActive().Should().BeFalse();
        }

        [TestMethod]
        public void ItShouldReturnAlternativeHostWhenProxyIsActive()
        {
            // Arrange
            _config.Urls.ApiUrl = API_URL;
            _appSettings.ActiveAlternativeApiBaseUrl.Returns(PROXY_HOST);
            _appSettings.DoHEnabled.Returns(true);
            _appSettings.LastPrimaryApiFailDateUtc.Returns(DateTime.UtcNow.Subtract(TimeSpan.FromHours(2)));
            ApiHostProvider sut = new(_appSettings, _config, _vpnStatusNotifier);
            RaiseVpnStatusEvent(VpnStatus.Disconnected);

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
            ApiHostProvider sut = new(_appSettings, _config, _vpnStatusNotifier);

            // Assert
            sut.GetHost().Should().Be(API_HOST);
        }

        private void RaiseVpnStatusEvent(VpnStatus status)
        {
            _vpnStatusNotifier.VpnStatusChanged += Raise.Event<EventHandler<VpnStatus>>(this, status);
        }
    }
}