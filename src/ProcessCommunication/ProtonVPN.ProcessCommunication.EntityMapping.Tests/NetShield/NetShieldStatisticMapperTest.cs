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
using ProtonVPN.Common.Legacy.NetShield;
using ProtonVPN.ProcessCommunication.Contracts.Entities.NetShield;
using ProtonVPN.ProcessCommunication.EntityMapping.NetShield;

namespace ProtonVPN.ProcessCommunication.EntityMapping.Tests.NetShield;

[TestClass]
public class NetShieldStatisticMapperTest
{
    private NetShieldStatisticMapper _mapper;

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
        NetShieldStatistic entityToTest = null;

        NetShieldStatisticIpcEntity result = _mapper.Map(entityToTest);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void TestMapLeftToRight()
    {
        NetShieldStatistic entityToTest = new()
        {
            NumOfMaliciousUrlsBlocked = DateTime.UtcNow.Millisecond,
            NumOfAdvertisementUrlsBlocked = DateTime.UtcNow.Ticks,
            NumOfTrackingUrlsBlocked = DateTime.UtcNow.Year
        };
        Assert.IsNotNull(entityToTest.TimestampUtc);

        NetShieldStatisticIpcEntity result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(entityToTest.NumOfMaliciousUrlsBlocked, result.NumOfMaliciousUrlsBlocked);
        Assert.AreEqual(entityToTest.NumOfAdvertisementUrlsBlocked, result.NumOfAdvertisementUrlsBlocked);
        Assert.AreEqual(entityToTest.NumOfTrackingUrlsBlocked, result.NumOfTrackingUrlsBlocked);
        Assert.IsNotNull(result.TimestampUtc);
        Assert.AreEqual(entityToTest.TimestampUtc, result.TimestampUtc);
    }

    [TestMethod]
    public void TestMapRightToLeft_WhenNull()
    {
        NetShieldStatisticIpcEntity entityToTest = null;

        NetShieldStatistic result = _mapper.Map(entityToTest);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void TestMapRightToLeft()
    {
        NetShieldStatisticIpcEntity entityToTest = new()
        {
            NumOfMaliciousUrlsBlocked = DateTime.UtcNow.Millisecond,
            NumOfAdvertisementUrlsBlocked = DateTime.UtcNow.Ticks,
            NumOfTrackingUrlsBlocked = DateTime.UtcNow.Year,
            TimestampUtc = DateTime.UtcNow,
        };

        NetShieldStatistic result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(entityToTest.NumOfMaliciousUrlsBlocked, result.NumOfMaliciousUrlsBlocked);
        Assert.AreEqual(entityToTest.NumOfAdvertisementUrlsBlocked, result.NumOfAdvertisementUrlsBlocked);
        Assert.AreEqual(entityToTest.NumOfTrackingUrlsBlocked, result.NumOfTrackingUrlsBlocked);
        Assert.IsNotNull(result.TimestampUtc);
        Assert.AreEqual(entityToTest.TimestampUtc, result.TimestampUtc);
    }
}