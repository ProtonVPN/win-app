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
    public void TestMapLeftToRight()
    {
        VpnHost entityToTest = new(
            name: "protonvpn.com",
            ip: "192.168.0.0",
            label: DateTime.UtcNow.Millisecond.ToString(),
            x25519PublicKey: new PublicKey("PVPN", KeyAlgorithm.Unknown),
            signature: DateTime.UtcNow.Ticks.ToString());

        VpnServerIpcEntity result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(entityToTest.Name, result.Name);
        Assert.AreEqual(entityToTest.Ip, result.Ip);
        Assert.AreEqual(entityToTest.Label, result.Label);
        Assert.AreEqual(_expectedServerPublicKeyIpcEntity, result.X25519PublicKey);
        Assert.AreEqual(entityToTest.Signature, result.Signature);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestMapRightToLeft_ThrowsWhenNull()
    {
        VpnServerIpcEntity entityToTest = null;

        _mapper.Map(entityToTest);
    }

    [TestMethod]
    public void TestMapRightToLeft()
    {
        VpnServerIpcEntity entityToTest = new()
        {
            Name = "protonvpn.com",
            Ip = "192.168.0.0",
            Label = DateTime.UtcNow.Millisecond.ToString(),
            X25519PublicKey = new ServerPublicKeyIpcEntity(),
            Signature = DateTime.UtcNow.Ticks.ToString()
        };

        VpnHost result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(entityToTest.Name, result.Name);
        Assert.AreEqual(entityToTest.Ip, result.Ip);
        Assert.AreEqual(entityToTest.Label, result.Label);
        Assert.AreEqual(_expectedPublicKey, result.X25519PublicKey);
        Assert.AreEqual(entityToTest.Signature, result.Signature);
    }
}