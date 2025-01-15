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

using NUnit.Framework;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("1")]
public class SplitTunnelingIncludeTests : BaseTest
{
    private string _ipAddressNotConnected = null;
    private const string IP_ADDRESS_1 = "104.26.12.205";
    private const string IP_ADDRESS_2 = "104.26.13.205";
    private const string IP_ADDRESS_3 = "172.67.74.152";

    [OneTimeSetUp]
    public void SetUp()
    {
        _ipAddressNotConnected = NetworkUtils.GetIpAddress();
        LaunchApp();
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test, Order(0)]
    public void SplitTunnelingIncludeModeIP()
    {
        SettingRobot
            .OpenSettings()
            .OpenSplitTunnelingSettings();
        SplitTunnelingRobot
            .ToggleSplitTunnelingSwitch()
            .SelectInverseMode()
            .AddIpAddress(IP_ADDRESS_1)
            .AddIpAddress(IP_ADDRESS_2)
            .AddIpAddress(IP_ADDRESS_3);

        SettingRobot.ApplySettings()
            .CloseSettings();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        NetworkUtils.VerifyIpAddressDoesNotMatchWithRetry(_ipAddressNotConnected);
    }

    [Test, Order(2)]
    public void SplitTunnelingIsDisabledByTickingCheckbox()
    {
        SettingRobot
            .OpenSettings()
            .OpenSplitTunnelingSettings();

        SplitTunnelingRobot
            .TickIpAddressCheckBox(IP_ADDRESS_1)
            .TickIpAddressCheckBox(IP_ADDRESS_2)
            .TickIpAddressCheckBox(IP_ADDRESS_3);

        SettingRobot.Reconnect();
        HomeRobot.Verify.IsConnected();

        NetworkUtils.VerifyIpAddressMatchesWithRetry(_ipAddressNotConnected);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        Cleanup();
    }
}
