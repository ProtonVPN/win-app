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
using ProtonVPN.Crypto.Contracts;

namespace ProtonVPN.Crypto.Tests;

[TestClass]
public class Sha1CalculatorTest
{
    private ISha1Calculator? _sha1Calculator;

    [TestInitialize]
    public void Initialize()
    {
        _sha1Calculator = new Sha1Calculator();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _sha1Calculator = null;
    }

    [DataTestMethod]
    [DataRow("da39a3ee5e6b4b0d3255bfef95601890afd80709", "")]
    [DataRow("1ef0c5a368ea1e692f92214a6d4fb2b87e2626e8", "testas1")]
    [DataRow("089249220d3ec96e9457427b03f03f465a9e9955", "testas2")]
    [DataRow("16e5ef380cf2bc2f081344f1b3d5aada387a3a3c", "testas3")]
    [DataRow("aeda6f734aff5793c3c61cf57841453423fdc2c0", "example.test.account.123456789@account.protonvpn.com")]
    public void TestHash(string expectedResult, string input)
    {
        string result = _sha1Calculator!.Hash(input);

        Assert.AreEqual(expectedResult, result);
    }
}