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
using ProtonVPN.Common.Core.Helpers;

namespace ProtonVPN.Common.Core.Tests.Helpers;

[TestClass]
public class ResettableLazyTest
{
    [TestMethod]
    public void Test_Value()
    {
        ResettableLazy<DateTime> resettableLazy = new(() => DateTime.UtcNow);

        DateTime value1 = resettableLazy.Value;
        DateTime value2 = resettableLazy.Value;

        Assert.AreEqual(value1, value2);
    }

    [TestMethod]
    public void Test_Reset()
    {
        ResettableLazy<DateTime> resettableLazy = new(() => DateTime.UtcNow);

        DateTime value1 = resettableLazy.Value;
        resettableLazy.Reset();
        DateTime value2 = resettableLazy.Value;

        Assert.AreNotEqual(value1, value2);
    }

    [TestMethod]
    public void Test_LongRun()
    {
        ResettableLazy<DateTime> resettableLazy = new(() => DateTime.UtcNow);

        DateTime value1 = resettableLazy.Value;
        DateTime value2 = resettableLazy.Value;

        Assert.AreEqual(value1, value2);

        resettableLazy.Reset();
        DateTime value3 = resettableLazy.Value;

        Assert.AreNotEqual(value2, value3);

        DateTime value4 = resettableLazy.Value;

        Assert.AreEqual(value3, value4);
    }
}