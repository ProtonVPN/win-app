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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Common.Extensions;

namespace ProtonVPN.Common.Tests.Extensions
{
    [TestClass]
    public class ByteExtensionsTest
    {
        [DataTestMethod]
        [DataRow(null, null)]
        [DataRow(new byte[]{}, new byte[]{})]
        [DataRow(new byte[]{0}, new byte[]{})]
        [DataRow(new byte[]{1}, new byte[]{1})]
        [DataRow(new byte[]{0,0}, new byte[]{})]
        [DataRow(new byte[]{2,0}, new byte[]{2})]
        [DataRow(new byte[]{0,3}, new byte[]{0,3})]
        [DataRow(new byte[]{4,5}, new byte[]{4,5})]
        [DataRow(new byte[]{0,0,0}, new byte[]{})]
        [DataRow(new byte[]{6,0,0}, new byte[]{6})]
        [DataRow(new byte[]{0,7,0}, new byte[]{0,7})]
        [DataRow(new byte[]{8,9,1}, new byte[]{8,9,1})]
        [DataRow(new byte[]{0,0,0,0,0,0,0,0,0}, new byte[]{})]
        [DataRow(new byte[]{2,0,0,0,0,0,0,0,0}, new byte[]{2})]
        [DataRow(new byte[]{0,0,0,0,3,0,0,0,0}, new byte[]{0,0,0,0,3})]
        [DataRow(new byte[]{4,5,6,7,8,0,0,0,0}, new byte[]{4,5,6,7,8})]
        [DataRow(new byte[]{0,0,0,0,0,0,0,9,0}, new byte[]{0,0,0,0,0,0,0,9})]
        [DataRow(new byte[]{1,2,3,4,5,6,7,8,0}, new byte[]{1,2,3,4,5,6,7,8})]
        [DataRow(new byte[]{0,0,0,0,0,0,0,0,9}, new byte[]{0,0,0,0,0,0,0,0,9})]
        [DataRow(new byte[]{1,2,3,4,5,6,7,8,9}, new byte[]{1,2,3,4,5,6,7,8,9})]
        public void TestTrimTrailingZeroBytes(byte[] argument, byte[] expectedResult)
        {
            byte[] result = argument.TrimTrailingZeroBytes();
            result.Should().BeEquivalentTo(expectedResult);
        }
    }
}