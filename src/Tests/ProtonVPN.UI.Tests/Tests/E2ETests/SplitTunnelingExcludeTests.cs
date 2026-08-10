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
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.Enums.Locations;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.TestsHelper.UiFlows;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("3")]
[Category("ARM")]
[Category("SMOKE_3")]
public class SplitTunnelingExcludeTests : BaseTest
{
    private const Country COUNTRY_NAME = Country.Austria;

    private const Browser APP_TO_EXCLUDE = Browser.GoogleChrome;
    private const Browser OTHER_APP = Browser.Edge;

    private string? _ipAddressNotConnected = null;

    private const string INVALID_IP = "192.A.B.1";
    private const string IPV6_ADDRESS = "2001:db8:3333:4444:5555:6666:7777:8888";
    private const string IP_ADDRESS_TO_EXCLUDE = "208.95.112.1";

    private static readonly string _invalidIpError = LanguageHelper.GetTranslatedString("Settings_Common_IpAddresses_Invalid");

    private static readonly string[] _specialIPs = { "127.0.0.1", "192.168.0.1", "0.0.0.0", "255.255.255.255", "10.0.0.1", "172.17.135.1" };

    [SetUp]
    public void SetUp()
    {
        WindowsUtils.RestoreChrome();
        LaunchClient();
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
        NetworkUtils.AssertInternetAvailability(true);
        _ipAddressNotConnected = NetworkUtils.GetIpAddressWithRetry();

        SettingRobot
            .OpenSettings()
            .OpenSplitTunnelingSettings();
        SplitTunnelingRobot
            .EnableSplitTunnelingToggle()
            .SelectExcludeMode();
    }

    [Test, Order(0)]
    [Property("TestCaseId", "602413")]
    public void SplitTunnelingIpInputDoesNotAllowInvalidIp()
    {
        SplitTunnelingRobot
            .EditSplitTunnelingIps();

        IpSelectorRobot
            .Verify.IsIpSelectorOpened()
            .AddIpAddress(INVALID_IP)
            .Verify.WasIpNotAdded(INVALID_IP)
                   .IsErrorMessageDisplayed(_invalidIpError)
            .ClearIpInput();

        ConfirmationRobot
            .CancelAction()
            .Verify.IsOverlayClosed();
    }

    [Test, Order(1)]
    [Property("TestCaseId", "788411")]
    public void SplitTunnelingIpInputAllowsIpV6()
    {
        SplitTunnelingRobot
            .EditSplitTunnelingIps();
        IpSelectorRobot
            .AddIpAddress(IPV6_ADDRESS)
            .Verify.WasIpAdded(IPV6_ADDRESS);
        ConfirmationRobot
           .PrimaryAction()
           .Verify.IsOverlayClosed();
    }

    [Test, Order(2)]
    [Property("TestCaseId", "602414")]
    [Retry(3)]
    public void SplitTunnelingExcludeIpAddress()
    {
        SplitTunnelingRobot
            .EditSplitTunnelingIps();
        IpSelectorRobot
            .AddIpAddress(IP_ADDRESS_TO_EXCLUDE)
            .Verify.WasIpAdded(IP_ADDRESS_TO_EXCLUDE);
        ConfirmationRobot
            .PrimaryAction()
            .Verify.IsOverlayClosed();

        SettingRobot
            .ApplySettings()
            .CloseSettings();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        NetworkUtils.VerifyIpAddressMatchesWithRetry(_ipAddressNotConnected);
    }

