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

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Common.Core.Extensions;

namespace ProtonVPN.Common.Core.Tests.Extensions;

[TestClass]
public class DateTimeOffsetExtensionsTest
{
    [TestMethod]
    public void TestMin_WhenCorrectArgumentIsLeft()
    {
        DateTimeOffset dateTimeOffsetLeft = DateTimeOffset.MinValue;
        DateTimeOffset dateTimeOffsetRight = DateTimeOffset.MaxValue;

        DateTimeOffset result = DateTimeOffsetExtensions.Min(dateTimeOffsetLeft, dateTimeOffsetRight);

        Assert.AreEqual(dateTimeOffsetLeft, result);
    }

    [TestMethod]
    public void TestMin_WhenCorrectArgumentIsRight()
    {
        DateTimeOffset dateTimeOffsetLeft = DateTimeOffset.MaxValue;
        DateTimeOffset dateTimeOffsetRight = DateTimeOffset.MinValue;

        DateTimeOffset result = DateTimeOffsetExtensions.Min(dateTimeOffsetLeft, dateTimeOffsetRight);

        Assert.AreEqual(dateTimeOffsetRight, result);
    }

    [TestMethod]
    public void TestMin_WhenBothAreCorrect()
    {
        DateTimeOffset dateTimeOffsetLeft = DateTimeOffset.UnixEpoch;
        DateTimeOffset dateTimeOffsetRight = DateTimeOffset.UnixEpoch;

        DateTimeOffset result = DateTimeOffsetExtensions.Min(dateTimeOffsetLeft, dateTimeOffsetRight);

        Assert.AreEqual(dateTimeOffsetLeft, result);
        Assert.AreEqual(dateTimeOffsetRight, result);
    }

    [TestMethod]
    public void TestMax_WhenCorrectArgumentIsLeft()
    {
        DateTimeOffset dateTimeOffsetLeft = DateTimeOffset.MaxValue;
        DateTimeOffset dateTimeOffsetRight = DateTimeOffset.MinValue;

        DateTimeOffset result = DateTimeOffsetExtensions.Max(dateTimeOffsetLeft, dateTimeOffsetRight);

        Assert.AreEqual(dateTimeOffsetLeft, result);
    }

    [TestMethod]
    public void TestMax_WhenCorrectArgumentIsRight()
    {
        DateTimeOffset dateTimeOffsetLeft = DateTimeOffset.MinValue;
        DateTimeOffset dateTimeOffsetRight = DateTimeOffset.MaxValue;

        DateTimeOffset result = DateTimeOffsetExtensions.Max(dateTimeOffsetLeft, dateTimeOffsetRight);

        Assert.AreEqual(dateTimeOffsetRight, result);
    }

    [TestMethod]
    public void TestMax_WhenBothAreCorrect()
    {
        DateTimeOffset dateTimeOffsetLeft = DateTimeOffset.UnixEpoch;
        DateTimeOffset dateTimeOffsetRight = DateTimeOffset.UnixEpoch;

        DateTimeOffset result = DateTimeOffsetExtensions.Max(dateTimeOffsetLeft, dateTimeOffsetRight);

        Assert.AreEqual(dateTimeOffsetLeft, result);
        Assert.AreEqual(dateTimeOffsetRight, result);
    }
}