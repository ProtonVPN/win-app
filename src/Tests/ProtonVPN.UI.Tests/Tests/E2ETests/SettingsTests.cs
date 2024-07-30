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

using NUnit.Framework;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.Robots.Home;
using ProtonVPN.UI.Tests.Robots.Login;
using ProtonVPN.UI.Tests.Robots.Overlays;
using ProtonVPN.UI.Tests.Robots.Settings;
using ProtonVPN.UI.Tests.Robots.Shell;
using ProtonVPN.UI.Tests.TestsHelper;
using static ProtonVPN.UI.Tests.TestsHelper.TestConstants;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("2")]
public class SettingsTests : TestSession
{
    private const string SETTINGS_PAGE_TITLE = "Settings";
    private const string NETSHIELD_PAGE_TITLE = "NetShield";
    private const string KILL_SWITCH_PAGE_TITLE = "Kill switch";
    private const string PORT_FORWARDING_PAGE_TITLE = "Port forwarding";
    private const string SPLIT_TUNNELING_PAGE_TITLE = "Split tunneling";
    private const string PROTOCOL_PAGE_TITLE = "Protocol";
    private const string VPN_ACCELERATOR_PAGE_TITLE = "VPN Accelerator";
    private const string ADVANCED_SETTINGS_PAGE_TITLE = "Advanced settings";
    private const string AUTO_STARTUP_PAGE_TITLE = "Auto startup";
    private const string DEBUG_LOGS_PAGE_TITLE = "Debug logs";
    private const string CENSORSHIP_PAGE_TITLE = "Help us fight censorship";
    private const string CUSTOM_DNS_SERVERS_PAGE_TITLE = "Custom DNS servers";

    private const string SMART_PROTOCOL = "Smart";
    private const string WIREGUARD_UDP_PROTOCOL = "WireGuard (UDP)";
    private const string WIREGUARD_TLS_PROTOCOL = "Stealth";

    private ShellRobot _shellRobot = new();
    private HomeRobot _homeRobot = new();
    private SettingsRobot _settingsRobot = new();
    private LoginRobot _loginRobot = new();

    [SetUp]
    public void TestInitialize()
    {
        LaunchApp();

        _loginRobot
            .Wait(StartupDelay)
            .DoLogin(TestUserData.PlusUser);

        _homeRobot
            .DoCloseWelcomeOverlay()
            .DoWaitForVpnStatusSubtitleLabel()
            .VerifyVpnStatusIsDisconnected()
            .VerifyConnectionCardIsInInitalState();
    }

    [Test]
    public void ChangeProtocolSettings()
    {
        _shellRobot
            .DoNavigateToSettingsPage()
            .VerifyCurrentPage(SETTINGS_PAGE_TITLE, false);

        _settingsRobot
            .VerifyProtocolSettingsCard(SMART_PROTOCOL)
            .DoNavigateToProtocolSettingsPage()
            .VerifyProtocolSettingsPage();

        _shellRobot
            .VerifyCurrentPage(PROTOCOL_PAGE_TITLE, true);

        _settingsRobot
            .VerifySmartProtocolIsChecked()
            .DoSelectProtocol(Protocol.WireGuardUdp)
            .VerifyWireGuardUdpProtocolIsChecked()
            .DoApplyChanges();

        _shellRobot
            .DoNavigateBackward()
            .VerifyCurrentPage(SETTINGS_PAGE_TITLE, false);

        _settingsRobot
            .VerifyProtocolSettingsCard(WIREGUARD_UDP_PROTOCOL);

        _settingsRobot
            .DoNavigateToProtocolSettingsPage();

        _settingsRobot
            .VerifyWireGuardUdpProtocolIsChecked()
            .DoSelectProtocol(Protocol.WireGuardTls)
            .VerifyWireGuardTlsProtocolIsChecked()
            .DoApplyChanges();

        _shellRobot
            .DoNavigateBackward()
            .VerifyCurrentPage(SETTINGS_PAGE_TITLE, false);

        _settingsRobot
            .VerifyProtocolSettingsCard(WIREGUARD_TLS_PROTOCOL);
    }

    [Test]
    public void NavigateThroughSettingsPages()
    {
        _shellRobot
            .DoNavigateToSettingsPage()
            .VerifyCurrentPage(SETTINGS_PAGE_TITLE, false);

        _settingsRobot
            .DoNavigateToProtocolSettingsPage();

        _shellRobot
            .VerifyCurrentPage(PROTOCOL_PAGE_TITLE, true)
            .DoNavigateBackward();

        _settingsRobot
            .DoNavigateToVpnAcceleratorSettingsPage();

        _shellRobot
            .VerifyCurrentPage(VPN_ACCELERATOR_PAGE_TITLE, true)
            .DoNavigateBackward();

        _settingsRobot
            .DoNavigateToAdvancedSettingsPage();

        _shellRobot
            .VerifyCurrentPage(ADVANCED_SETTINGS_PAGE_TITLE, true)
            .DoNavigateBackward();

        _settingsRobot
            .DoNavigateToAutoStartupSettingsPage();

        _shellRobot
            .VerifyCurrentPage(AUTO_STARTUP_PAGE_TITLE, true)
            .DoNavigateBackward();

        _settingsRobot
            .DoNavigateToDebugLogsSettingsPage();

        _shellRobot
            .VerifyCurrentPage(DEBUG_LOGS_PAGE_TITLE, true)
            .DoNavigateBackward();

        _settingsRobot
            .DoNavigateToCensorshipSettingsPage();

        _shellRobot
            .VerifyCurrentPage(CENSORSHIP_PAGE_TITLE, true)
            .DoNavigateBackward();

        _settingsRobot
            .DoNavigateToCustomDnsServersSettingsPage();

        _shellRobot
            .VerifyCurrentPage(CUSTOM_DNS_SERVERS_PAGE_TITLE, true)
            .DoNavigateBackward();
    }

    [TearDown]
    public void TestCleanup()
    {
        Cleanup();
    }
}