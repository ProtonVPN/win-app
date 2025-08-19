/*
 * Copyright (c) 2025 Proton AG
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
using ProtonVPN.Common.Core.Vpn;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;
using ProtonVPN.ProcessCommunication.EntityMapping.Vpn;

namespace ProtonVPN.ProcessCommunication.EntityMapping.Tests.Vpn;

[TestClass]
public class ConnectionDetailsMapperTest
{
    private IEntityMapper _entityMapper;
    private ConnectionDetailsMapper _mapper;

    private IpAddressInfo _expectedServerAddress;
    private VpnServerAddressIpcEntity _expectedServerAddressIpcEntity;

    [TestInitialize]
    public void Initialize()
    {
        _entityMapper = Substitute.For<IEntityMapper>();
        _mapper = new(_entityMapper);

        _expectedServerAddress = new()
        {
            Ipv4Address = "127.0.0.1",
            Ipv6Address = "::1",
        };

        _expectedServerAddressIpcEntity = new()
        {
            Ipv4Address = "127.0.0.1",
            Ipv6Address = "::1",
        };

        _entityMapper.Map<VpnServerAddressIpcEntity, IpAddressInfo>(Arg.Any<VpnServerAddressIpcEntity>())
            .Returns(_expectedServerAddress);

        _entityMapper.Map<IpAddressInfo, VpnServerAddressIpcEntity>(Arg.Any<IpAddressInfo>())
            .Returns(_expectedServerAddressIpcEntity);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _entityMapper = null;
        _mapper = null;

        _expectedServerAddressIpcEntity = null;
    }

    [TestMethod]
    public void TestMapLeftToRight_WhenNull()
    {
        ConnectionDetails entityToTest = null;

        ConnectionDetailsIpcEntity result = _mapper.Map(entityToTest);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void TestMapLeftToRight()
    {
        ConnectionDetails entityToTest = new()
        {
            ClientIpAddress = $"A {DateTime.UtcNow}",
            ClientCountryIsoCode = $"B {DateTime.UtcNow}",
            ServerIpAddress = _expectedServerAddress,
        };

        ConnectionDetailsIpcEntity result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(entityToTest.ClientIpAddress, result.ClientIpAddress);
        Assert.AreEqual(_expectedServerAddressIpcEntity.Ipv4Address, result.ServerIpAddress.Ipv4Address);
        Assert.AreEqual(_expectedServerAddressIpcEntity.Ipv6Address, result.ServerIpAddress.Ipv6Address);
        Assert.AreEqual(entityToTest.ClientCountryIsoCode, result.ClientCountryIsoCode);
    }

    [TestMethod]
    public void TestMapRightToLeft_WhenNull()
    {
        ConnectionDetailsIpcEntity entityToTest = null;

        ConnectionDetails result = _mapper.Map(entityToTest);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void TestMapRightToLeft()
    {
        ConnectionDetailsIpcEntity entityToTest = new()
        {
            ClientIpAddress = $"A {DateTime.UtcNow}",
            ClientCountryIsoCode = $"B {DateTime.UtcNow}",
            ServerIpAddress = _expectedServerAddressIpcEntity,
        };

        ConnectionDetails result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(entityToTest.ClientIpAddress, result.ClientIpAddress);
        Assert.AreEqual(_expectedServerAddress.Ipv4Address, result.ServerIpAddress.Ipv4Address);
        Assert.AreEqual(_expectedServerAddress.Ipv6Address, result.ServerIpAddress.Ipv6Address);
        Assert.AreEqual(entityToTest.ClientCountryIsoCode, result.ClientCountryIsoCode);
    }
}