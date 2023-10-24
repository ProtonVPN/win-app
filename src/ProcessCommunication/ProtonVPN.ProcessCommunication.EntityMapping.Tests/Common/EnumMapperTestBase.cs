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
using ProtonVPN.EntityMapping.Contracts;

namespace ProtonVPN.ProcessCommunication.EntityMapping.Tests.Common;

[TestClass]
public abstract class EnumMapperTestBase<TLeft, TRight>
    where TLeft : struct, Enum
    where TRight : struct, Enum
{
    private IMapper<TLeft, TRight> _mapper;

    [TestInitialize]
    public void Initialize()
    {
        _mapper = CreateMapper();
    }

    protected abstract IMapper<TLeft, TRight> CreateMapper();

    [TestCleanup]
    public void Cleanup()
    {
        _mapper = null;
    }

    [TestMethod]
    public void TestEnumsHaveEqualUnderlyingType()
    {
        Assert.AreEqual(
            Enum.GetUnderlyingType(typeof(TLeft)),
            Enum.GetUnderlyingType(typeof(TRight)));
    }

    [TestMethod]
    public void TestEnumsHaveEqualCount()
    {
        int count = Enum.GetValues<TLeft>().Count();
        Assert.AreEqual(count, Enum.GetValues<TRight>().Count());
        Assert.AreEqual(count, Enum.GetNames<TLeft>().Count());
        Assert.AreEqual(count, Enum.GetNames<TRight>().Count());
    }

    [TestMethod]
    public void TestEnumsHaveEqualValues()
    {
        List<TLeft> leftEntities = Enum.GetValues<TLeft>().ToList();
        List<TRight> rightEntities = Enum.GetValues<TRight>().ToList();

        for (int i = 0; i < leftEntities.Count; i++)
        {
            Assert.AreEqual(
                GetValueAsUnderlyingType(leftEntities[i]),
                GetValueAsUnderlyingType(rightEntities[i]));
            Assert.AreEqual(rightEntities[i], _mapper.Map(leftEntities[i]));
            Assert.AreEqual(leftEntities[i], _mapper.Map(rightEntities[i]));
        }
    }

    private object GetValueAsUnderlyingType<T>(T value)
        where T : struct, Enum
    {
        return Convert.ChangeType(value, value.GetTypeCode());
    }

    [TestMethod]
    public void TestEnumsHaveEqualNames()
    {
        List<string> leftEntitynames = Enum.GetNames<TLeft>().ToList();
        List<string> rightEntityNames = Enum.GetNames<TRight>().ToList();

        for (int i = 0; i < leftEntitynames.Count; i++)
        {
            Assert.AreEqual(leftEntitynames[i], rightEntityNames[i]);
        }
    }
}