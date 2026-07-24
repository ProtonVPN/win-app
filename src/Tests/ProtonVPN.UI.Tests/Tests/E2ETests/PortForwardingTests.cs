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

using System;
using System.Text;
using System.Collections.Generic;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.Enums.Locations;
using ProtonVPN.UI.Tests.TestsHelper.UiFlows;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("3")]
[Category("ARM")]
public class PortForwardingTests : FreshSessionSetUp
{
    private const Country COUNTRY_NAME = Country.Austria;

    private static readonly string _enableModerateNatTitle = LanguageHelper.GetTranslatedString("Settings_Connection_Advanced_NatType_Conflict_Title");
    private static readonly string _enableModerateNatDescription = LanguageHelper.GetTranslatedString("Settings_Connection_Advanced_NatType_Conflict_Description");

    private static readonly string _enablePortForwardingTitle = LanguageHelper.GetTranslatedString("Settings_Connection_PortForwarding_Conflict_Title");
    private static readonly string _enablePortForwardingDescription = LanguageHelper.GetTranslatedString("Settings_Connection_PortForwarding_Conflict_Description");

    private static readonly string _enableButton = LanguageHelper.GetTranslatedString("Common_Actions_Enable");

    private const string MODERATE_NAT_ENABLED_LINE_TO_LOOK_FOR = "\"randomized-nat\": false, \"port-forwarding\": false";
    private const string MODERATE_NAT_DISABLED_LINE_TO_LOOK_FOR = "\"randomized-nat\": true, \"port-forwarding\": true";

    private static readonly string _serviceLogsPath = TestEnvironment.GetServiceLogsPath();

    private static readonly List<string> _serversWithoutP2PSupport = ["FI#3", "MD#48"];

