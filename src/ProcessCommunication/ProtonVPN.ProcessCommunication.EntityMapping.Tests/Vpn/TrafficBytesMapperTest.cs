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
using ProtonVPN.Common.Vpn;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;
using ProtonVPN.ProcessCommunication.EntityMapping.Vpn;

namespace ProtonVPN.ProcessCommunication.EntityMapping.Tests.Vpn;

[TestClass]
public class TrafficBytesMapperTest
{
    private TrafficBytesMapper _mapper;

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
    public void TestMapLeftToRight_WhenZero()
    {
        InOutBytes entityToTest = InOutBytes.Zero;

        TrafficBytesIpcEntity result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(0u, result.BytesIn);
        Assert.AreEqual(0u, result.BytesOut);
    }

    [TestMethod]
    public void TestMapLeftToRight()
    {
        InOutBytes entityToTest = new((ulong)DateTime.UtcNow.Ticks, (ulong)DateTime.UtcNow.Millisecond);

        TrafficBytesIpcEntity result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(entityToTest.BytesIn, result.BytesIn);
        Assert.AreEqual(entityToTest.BytesOut, result.BytesOut);
    }

    [TestMethod]
    public void TestMapRightToLeft_WhenNull()
    {
        TrafficBytesIpcEntity entityToTest = null;

        InOutBytes result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(0u, result.BytesIn);
        Assert.AreEqual(0u, result.BytesOut);
    }

    [TestMethod]
    public void TestMapRightToLeft()
    {
        TrafficBytesIpcEntity entityToTest = new()
        {
            BytesIn = (ulong)DateTime.UtcNow.Ticks,
            BytesOut = (ulong)DateTime.UtcNow.Millisecond,
        };

        InOutBytes result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(entityToTest.BytesIn, result.BytesIn);
        Assert.AreEqual(entityToTest.BytesOut, result.BytesOut);
    }
}