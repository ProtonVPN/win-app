/*
 * Copyright (c) 2025 Proton AG
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NUnit.Framework;
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

    [SetUp]
    public void SetUp()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test]
    [Retry(3)]
    public async Task PortForwardingOpensThePortAsync()
    {
        TorrentHelper.AllowAriaFirewallScript();
        TorrentHelper.StopAndCleanup();

        EnablePortForwardingAndConnect();

        string? ipAddressConnected = HomeRobot.GetVpnServerIp();
        Assert.That(ipAddressConnected, Is.Not.Null);

        SettingRobot.ClickCopyPortNumber();
        int forwardedPort = GetForwardedPortFromClipboard();

        await TorrentHelper.StartTorrentOnPortAsync(forwardedPort);

        await TorrentHelper.IsPortOpenAsync(ipAddressConnected!, forwardedPort);

        TorrentHelper.StopAndCleanup();
    }

    [Test]
    public void VerifyCopiedPortForwardingNotification()
    {
        EnablePortForwardingAndConnect();

        SettingRobot
            .ClickCopyPortNumber();

        int uiPort = GetForwardedPortFromClipboard();

        DesktopRobot.Verify
            .IsDisplayed()
            .PortMatchesUI(uiPort)
            .ClickCopyMatchesUI(uiPort);
    }

    [Test]
    public void VerifyPortForwardingHoverOver()
    {
        EnablePortForwardingAndConnect();

        SettingRobot
            .ClickCopyPortNumber();

        int uiPort = GetForwardedPortFromClipboard();

        DesktopRobot
            .HoverOverPortForwarding()
            .ClickHoverCopyPort();

        int hoverPort = GetForwardedPortFromClipboard();

        Assert.That(hoverPort, Is.EqualTo(uiPort),
                $"Port in toast ({hoverPort}) does not match port in UI ({uiPort}).");
    }

    private static void EnablePortForwardingAndConnect()
    {
        SettingRobot
            .OpenSettings()
            .OpenPortForwardingSettings()
            .TogglePortForwardingnSetting()
            .ApplySettings()
            .CloseSettings();

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
}