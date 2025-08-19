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
using ProtonVPN.ProcessCommunication.Contracts.Entities.Crypto;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;
using ProtonVPN.ProcessCommunication.EntityMapping.Vpn;

namespace ProtonVPN.ProcessCommunication.EntityMapping.Tests.Vpn;

[TestClass]
public class VpnServerMapperTest
{
    private IEntityMapper _entityMapper;
    private VpnServerMapper _mapper;

    private ServerPublicKeyIpcEntity _expectedServerPublicKeyIpcEntity;
    private PublicKey _expectedPublicKey;

    [TestInitialize]
    public void Initialize()
    {
        _entityMapper = Substitute.For<IEntityMapper>();
        _mapper = new(_entityMapper);

        _expectedServerPublicKeyIpcEntity = new ServerPublicKeyIpcEntity();
        _entityMapper.Map<PublicKey, ServerPublicKeyIpcEntity>(Arg.Any<PublicKey>())
            .Returns(_expectedServerPublicKeyIpcEntity);

        _expectedPublicKey = new PublicKey("PVPN", KeyAlgorithm.Unknown);
        _entityMapper.Map<ServerPublicKeyIpcEntity, PublicKey>(Arg.Any<ServerPublicKeyIpcEntity>())
            .Returns(_expectedPublicKey);

        _entityMapper.Map<VpnProtocol, VpnProtocolIpcEntity>(Arg.Any<VpnProtocol>())
            .Returns(x => (VpnProtocolIpcEntity)(int)x.Arg<VpnProtocol>());
        _entityMapper.Map<VpnProtocolIpcEntity, VpnProtocol>(Arg.Any<VpnProtocolIpcEntity>())
            .Returns(x => (VpnProtocol)(int)x.Arg<VpnProtocolIpcEntity>());
    }

    [TestCleanup]
    public void Cleanup()
    {
        _entityMapper = null;
        _mapper = null;

        _expectedServerPublicKeyIpcEntity = null;
        _expectedPublicKey = null;
    }

    [TestMethod]
    public void TestMapLeftToRight_WithNullRelayIpByProtocol()
    {
        VpnHost entityToTest = new(
            name: "protonvpn.com",
            ip: "192.168.0.0",
            label: DateTime.UtcNow.Millisecond.ToString(),
            x25519PublicKey: new PublicKey("PVPN", KeyAlgorithm.Unknown),
            signature: DateTime.UtcNow.Ticks.ToString(),
            isIpv6Supported: false,
            relayIpByProtocol: null);

        VpnServerIpcEntity result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(entityToTest.Name, result.Name);
        Assert.AreEqual(entityToTest.Ip, result.Ip);
        Assert.AreEqual(entityToTest.Label, result.Label);
        Assert.AreEqual(_expectedServerPublicKeyIpcEntity, result.X25519PublicKey);
        Assert.AreEqual(entityToTest.Signature, result.Signature);
        Assert.IsNull(result.RelayIpByProtocol);
    }

