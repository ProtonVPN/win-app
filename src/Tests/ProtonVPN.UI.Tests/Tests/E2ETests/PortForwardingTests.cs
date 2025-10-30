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

using System.Net.Sockets;
using System;
using NUnit.Framework;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using System.Net;
using System.Threading;
using System.Net.Http;
using System.Threading.Tasks;
using ProtonVPN.UI.Tests.Robots;
using Clipboard = System.Windows.Forms.Clipboard;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("3")]
[Category("ARM")]
public class PortForwardingTests : FreshSessionSetUp
{
    [SetUp]
    public void SetUp()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test]
    public void VerifyPortForwardingNotification()
    {
        EnablePortForwardingAndConnect();

        SettingRobot.ClickCopyPortNumber();

        int uiPort = GetForwardedPortFromClipboard();

        DesktopRobot.Verify
            .IsDisplayed()
            .PortMatchesUI(uiPort);
    }

    [Test]
    public async Task PortForwardingOpensThePortAsync()
    {
        EnablePortForwardingAndConnect();

        string ipAddressConnected = GetExternalIpAddress();
        SettingRobot.ClickCopyPortNumber();
        int forwardedPort = GetForwardedPortFromClipboard();

        TcpListener listener = StartTcpListener(forwardedPort);
        await Task.Delay(5000);

        bool isPortOpen = await IsPortOpenAsync(ipAddressConnected, forwardedPort);

        listener.Stop();

        Assert.That(isPortOpen, Is.True,
            $"Port {forwardedPort} is not reported as open on {ipAddressConnected} by external port-check.");
    }

    [Test]
    public void VerifyCopiedPortForwardingNotification()
    {
        EnablePortForwardingAndConnect();

        SettingRobot.ClickCopyPortNumber();

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

        SettingRobot.ClickCopyPortNumber();

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
            .ConnectToFastest();

        HomeRobot
            .Verify.IsConnecting()
                   .IsConnected();
    }

    private static string GetExternalIpAddress()
    {
        return NetworkUtils.GetIpAddressWithRetry();
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

    private static TcpListener StartTcpListener(int port)
    {
        TcpListener listener = new(IPAddress.Any, port);
        listener.Start();
        return listener;
    }

    private static async Task<bool> IsPortOpenAsync(string ip, int port)
    {
        try
        {
            string url = $"{TestConstants.PORT_CHECKER_API_BASE_URL}/{ip}/{port}";
            using HttpClient client = new();
            HttpResponseMessage response = await client.GetAsync(url);
            string result = await response.Content.ReadAsStringAsync();

            return result.Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception)
        {
            Assert.Fail("External port-check request failed.");
            return false;
        }
    }    
}