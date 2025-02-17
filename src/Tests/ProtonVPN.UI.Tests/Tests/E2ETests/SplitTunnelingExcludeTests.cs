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
[Category("3")]
[Category("ARM")]
public class SplitTunnelingExcludeModeTests : BaseTest
{
    private string _ipAddressNotConnected = null;
    private const string INVALID_IP = "192.A.B.1";
    private const string IPV6_ADDRESS = "2001:db8:3333:4444:5555:6666:7777:8888";
    private const string IP_ADDRESS_TO_EXCLUDE = "208.95.112.1";

    [OneTimeSetUp]
    public void SetUp()
    {
        _ipAddressNotConnected = NetworkUtils.GetIpAddressWithRetry();
        LaunchApp();
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test, Order(0)]
    public void SplitTunnelingIpInputDoesNotAllowInvalidIp()
    {
        SettingRobot
            .OpenSettings()
            .OpenSplitTunnelingSettings();

        SplitTunnelingRobot
            .ToggleSplitTunnelingSwitch()
            .AddIpAddress(INVALID_IP)
            .Verify.WasIpNotAdded(INVALID_IP);

        SplitTunnelingRobot.ClearIpInput();
    }

    [Test, Order(1)]
    public void SplitTunnelingIpInputAllowsIpV6()
    {
        SplitTunnelingRobot
            .AddIpAddress(IPV6_ADDRESS)
            .Verify.WasIpAdded(IPV6_ADDRESS);
    }

    [Test, Order(2)]
    public void SplitTunnelingExcludeModeIP()
    {
        SplitTunnelingRobot
            .AddIpAddress(IP_ADDRESS_TO_EXCLUDE);

        SettingRobot
            .ApplySettings()
            .CloseSettings();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        NetworkUtils.VerifyIpAddressMatchesWithRetry(_ipAddressNotConnected);
    }

    [Test, Order(3)]
    public void SplitTunnelingDeleteIP()
    {
        SettingRobot
            .OpenSettings()
            .OpenSplitTunnelingSettings();

        SplitTunnelingRobot.DeleteAllIps();

        SettingRobot.Reconnect();

        NetworkUtils.VerifyIpAddressDoesNotMatchWithRetry(_ipAddressNotConnected);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        Cleanup();
    }
}