    [TestMethod]
    public void TestMapLeftToRight_WithRelayIpByProtocol()
    {
        Dictionary<VpnProtocol, string> relayIpByProtocol = new()
        {
            { VpnProtocol.WireGuardUdp, "1.1.1.1" },
            { VpnProtocol.WireGuardTcp, "2.2.2.2" },
            { VpnProtocol.WireGuardTls, "3.3.3.3" },
            { VpnProtocol.OpenVpnUdp, "4.4.4.4" },
            { VpnProtocol.OpenVpnTcp, "5.5.5.5" }
        };

        VpnHost entityToTest = new(
            name: "protonvpn.com",
            ip: "192.168.0.0",
            label: DateTime.UtcNow.Millisecond.ToString(),
            x25519PublicKey: new PublicKey("PVPN", KeyAlgorithm.Unknown),
            signature: DateTime.UtcNow.Ticks.ToString(),
            isIpv6Supported: false,
            relayIpByProtocol: relayIpByProtocol);

        VpnServerIpcEntity result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(entityToTest.Name, result.Name);
        Assert.AreEqual(entityToTest.Ip, result.Ip);
        Assert.AreEqual(entityToTest.Label, result.Label);
        Assert.AreEqual(_expectedServerPublicKeyIpcEntity, result.X25519PublicKey);
        Assert.AreEqual(entityToTest.Signature, result.Signature);

        Assert.IsNotNull(result.RelayIpByProtocol);
        Assert.AreEqual(relayIpByProtocol.Count, result.RelayIpByProtocol.Count);
        Assert.AreEqual(relayIpByProtocol[VpnProtocol.WireGuardUdp], result.RelayIpByProtocol[VpnProtocolIpcEntity.WireGuardUdp]);
        Assert.AreEqual(relayIpByProtocol[VpnProtocol.WireGuardTcp], result.RelayIpByProtocol[VpnProtocolIpcEntity.WireGuardTcp]);
        Assert.AreEqual(relayIpByProtocol[VpnProtocol.WireGuardTls], result.RelayIpByProtocol[VpnProtocolIpcEntity.WireGuardTls]);
        Assert.AreEqual(relayIpByProtocol[VpnProtocol.OpenVpnUdp], result.RelayIpByProtocol[VpnProtocolIpcEntity.OpenVpnUdp]);
        Assert.AreEqual(relayIpByProtocol[VpnProtocol.OpenVpnTcp], result.RelayIpByProtocol[VpnProtocolIpcEntity.OpenVpnTcp]);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestMapRightToLeft_ThrowsWhenNull()
    {
        VpnServerIpcEntity entityToTest = null;

        _mapper.Map(entityToTest);
    }

    [TestMethod]
    public void TestMapRightToLeft_WithNullRelayIpByProtocol()
    {
        VpnServerIpcEntity entityToTest = new()
        {
            Name = "protonvpn.com",
            Ip = "192.168.0.0",
            Label = DateTime.UtcNow.Millisecond.ToString(),
            X25519PublicKey = new ServerPublicKeyIpcEntity(),
            Signature = DateTime.UtcNow.Ticks.ToString(),
            RelayIpByProtocol = null
        };

        VpnHost result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(entityToTest.Name, result.Name);
        Assert.AreEqual(entityToTest.Ip, result.Ip);
        Assert.AreEqual(entityToTest.Label, result.Label);
        Assert.AreEqual(_expectedPublicKey, result.X25519PublicKey);
        Assert.AreEqual(entityToTest.Signature, result.Signature);
        Assert.IsNull(result.RelayIpByProtocol);
    }

    [TestMethod]
    public void TestMapRightToLeft_WithRelayIpByProtocol()
    {
        Dictionary<VpnProtocolIpcEntity, string> relayIpByProtocol = new()
        {
            { VpnProtocolIpcEntity.WireGuardUdp, "1.1.1.1" },
            { VpnProtocolIpcEntity.WireGuardTcp, "2.2.2.2" },
            { VpnProtocolIpcEntity.WireGuardTls, "3.3.3.3" },
            { VpnProtocolIpcEntity.OpenVpnUdp, "4.4.4.4" },
            { VpnProtocolIpcEntity.OpenVpnTcp, "5.5.5.5" }
        };

        VpnServerIpcEntity entityToTest = new()
        {
            Name = "protonvpn.com",
            Ip = "192.168.0.0",
            Label = DateTime.UtcNow.Millisecond.ToString(),
            X25519PublicKey = new ServerPublicKeyIpcEntity(),
            Signature = DateTime.UtcNow.Ticks.ToString(),
            RelayIpByProtocol = relayIpByProtocol
        };

        VpnHost result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(entityToTest.Name, result.Name);
        Assert.AreEqual(entityToTest.Ip, result.Ip);
        Assert.AreEqual(entityToTest.Label, result.Label);
        Assert.AreEqual(_expectedPublicKey, result.X25519PublicKey);
        Assert.AreEqual(entityToTest.Signature, result.Signature);

        Assert.IsNotNull(result.RelayIpByProtocol);
        Assert.AreEqual(relayIpByProtocol.Count, result.RelayIpByProtocol.Count);
        Assert.AreEqual(relayIpByProtocol[VpnProtocolIpcEntity.WireGuardUdp], result.RelayIpByProtocol[VpnProtocol.WireGuardUdp]);
        Assert.AreEqual(relayIpByProtocol[VpnProtocolIpcEntity.WireGuardTcp], result.RelayIpByProtocol[VpnProtocol.WireGuardTcp]);
        Assert.AreEqual(relayIpByProtocol[VpnProtocolIpcEntity.WireGuardTls], result.RelayIpByProtocol[VpnProtocol.WireGuardTls]);
        Assert.AreEqual(relayIpByProtocol[VpnProtocolIpcEntity.OpenVpnUdp], result.RelayIpByProtocol[VpnProtocol.OpenVpnUdp]);
        Assert.AreEqual(relayIpByProtocol[VpnProtocolIpcEntity.OpenVpnTcp], result.RelayIpByProtocol[VpnProtocol.OpenVpnTcp]);
    }
}