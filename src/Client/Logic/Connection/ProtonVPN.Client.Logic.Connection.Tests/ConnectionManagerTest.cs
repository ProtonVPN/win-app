﻿/*
 * Copyright (c) 2024 Proton AG
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

using NSubstitute;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.GuestHole;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Connection.Contracts.RequestCreators;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Auth;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Crypto;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Connection.Tests;

[TestClass]
public class ConnectionManagerTest
{
    private const int PAID_PLAN_TIER = 2;

    private ILogger? _logger;
    private ISettings? _settings;
    private IVpnServiceCaller? _vpnServiceCaller;
    private IEventMessageSender? _eventMessageSender;
    private IEntityMapper? _entityMapper;
    private IConnectionRequestCreator? _connectionRequestCreator;
    private IReconnectionRequestCreator? _reconnectionRequestCreator;
    private IDisconnectionRequestCreator? _disconnectionRequestCreator;
    private IServersLoader? _serversLoader;
    private IGuestHoleManager? _guestHoleManager;

    [TestInitialize]
    public void Initialize()
    {
        _logger = Substitute.For<ILogger>();
        _settings = Substitute.For<ISettings>();
        _vpnServiceCaller = Substitute.For<IVpnServiceCaller>();
        _eventMessageSender = Substitute.For<IEventMessageSender>();
        _entityMapper = Substitute.For<IEntityMapper>();
        _connectionRequestCreator = Substitute.For<IConnectionRequestCreator>();
        _reconnectionRequestCreator = Substitute.For<IReconnectionRequestCreator>();
        _disconnectionRequestCreator = Substitute.For<IDisconnectionRequestCreator>();
        _serversLoader = Substitute.For<IServersLoader>();
        _guestHoleManager = Substitute.For<IGuestHoleManager>();

        _connectionRequestCreator!.CreateAsync(Arg.Any<IConnectionIntent>()).Returns(GetConnectionRequestIpcEntity());
        _reconnectionRequestCreator!.CreateAsync(Arg.Any<IConnectionIntent>()).Returns(GetConnectionRequestIpcEntity());
    }

    [TestCleanup]
    public virtual void Cleanup()
    {
        _logger = null;
        _settings = null;
        _vpnServiceCaller = null;
        _eventMessageSender = null;
        _entityMapper = null;
        _connectionRequestCreator = null;
        _reconnectionRequestCreator = null;
        _disconnectionRequestCreator = null;
        _serversLoader = null;
        _guestHoleManager = null;
    }

    [TestMethod]
    [DataRow(typeof(SecureCoreFeatureIntent))]
    [DataRow(typeof(TorFeatureIntent))]
    public async Task ConnectAsync_Should_ChangeConnectionIntentWhenPortForwardingEnabledAsync(Type featureIntentType)
    {
        // Arrange
        _settings!.IsPortForwardingEnabled.Returns(true);
        _settings!.VpnPlan.Returns(new VpnPlan(string.Empty, string.Empty, PAID_PLAN_TIER));

        ConnectionManager connectionManager = GetConnectionManager();
        IFeatureIntent featureIntent = GetFeatureIntent(featureIntentType);
        IConnectionIntent connectionIntent = GetConnectionIntent(featureIntent);

        // Act
        await connectionManager.ConnectAsync(connectionIntent);

        // Assert
        Assert.IsFalse(connectionManager.CurrentConnectionIntent?.IsSameAs(connectionIntent));
        Assert.IsTrue(connectionManager.CurrentConnectionIntent?.Feature is P2PFeatureIntent);
    }

    [TestMethod]
    [DataRow(typeof(SecureCoreFeatureIntent))]
    [DataRow(typeof(TorFeatureIntent))]
    public async Task ConnectAsync_ShouldNot_ChangeConnectionIntentWhenPortForwardingDisabledAsync(Type featureIntentType)
    {
        // Arrange
        _settings!.IsPortForwardingEnabled.Returns(false);
        _settings!.VpnPlan.Returns(new VpnPlan(string.Empty, string.Empty, PAID_PLAN_TIER));

        ConnectionManager connectionManager = GetConnectionManager();
        IFeatureIntent featureIntent = GetFeatureIntent(featureIntentType);
        IConnectionIntent connectionIntent = GetConnectionIntent(featureIntent);

        // Act
        await connectionManager.ConnectAsync(connectionIntent);

        // Assert
        Assert.IsTrue(connectionManager.CurrentConnectionIntent?.IsSameAs(connectionIntent));
    }

    [TestMethod]
    [DataRow(typeof(SecureCoreFeatureIntent))]
    [DataRow(typeof(TorFeatureIntent))]
    public async Task ReconnectAsync_Should_ChangeConnectionIntentWhenPortForwardingEnabledAsync(Type featureIntentType)
    {
        // Arrange
        _settings!.IsPortForwardingEnabled.Returns(true);
        _settings!.VpnPlan.Returns(new VpnPlan(string.Empty, string.Empty, PAID_PLAN_TIER));

        ConnectionManager connectionManager = GetConnectionManager();
        IFeatureIntent featureIntent = GetFeatureIntent(featureIntentType);
        IConnectionIntent connectionIntent = GetConnectionIntent(featureIntent);

        // Act
        await connectionManager.ConnectAsync(connectionIntent);
        await connectionManager.ReconnectAsync();

        // Assert
        Assert.IsFalse(connectionManager.CurrentConnectionIntent?.IsSameAs(connectionIntent));
        Assert.IsTrue(connectionManager.CurrentConnectionIntent?.Feature is P2PFeatureIntent);
    }

    private ConnectionRequestIpcEntity GetConnectionRequestIpcEntity()
    {
        return new ConnectionRequestIpcEntity()
        {
            Servers = [new VpnServerIpcEntity()],
            Credentials = new VpnCredentialsIpcEntity
            {
                Certificate = new ConnectionCertificateIpcEntity()
                {
                    Pem = "pem",
                    ExpirationDateUtc = DateTime.Now.AddDays(1)
                },
                ClientKeyPair = new AsymmetricKeyPairIpcEntity
                {
                    PublicKey = new PublicKeyIpcEntity(),
                    SecretKey = new SecretKeyIpcEntity(),
                }
            }
        };
    }

    private ConnectionManager GetConnectionManager()
    {
        return new(
            _logger!,
            _settings!,
            _vpnServiceCaller!,
            _eventMessageSender!,
            _entityMapper!,
            _connectionRequestCreator!,
            _reconnectionRequestCreator!,
            _disconnectionRequestCreator!,
            _serversLoader!,
            _guestHoleManager!);
    }

    private IConnectionIntent GetConnectionIntent(IFeatureIntent featureIntent)
    {
        return new ConnectionIntent(new CountryLocationIntent("US"), featureIntent);
    }

    private IFeatureIntent GetFeatureIntent(Type type)
    {
        return (Activator.CreateInstance(type) as IFeatureIntent)!;
    }
}