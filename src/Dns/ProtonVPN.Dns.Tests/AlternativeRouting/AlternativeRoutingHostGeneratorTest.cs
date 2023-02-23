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
using NSubstitute;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Dns.AlternativeRouting;

namespace ProtonVPN.Dns.Tests.AlternativeRouting
{
    [TestClass]
    public class AlternativeRoutingHostGeneratorTest
    {
        private const string API_URL = "https://api.protonvpn.ch";
        private const string EXPECTED_BASE_HOST = "dMFYGSLTQOJXXI33OOZYG4LTDNA.protonpro.xyz";

        [TestMethod]
        [DataRow(null, EXPECTED_BASE_HOST)]
        [DataRow("", EXPECTED_BASE_HOST)]
        [DataRow("abc123", $"abc123.{EXPECTED_BASE_HOST}")]
        public void TestGenerate(string uid, string expectedResult)
        {
            IConfiguration configuration = Substitute.For<IConfiguration>();
            configuration.Urls.Returns(new UrlConfig()
            {
                ApiUrl = API_URL
            });
            AlternativeRoutingHostGenerator generator = new(configuration);

            string result = generator.Generate(uid);

            Assert.AreEqual(expectedResult, result);
        }
    }
}