    [SetUp]
    public void SetUp()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test]
    [Property("TestCaseId", "602439")]
    [Retry(3)]
    public void PortForwardingOpensThePort()
    {
        TorrentHelper.AllowTorrentFirewall();
        TorrentHelper.StopAndCleanup();

        EnablePortForwardingAndConnect();

        string? ipAddressConnected = HomeRobot.GetVpnServerIp();
        Assert.That(ipAddressConnected, Is.Not.Null);

        FeaturesRobot.ClickCopyPortNumberFromActivePortSection();
        int forwardedPort = GetForwardedPortFromClipboard();

        try
        {
            TorrentHelper.StartTorrentOnPort(forwardedPort);
            Window?.Focus();
            TorrentHelper.IsPortOpen(ipAddressConnected!, forwardedPort);
        }
        finally
        {
            TorrentHelper.StopAndCleanup();
            CommonUiFlows.EnsureUserIsDisconnected();
        }
    }

    [Test]
    [Property("TestCaseId", "602440")]
    public void PortForwardingIsDisabledWhenModerateNatIsEnabled()
    {
        SettingRobot
            .OpenSettings()
            .OpenPortForwardingSettings()
            .EnablePortForwarding()
            .ApplySettings()
            .OpenAdvancedSettings();
        AdvancedSettingsRobot
            .SelectNatType(NatType.Moderate);

        ConfirmationRobot
            .Verify.IsOverlayDisplayed()
                   .OverlayTextContains(_enableModerateNatTitle)
                   .OverlayTextContains(_enableModerateNatDescription)
                   .OverlayButtonsEquals(primary: _enableButton)
            .PrimaryAction()
            .Verify.IsOverlayClosed();

        SettingRobot
            .ApplySettings()
            .Verify.IsPortForwardingDisabledStateDisplayed()
            .CloseSettings();

        ConnectAndVerify();

        WindowsUtils.AssertLogFile(_serviceLogsPath, MODERATE_NAT_ENABLED_LINE_TO_LOOK_FOR);

        CommonUiFlows.EnsureUserIsDisconnected();
    }

    [Test]
    [Property("TestCaseId", "786553")]
    public void ModerateNatIsDisabledWhenPortForwardingIsEnabled()
    {
        SettingRobot
            .OpenSettings()
            .OpenAdvancedSettings();
        AdvancedSettingsRobot
            .SelectNatType(NatType.Moderate);
        SettingRobot
            .ApplySettings()
            .OpenPortForwardingSettings()
            .EnablePortForwarding();

        ConfirmationRobot
            .Verify.IsOverlayDisplayed()
                   .OverlayTextContains(_enablePortForwardingTitle)
                   .OverlayTextContains(_enablePortForwardingDescription)
                   .OverlayButtonsEquals(primary: _enableButton)
            .PrimaryAction()
            .Verify.IsOverlayClosed();

        SettingRobot
            .ApplySettings()
            .Verify.IsPortForwardingEnabledStateDisplayed();

        ConnectAndVerify();

        WindowsUtils.AssertLogFile(_serviceLogsPath, MODERATE_NAT_DISABLED_LINE_TO_LOOK_FOR);

        CommonUiFlows.EnsureUserIsDisconnected();
    }

    [Test]
    [Property("TestCaseId", "602441")]
    [Category("SMOKE_3")]
    [Retry(3)]
    public void VerifyP2PServerGeneratesPortNumber()
    {
        DesktopRobot.DismissOldToastsIfVisible();

        SettingRobot
            .OpenSettings()
            .Verify.AreNotificationsEnabled()
            .CloseSettings();

        EnablePortForwardingAndConnect();

        FeaturesRobot.ClickCopyPortNumberFromActivePortSection();
        int widgetMenuPort = GetForwardedPortFromClipboard();

        VerifyPortInToast(widgetMenuPort);

        CopyPortFromFlyoutHover();
        int flyoutHoverPort = GetForwardedPortFromClipboard();

        CopyPortFromSettings();
        int settingsPort = GetForwardedPortFromClipboard();

        Assert.That(widgetMenuPort == flyoutHoverPort && flyoutHoverPort == settingsPort,
            $"Port in Flyout menu ({flyoutHoverPort}) does not match port in UI ({widgetMenuPort}) or settings ({settingsPort}).");

        VerifyPortUnavailableForServerWithoutP2PSupport();

        CommonUiFlows.EnsureUserIsDisconnected();
    }

    private static void CopyPortFromFlyoutHover()
    {
        FeaturesRobot
            .HoverOverPortForwardingWidget()
            .Verify.IsLastChangedTimerDisplayed()
            .ClickCopyPortNumberFromFlyoutMenu();
    }

    private static void CopyPortFromSettings()
    {
        SettingRobot
            .OpenSettings()
            .OpenPortForwardingSettings()
            .ClickCopyPortNumber();
    }

    private static void VerifyPortUnavailableForServerWithoutP2PSupport()
    {
        ConnectToServerWithoutP2PSupport();

        DesktopRobot
            .Verify.IsToastNotDisplayed();

        FeaturesRobot
            .HoverOverPortForwardingWidget()
            .Verify.IsPortUnavailable();
    }

    private static void ConnectToServerWithoutP2PSupport()
    {
        StringBuilder failureMessages = new();

        foreach (string server in _serversWithoutP2PSupport)
        {
            try
            {
                SidebarRobot
                    .SearchFor(server)
                    .ConnectToServer(server);

                HomeRobot
                    .Verify.ConnectionCardDescriptionContains(server);

                return;
            }
            catch (Exception e)
            {
                failureMessages.AppendLine($"Failed to connect to {server}: {e.Message}");
            }
        }

        Assert.Fail(failureMessages.ToString());
    }

    private static void VerifyPortInToast(int port)
    {
        DesktopRobot
            .Verify.IsToastDisplayed()
            .DoesToastPortMatchUI(port)
            .DoesToastCopyPortMatchUI(port);
    }

    private static void EnablePortForwardingAndConnect()
    {
        SettingRobot
            .OpenSettings()
            .OpenPortForwardingSettings()
            .EnablePortForwarding()
            .ApplySettings()
            .CloseSettings();

        ConnectAndVerify();
    }

    private static void ConnectAndVerify()
    {
        SidebarRobot
            .NavigateToP2PCountriesTab()
            .ConnectToCountry(COUNTRY_NAME);
        HomeRobot
            .Verify.IsConnected();
    }

    private static int GetForwardedPortFromClipboard()
    {
        string clipboardPortText = DesktopRobot.ReadClipboardText();

        if (!int.TryParse(clipboardPortText, out int port))
        {
            Assert.Fail($"Invalid port number copied: '{clipboardPortText}'");
        }

        return port;
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        DesktopRobot.Dispose();
    }
}