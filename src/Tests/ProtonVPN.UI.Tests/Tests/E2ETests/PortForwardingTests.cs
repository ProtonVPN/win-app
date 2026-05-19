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
    private const string SERVER_WITHOUT_P2P_SUPPORT = "FI#3";

    private const string ENABLE_MODERATE_NAT_TITLE = "Enable Moderate NAT?";
    private const string ENABLE_MODERATE_NAT_DESCRIPTION = "You won't be able to use port forwarding with Moderate NAT.";

    private const string ENABLE_PORT_FORWARDING_TITLE = "Enable port forwarding?";
    private const string ENABLE_PORT_FORWARDING_DESCRIPTION = "You won't be able to use Moderate NAT when port forwarding is enabled.";

    private const string ENABLE_BUTTON = "Enable";

    private const string MODERATE_NAT_ENABLED_LINE_TO_LOOK_FOR = "\"randomized-nat\": false, \"port-forwarding\": false";
    private const string MODERATE_NAT_DISABLED_LINE_TO_LOOK_FOR = "\"randomized-nat\": true, \"port-forwarding\": true";

    private static readonly string _serviceLogsPath = TestEnvironment.GetServiceLogsPath();

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
        TorrentHelper.AllowAriaFirewallScript();
        TorrentHelper.StopAndCleanup();

        EnablePortForwardingAndConnect();

        string? ipAddressConnected = HomeRobot.GetVpnServerIp();
        Assert.That(ipAddressConnected, Is.Not.Null);

        HomeRobot.ClickCopyPortNumber();
        int forwardedPort = GetForwardedPortFromClipboard();

        try
        {
            TorrentHelper.StartTorrentOnPort(forwardedPort);
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
            .OpenAdvancedSettings()
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
            .OpenAdvancedSettings()
            .SelectNatType(NatType.Moderate)
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
    public void VerifyP2PServerGeneratesPortNumber()
    {
        SettingRobot
            .OpenSettings()
            .Verify.AreNotificationsEnabled()
            .CloseSettings();

        EnablePortForwardingAndConnect();

        HomeRobot.ClickCopyPortNumber();
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

    private void CopyPortFromFlyoutHover()
    {
        HomeRobot
            .HoverOverPortForwardingWidget()
            .Verify.IsLastChangedTimerDisplayed()
            .ClickHoverCopyPortNumber();
    }

    private void CopyPortFromSettings()
    {
        SettingRobot
            .OpenSettings()
            .OpenPortForwardingSettings()
            .ClickCopyPortNumber();
    }

    private void VerifyPortUnavailableForServerWithoutP2PSupport()
    {
        SidebarRobot
            .SearchFor(SERVER_WITHOUT_P2P_SUPPORT)
            .ConnectToServer(SERVER_WITHOUT_P2P_SUPPORT);

        HomeRobot
            .Verify.IsConnected();

        DesktopRobot
            .Verify.IsToastNotDisplayed();

        HomeRobot
            .HoverOverPortForwardingWidget()
            .Verify.IsPortUnavailable();
    }

    private void VerifyPortInToast(int port)
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