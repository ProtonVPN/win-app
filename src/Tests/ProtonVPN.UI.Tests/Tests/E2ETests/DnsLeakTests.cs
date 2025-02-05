/*
 * Copyright (c) 2024 Proton AG
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
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("2")]
[Category("ARM")]
public class DnsLeakTests : FreshSessionSetUp
{
    private const string COUNTRY_NAME = "Australia";
    private const string COUNTRY_CODE = "AU";

    [SetUp]
    public void TestInitialize()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test]
    public void DnsIsNotLeaking()
    {
        List<string> dnsListNotConnected = DnsLeakHelper.GetDnsServers();
        
        SidebarRobot
            .SearchFor(COUNTRY_NAME)
            .ConnectToCountry(COUNTRY_CODE);

        HomeRobot
            .Verify.IsConnected();

        NavigationRobot
            .Verify.IsOnConnectionDetailsPage();

        DnsLeakHelper.VerifyIsNotLeaking(dnsListNotConnected);
    }
}
