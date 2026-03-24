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
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("3")]
[Category("ARM")]
public class SplitTunnelingExcludeTests : BaseTest
{
    private const string COUNTRY_NAME = "Austria";

    private string? _ipAddressNotConnected = null;
    private const string INVALID_IP_ERROR = "Enter a valid IPv4 or IPv6 address";

    private static readonly string[] _specialIPs = { "127.0.0.1", "192.168.0.1", "0.0.0.0", "255.255.255.255", "10.0.0.1", "172.17.135.1" };
    private const string INVALID_IP = "192.A.B.1";
    private const string IPV6_ADDRESS = "2001:db8:3333:4444:5555:6666:7777:8888";
    private const string IP_ADDRESS_TO_EXCLUDE = "208.95.112.1";

    private const string APP_TO_EXCLUDE = "Google Chrome";
    private const string OTHER_APP = "Edge";

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
            .SelectExcludeMode()
            .EditSplitTunnelingIps();

        IpSelectorRobot
            .Verify.IsIpSelectorOpened()
            .AddIpAddress(INVALID_IP)
            .Verify.WasIpNotAdded(INVALID_IP)
                   .IsErrorMessageDisplayed(INVALID_IP_ERROR)
            .ClearIpInput();
    }

    [Test, Order(1)]
    public void SplitTunnelingIpInputAllowsIpV6()
    {
        IpSelectorRobot
            .AddIpAddress(IPV6_ADDRESS)
            .Verify.WasIpAdded(IPV6_ADDRESS);
    }

    [Test, Order(2)]
    public void SplitTunnelingExcludeIpAddress()
    {
        IpSelectorRobot
            .AddIpAddress(IP_ADDRESS_TO_EXCLUDE)
            .Verify.WasIpAdded(IP_ADDRESS_TO_EXCLUDE);
        ConfirmationRobot
            .PrimaryAction();

        SettingRobot
            .ApplySettings()
            .CloseSettings();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        NetworkUtils.VerifyIpAddressMatchesWithRetry(_ipAddressNotConnected);
    }

    [Test, Order(3)]
    [Ignore("JIRA - VPNWIN-1563")]
    public void SplitTunnelingExcludeModeSpecialIP()
    {
        Assert.Fail("JIRA - VPNWIN-1563");

        SettingRobot
            .OpenSettings()
            .OpenSplitTunnelingSettings();

        SplitTunnelingRobot
            .EditSplitTunnelingIps();
        foreach (string specialIP in _specialIPs)
        {
            IpSelectorRobot
                .AddIpAddress(specialIP);
        }
        ConfirmationRobot
            .PrimaryAction();

        SettingRobot
            .Reconnect();

        HomeRobot
            .Verify.IsConnected();

        string ipAddressConnected = NetworkUtils.GetIpAddressWithRetry();
        string vpnServerIp = HomeRobot.GetVpnServerIp()!;

        //Verifying:
        //if internet is good and if public ip has changed
        HomeRobot
            .Verify.AssertVpnConnectionEstablished(_ipAddressNotConnected!, ipAddressConnected)
                   .AssertVPNIpAndExternalIpMatch(vpnServerIp, ipAddressConnected);

        //if after 60sec user is still connected
        Thread.Sleep(60_000);
        HomeRobot
            .Verify.IsConnected();

        //if LAN works
        NetworkUtils.VerifyIfLocalNetworkingWorks();

        //if location change works
        SidebarRobot
           .NavigateToAllCountriesTab()
           .ConnectToCountry(CountryCodes.GetCode(COUNTRY_NAME));
        HomeRobot
            .Verify.IsConnected();

        //if internet is still good after changing location
        HomeRobot
            .Verify.AssertVpnConnectionEstablished(_ipAddressNotConnected!, ipAddressConnected)
                   .AssertVPNIpAndExternalIpMatch(vpnServerIp, ipAddressConnected);
    }

    [Test, Order(4)]
    public void SplitTunnelingDeleteIpAddress()
    {
        SettingRobot
            .OpenSettings()
            .OpenSplitTunnelingSettings();

        SplitTunnelingRobot
            .EditSplitTunnelingIps();
        IpSelectorRobot
            .Verify.IsIpSelectorOpened()
            .DeleteAllIps();
        ConfirmationRobot
            .PrimaryAction();

        SettingRobot
            .Reconnect();

        HomeRobot
            .Verify.IsConnected();

        NetworkUtils.VerifyIpAddressDoesNotMatchWithRetry(_ipAddressNotConnected);
    }

    [Test, Order(5)]
    public void SplitTunnelingExcludeModeApp()
    {
        SettingRobot
            .OpenSettings()
            .OpenSplitTunnelingSettings();

        SplitTunnelingRobot
            .EditSplitTunnelingApps();
        AppSelectorRobot
            .Verify.AssertAppAvailability(APP_TO_EXCLUDE, true)
            .AddSuggestedApp(APP_TO_EXCLUDE)
            .Verify.IsAppChecked(APP_TO_EXCLUDE);
        ConfirmationRobot
            .PrimaryAction();

        SettingRobot
            .Reconnect();

        HomeRobot
            .Verify.IsConnected();
        string? ipAddressToCompare = HomeRobot.GetVpnServerIp();

        BrowserUtils.VerifyBrowserIpWithRetry(OTHER_APP, true, ipAddressToCompare);
        BrowserUtils.VerifyBrowserIpWithRetry(APP_TO_EXCLUDE, false, ipAddressToCompare);
        BrowserUtils.KillAllBrowsers();

        HomeRobot
            .Disconnect()
            .Verify.IsDisconnected();

        BrowserUtils.VerifyBrowserIpWithRetry(OTHER_APP, false, ipAddressToCompare);
        BrowserUtils.VerifyBrowserIpWithRetry(APP_TO_EXCLUDE, false, ipAddressToCompare);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        BrowserUtils.KillAllBrowsers();
        Cleanup();
    }
}