/*
 * Copyright (c) 2026 Proton AG
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

using System.Threading;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("2")]
public class NatTypeTests : FreshSessionSetUp
{
    private const string STRICT_NAT_TYPE = "\"randomized-nat\": true";
    private const string MODERATE_NAT_TYPE = "\"randomized-nat\": false";
    private const string LINE_TO_LOOK_FOR = "{\"state\": \"connected\"";
    private static readonly string _serviceLogsPath = TestEnvironment.GetServiceLogsPath();

    [SetUp]
    public void TestInitialize()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test]
    public void NatTypeSetToModerate()
    {
        VerifyNatType(NatType.Moderate, MODERATE_NAT_TYPE);
    }

    [Test]
    public void NatTypeSetToStrict()
    {
        VerifyNatType(NatType.Strict, STRICT_NAT_TYPE);
    }

    private void VerifyNatType(NatType natType, string wordToLookFor)
    {
        SettingRobot
            .OpenSettings()
            .OpenAdvancedSettings()
            .SelectNatType(natType);

        if (natType == NatType.Moderate)
        {
            SettingRobot
                .ApplySettings();
        }

        SettingRobot
            .CloseSettings();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        //give it time to populate the service-logs after connecting
        Thread.Sleep(TestConstants.OneSecondTimeout);

        WindowsUtils.AssertLogFile(_serviceLogsPath, LINE_TO_LOOK_FOR, wordToLookFor);
    }
}