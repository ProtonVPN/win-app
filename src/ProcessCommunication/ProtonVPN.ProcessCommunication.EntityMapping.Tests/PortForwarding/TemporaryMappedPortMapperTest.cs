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
using ProtonVPN.Common.Legacy.PortForwarding;
using ProtonVPN.ProcessCommunication.Contracts.Entities.PortForwarding;
using ProtonVPN.ProcessCommunication.EntityMapping.PortForwarding;

namespace ProtonVPN.ProcessCommunication.EntityMapping.Tests.PortForwarding;

[TestClass]
public class TemporaryMappedPortMapperTest
{
    private TemporaryMappedPortMapper _mapper;

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
        TemporaryMappedPort entityToTest = null;

        TemporaryMappedPortIpcEntity result = _mapper.Map(entityToTest);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void TestMapLeftToRight_WhenMappedPortIsNull()
    {
        TemporaryMappedPort entityToTest = new()
        {
            Lifetime = TimeSpan.FromMinutes(1),
            ExpirationDateUtc = DateTime.UtcNow
        };

        TemporaryMappedPortIpcEntity result = _mapper.Map(entityToTest);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void TestMapLeftToRight()
    {
        int internalPort = 7;
        int externalPort = 28;
        TemporaryMappedPort entityToTest = new()
        {
            MappedPort = new MappedPort(internalPort, externalPort),
            Lifetime = TimeSpan.FromMinutes(4),
            ExpirationDateUtc = DateTime.UtcNow
        };

        TemporaryMappedPortIpcEntity result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(internalPort, result.InternalPort);
        Assert.AreEqual(externalPort, result.ExternalPort);
        Assert.AreEqual(entityToTest.Lifetime, result.Lifetime);
        Assert.AreEqual(entityToTest.ExpirationDateUtc, result.ExpirationDateUtc);
    }

    [TestMethod]
    public void TestMapRightToLeft_WhenNull()
    {
        TemporaryMappedPortIpcEntity entityToTest = null;

        TemporaryMappedPort result = _mapper.Map(entityToTest);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void TestMapRightToLeft()
    {
        int internalPort = 7;
        int externalPort = 28;
        TemporaryMappedPortIpcEntity entityToTest = new()
        {
            InternalPort = internalPort,
            ExternalPort = externalPort,
            Lifetime = TimeSpan.FromMinutes(3),
            ExpirationDateUtc = DateTime.UtcNow
        };

        TemporaryMappedPort result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.MappedPort);
        Assert.AreEqual(entityToTest.InternalPort, result.MappedPort.InternalPort);
        Assert.AreEqual(entityToTest.ExternalPort, result.MappedPort.ExternalPort);
        Assert.AreEqual(entityToTest.Lifetime, result.Lifetime);
        Assert.AreEqual(entityToTest.ExpirationDateUtc, result.ExpirationDateUtc);
    }
}