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
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("3")]
[Category("ARM")]
public class PortForwardingTests : FreshSessionSetUp
{
    private const string COUNTRY_NAME = "Austria";

    private const string ENABLE_MODERATE_NAT_TITLE = "Enable Moderate NAT?";
    private const string ENABLE_MODERATE_NAT_DESCRIPTION = "You won't be able to use port forwarding with Moderate NAT.";

    private const string ENABLE_PORT_FORWARDING_TITLE = "Enable port forwarding?";
    private const string ENABLE_PORT_FORWARDING_DESCRIPTION = "You won't be able to use Moderate NAT when port forwarding is enabled.";

    private const string ENABLE_BUTTON = "Enable";

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
                   .OverlayTextContains(ENABLE_MODERATE_NAT_TITLE)
                   .OverlayTextContains(ENABLE_MODERATE_NAT_DESCRIPTION)
                   .OverlayButtonsEquals(primary: ENABLE_BUTTON)
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
                   .OverlayTextContains(ENABLE_PORT_FORWARDING_TITLE)
                   .OverlayTextContains(ENABLE_PORT_FORWARDING_DESCRIPTION)
                   .OverlayButtonsEquals(primary: ENABLE_BUTTON)
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
    [Retry(3)]
    [Property("TestCaseId", "602441")]
    [Category("SMOKE_3")]
    public void VerifyP2PServerGeneratesPortNumber()
    {
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
        string portText = string.Empty;
        Thread staThread = new(() =>
        {
            portText = Clipboard.GetText().Trim();
        });
        staThread.SetApartmentState(ApartmentState.STA);
        staThread.Start();
        staThread.Join();

        if (!int.TryParse(portText, out int port))
        {
            Assert.Fail($"Invalid port number copied: '{portText}'");
        }

        return port;
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        DesktopRobot.Dispose();
    }
}