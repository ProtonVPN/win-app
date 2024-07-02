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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Configurations.Contracts.Entities;

namespace ProtonVPN.Api.Tests;

[TestClass]
public class ApiHostProviderTest
{
    private const string API_HOST = "api.protonvpn.ch";
    private const string API_URL = "https://" + API_HOST;
    private const string PROXY_HOST = "alternative.api.url";
    private IConfiguration _config;
    private ISettings _settings;
    private IConnectionManager _connectionManager;

    [TestInitialize]
    public void Initialize()
    {
        _config = CreateConfiguration();
        _settings = Substitute.For<ISettings>();
        _connectionManager = Substitute.For<IConnectionManager>();
    }

    private IConfiguration CreateConfiguration()
    {
        IUrlsConfiguration urlsConfig = Substitute.For<IUrlsConfiguration>();
        urlsConfig.ApiUrl.Returns(API_URL);
        IConfiguration config = Substitute.For<IConfiguration>();
        config.Urls.Returns(urlsConfig);
        return config;
    }

    [TestCleanup]
    public void Cleanup()
    {
        _config = null;
        _settings = null;
    }

    [TestMethod]
    public void ItShouldReturnAlternativeApiHostWhenProxyIsActivated()
    {
        // Arrange
        _settings.IsAlternativeRoutingEnabled.Returns(true);
        _settings.ActiveAlternativeApiBaseUrl.Returns(PROXY_HOST);
        _connectionManager.IsDisconnected.Returns(true);
        ApiHostProvider sut = new(_settings, _config, _connectionManager);

        // Assert
        sut.IsProxyActive().Should().BeTrue();
    }

    [TestMethod]
    public void ProxyShouldBeDisabledWhenConnectedToVpn()
    {
        // Arrange
        _settings.IsAlternativeRoutingEnabled.Returns(true);
        _settings.ActiveAlternativeApiBaseUrl.Returns(PROXY_HOST);
        _connectionManager.IsDisconnected.Returns(false);
        ApiHostProvider sut = new(_settings, _config, _connectionManager);

        // Assert
        sut.IsProxyActive().Should().BeFalse();
    }

    [TestMethod]
    public void ProxyShouldBeDisabledWhenAlternativeHostIsEmpty()
    {
        // Arrange
        _settings.IsAlternativeRoutingEnabled.Returns(true);
        _settings.ActiveAlternativeApiBaseUrl.Returns(string.Empty);
        _connectionManager.IsDisconnected.Returns(true);
        ApiHostProvider sut = new(_settings, _config, _connectionManager);

        // Assert
        sut.IsProxyActive().Should().BeFalse();
    }

    [TestMethod]
    public void ItShouldReturnAlternativeHostWhenProxyIsActive()
    {
        // Arrange
        _config.Urls.ApiUrl.Returns(API_URL);
        _settings.ActiveAlternativeApiBaseUrl.Returns(PROXY_HOST);
        _settings.IsAlternativeRoutingEnabled.Returns(true);
        _connectionManager.IsDisconnected.Returns(true);
        ApiHostProvider sut = new(_settings, _config, _connectionManager);

        // Assert
        sut.GetBaseUri().Should().Be($"https://{PROXY_HOST}/");
    }

    [TestMethod]
    public void ItShouldReturnApiHostWhenProxyIsDisabled()
    {
        // Arrange
        _config.Urls.ApiUrl.Returns(API_URL);
        _settings.ActiveAlternativeApiBaseUrl.Returns(PROXY_HOST);
        _settings.IsAlternativeRoutingEnabled.Returns(false);
        ApiHostProvider sut = new(_settings, _config, _connectionManager);

        // Assert
        sut.GetBaseUri().Should().Be($"https://{API_HOST}/");
    }
}