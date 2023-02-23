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
using ProtonVPN.Common.Configuration;
using ProtonVPN.Dns.Contracts;
using ProtonVPN.Dns.Resolvers;
using ProtonVPN.Dns.Tests.Mocks;

namespace ProtonVPN.Dns.Tests.Resolvers
{
    [TestClass]
    public class DnsOverHttpsTxtRecordsResolverTest
        : DnsOverHttpsResolverTestBase<DnsOverHttpsTxtRecordsResolver>
    {
        private const string HOST = "dMFYGSLTQOJXXI33OOZYG4LTDNA.protonpro.xyz";

        public DnsOverHttpsTxtRecordsResolverTest() : base(HOST)
        {
        }

        protected override DnsOverHttpsTxtRecordsResolver CreateResolver(IConfiguration configuration,
            MockOfLogger logger, MockOfHttpClientFactory mockOfHttpClientFactory,
            IDnsOverHttpsProvidersManager dnsOverHttpsProvidersManager)
        {
            return new DnsOverHttpsTxtRecordsResolver(configuration, logger,
                mockOfHttpClientFactory, dnsOverHttpsProvidersManager);
        }

        protected override void AssertCorrectResponse(DnsResponse response)
        {
            Assert.IsTrue(response.AlternativeHosts.Count > 0);
        }
    }
}