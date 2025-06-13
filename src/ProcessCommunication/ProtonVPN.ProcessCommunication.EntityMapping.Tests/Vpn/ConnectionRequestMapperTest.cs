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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Crypto.Contracts;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;
using ProtonVPN.ProcessCommunication.EntityMapping.Vpn;

namespace ProtonVPN.ProcessCommunication.EntityMapping.Tests.Vpn;

[TestClass]
public class ConnectionRequestMapperTest
{
    private IEntityMapper _entityMapper;
    private ConnectionRequestMapper _mapper;

    private List<VpnServerIpcEntity> _expectedVpnServerIpcEntities;
    private VpnProtocolIpcEntity? _expectedVpnProtocolIpcEntity;
    private VpnConfigIpcEntity _expectedVpnConfigIpcEntity;
    private VpnCredentialsIpcEntity _expectedVpnCredentialsIpcEntity;
    private List<VpnHost> _expectedVpnHosts;
    private VpnProtocol? _expectedVpnProtocol;
    private VpnConfig _expectedVpnConfig;
    private VpnCredentials? _expectedVpnCredentials;

    [TestInitialize]
    public void Initialize()
    {
        _entityMapper = Substitute.For<IEntityMapper>();
        _mapper = new(_entityMapper);

        _expectedVpnServerIpcEntities = new List<VpnServerIpcEntity>() { new VpnServerIpcEntity() };
        _entityMapper.Map<VpnHost, VpnServerIpcEntity>(Arg.Any<IEnumerable<VpnHost>>())
            .Returns(_expectedVpnServerIpcEntities);

        _expectedVpnProtocolIpcEntity = VpnProtocolIpcEntity.OpenVpnUdp;
        _entityMapper.Map<VpnProtocol, VpnProtocolIpcEntity>(Arg.Any<VpnProtocol>())
            .Returns(_expectedVpnProtocolIpcEntity.Value);

        _expectedVpnConfigIpcEntity = new VpnConfigIpcEntity();
        _entityMapper.Map<VpnConfig, VpnConfigIpcEntity>(Arg.Any<VpnConfig>())
            .Returns(_expectedVpnConfigIpcEntity);

        _expectedVpnCredentialsIpcEntity = new VpnCredentialsIpcEntity();
        _entityMapper.Map<VpnCredentials, VpnCredentialsIpcEntity>(Arg.Any<VpnCredentials>())
            .Returns(_expectedVpnCredentialsIpcEntity);

        _expectedVpnHosts = new List<VpnHost>();
        _entityMapper.Map<VpnServerIpcEntity, VpnHost>(Arg.Any<IEnumerable<VpnServerIpcEntity>>())
            .Returns(_expectedVpnHosts);

        _expectedVpnProtocol = VpnProtocol.OpenVpnUdp;
        _entityMapper.Map<VpnProtocolIpcEntity, VpnProtocol>(Arg.Any<VpnProtocolIpcEntity>())
            .Returns(_expectedVpnProtocol.Value);

        _expectedVpnConfig = new VpnConfig(new VpnConfigParameters());
        _entityMapper.Map<VpnConfigIpcEntity, VpnConfig>(Arg.Any<VpnConfigIpcEntity>())
            .Returns(_expectedVpnConfig);

        _expectedVpnCredentials = new VpnCredentials();
        _entityMapper.Map<VpnCredentialsIpcEntity, VpnCredentials>(Arg.Any<VpnCredentialsIpcEntity>())
            .Returns(_expectedVpnCredentials.Value);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _entityMapper = null;
        _mapper = null;

        _expectedVpnServerIpcEntities = null;
        _expectedVpnProtocolIpcEntity = null;
        _expectedVpnConfigIpcEntity = null;
        _expectedVpnCredentialsIpcEntity = null;
        _expectedVpnHosts = null;
        _expectedVpnProtocol = null;
        _expectedVpnConfig = null;
        _expectedVpnCredentials = null;
    }

    [TestMethod]
    public void TestMapLeftToRight_WhenNull()
    {
        VpnConnectionRequest entityToTest = null;

        ConnectionRequestIpcEntity result = _mapper.Map(entityToTest);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void TestMapLeftToRight()
    {
        VpnConnectionRequest entityToTest = new(
            new List<VpnHost>(),
            VpnProtocol.OpenVpnUdp,
            new VpnConfig(new VpnConfigParameters()),
            new VpnCredentials(string.Empty, DateTime.UtcNow.AddDays(1), new AsymmetricKeyPair(
                new SecretKey("PVPN", KeyAlgorithm.Unknown),
                new PublicKey("PVPN", KeyAlgorithm.Unknown)),
                "username",
                "password"));


        ConnectionRequestIpcEntity result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(_expectedVpnServerIpcEntities.Count, result.Servers.Count());
        Assert.AreEqual(_expectedVpnServerIpcEntities.Single(), result.Servers.Single());
        Assert.AreEqual(_expectedVpnProtocolIpcEntity, result.Protocol);
        Assert.AreEqual(_expectedVpnConfigIpcEntity, result.Config);
        Assert.AreEqual(_expectedVpnCredentialsIpcEntity, result.Credentials);
        Assert.IsNotNull(result.Settings);
    }

    [TestMethod]
    public void TestMapRightToLeft_WhenNull()
    {
        ConnectionRequestIpcEntity entityToTest = null;

        VpnConnectionRequest result = _mapper.Map(entityToTest);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void TestMapRightToLeft()
    {
        ConnectionRequestIpcEntity entityToTest = new();

        VpnConnectionRequest result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(_expectedVpnHosts, result.Servers);
        Assert.AreEqual(_expectedVpnProtocol, result.VpnProtocol);
        Assert.AreEqual(_expectedVpnConfig, result.Config);
        Assert.AreEqual(_expectedVpnCredentials, result.Credentials);
    }
}