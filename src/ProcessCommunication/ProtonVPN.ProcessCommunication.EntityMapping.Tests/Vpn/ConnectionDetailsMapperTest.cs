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
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;
using ProtonVPN.ProcessCommunication.EntityMapping.Vpn;

namespace ProtonVPN.ProcessCommunication.EntityMapping.Tests.Vpn;

[TestClass]
public class ConnectionDetailsMapperTest
{
    private ConnectionDetailsMapper _mapper;

    [TestInitialize]
    public void Initialize()
    {
        _mapper = new();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _mapper = null;
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
            ServerIpAddress = $"B {DateTime.UtcNow}",
            ClientCountryIsoCode = $"C {DateTime.UtcNow}",
        };

        ConnectionDetailsIpcEntity result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(entityToTest.ClientIpAddress, result.ClientIpAddress);
        Assert.AreEqual(entityToTest.ServerIpAddress, result.ServerIpAddress);
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
            ServerIpAddress = $"B {DateTime.UtcNow}",
            ClientCountryIsoCode = $"C {DateTime.UtcNow}",
        };

        ConnectionDetails result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(entityToTest.ClientIpAddress, result.ClientIpAddress);
        Assert.AreEqual(entityToTest.ServerIpAddress, result.ServerIpAddress);
        Assert.AreEqual(entityToTest.ClientCountryIsoCode, result.ClientCountryIsoCode);
    }
}