    [Test, Order(3)]
    [Property("TestCaseId", "787617")]
    [Ignore("JIRA - VPNWIN-1563")]
    public void SplitTunnelingExcludeModeSpecialIP()
    {
        SplitTunnelingRobot
            .EditSplitTunnelingIps();
        foreach (string specialIP in _specialIPs)
        {
            IpSelectorRobot
                .AddIpAddress(specialIP);
        }
        ConfirmationRobot
            .PrimaryAction()
            .Verify.IsOverlayClosed();

        SettingRobot
            .ApplySettings()
            .CloseSettings();

        HomeRobot
            .ConnectViaConnectionCard()
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
        NetworkUtils.VerifyLocalNetworking(isLanEnabled: true);

        //if location change works
        SidebarRobot
           .NavigateToAllCountriesTab()
           .ConnectToCountry(COUNTRY_NAME);
        HomeRobot
            .Verify.IsConnected();

        //if internet is still good after changing location
        HomeRobot
            .Verify.AssertVpnConnectionEstablished(_ipAddressNotConnected!, ipAddressConnected)
                   .AssertVPNIpAndExternalIpMatch(vpnServerIp, ipAddressConnected);
    }

    [Test, Order(4)]
    [Property("TestCaseId", "602417")]
    public void SplitTunnelingDeleteIpAddress()
    {
        SplitTunnelingRobot
            .EditSplitTunnelingIps();
        IpSelectorRobot
            .AddIpAddress(IP_ADDRESS_TO_EXCLUDE);
        ConfirmationRobot
            .PrimaryAction()
            .Verify.IsOverlayClosed();

        SettingRobot
            .ApplySettings()
            .CloseSettings();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        SettingRobot
            .OpenSettings()
            .OpenSplitTunnelingSettings();
        
        SplitTunnelingRobot
            .EditSplitTunnelingIps();
        IpSelectorRobot
            .Verify.IsIpSelectorOpened()
            .RemoveAllIps();
        ConfirmationRobot
            .PrimaryAction()
            .Verify.IsOverlayClosed();

        SettingRobot
            .Reconnect();

        HomeRobot
            .Verify.IsConnected();

        NetworkUtils.VerifyIpAddressDoesNotMatchWithRetry(_ipAddressNotConnected);
    }

    [Test, Order(5)]
    [Property("TestCaseId", "787609")]
    [Retry(3)]
    public void SplitTunnelingExcludeModeApp()
    {
        try
        {
            SplitTunnelingRobot
                .EditSplitTunnelingApps();
            AppSelectorRobot
                .Verify.AssertAppAvailability(APP_TO_EXCLUDE, shouldBeAvailable: true)
                .AddSuggestedApp(APP_TO_EXCLUDE)
                .Verify.IsAppChecked(APP_TO_EXCLUDE);
            ConfirmationRobot
                .PrimaryAction()
                .Verify.IsOverlayClosed();

            SettingRobot
                .ApplySettings()
                .CloseSettings();

            HomeRobot
                .ConnectViaConnectionCard()
                .Verify.IsConnected();

            string? ipAddressToCompare = HomeRobot.GetVpnServerIp();

            BrowserUtils.KillAllBrowsers();
            BrowserUtils.VerifyBrowserIpWithRetry(OTHER_APP, hasVpn: true, ipAddressToCompare);
            BrowserUtils.VerifyBrowserIpWithRetry(APP_TO_EXCLUDE, hasVpn: false, ipAddressToCompare);

            HomeRobot
                .Disconnect()
                .Verify.IsDisconnected();

            BrowserUtils.KillAllBrowsers();
            BrowserUtils.VerifyBrowserIpWithRetry(OTHER_APP, hasVpn: false, ipAddressToCompare);
            BrowserUtils.VerifyBrowserIpWithRetry(APP_TO_EXCLUDE, hasVpn: false, ipAddressToCompare);
        }
        finally
        {
            SettingRobot
                .OpenSettings()
                .OpenSplitTunnelingSettings();
            SplitTunnelingRobot
                .DisableSplitTunnelingToggle();
            SettingRobot
                .ApplySettings();
        }
    }

    [TearDown]
    public void TearDown()
    {
        Keyboard.Press(VirtualKeyShort.ESCAPE);
        try
        {
            ConfirmationRobot
                .Verify.IsOverlayDisplayed()
                .CancelAction();
        }
        catch { }
        BrowserUtils.KillAllBrowsers();
        Cleanup();
    }
}