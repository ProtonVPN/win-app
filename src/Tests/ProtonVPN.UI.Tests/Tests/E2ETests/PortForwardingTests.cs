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
        EnablePortForwardingAndConnect();

        //string ipAddressConnected = NetworkUtils.GetIpAddressWithRetry();
        string? ipAddressConnected = HomeRobot.GetVpnServerIp();

        SettingRobot.ClickCopyPortNumber();
        int forwardedPort = GetForwardedPortFromClipboard();

        TestContext.WriteLine($"ip: {ipAddressConnected}, port: {forwardedPort}");
        Assert.That(ipAddressConnected, Is.Not.Null);

        TcpListener listener = StartTcpListener(forwardedPort);
        await Task.Delay(3_000);

        bool isPortOpen = await IsPortOpenAsync(ipAddressConnected!, forwardedPort);

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
            .ConnectToCountry(CountryCodes.GetCode(COUNTRY_NAME));
        //.ConnectToFastest();

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

    private static TcpListener StartTcpListener(int port)
    {
        TcpListener listener = new(IPAddress.Any, port);
        listener.Start();
        return listener;
    }

    private static async Task<bool> IsPortOpenAsync(string ip, int port)
    {
        using HttpClient client = new();
        string url = $"{TestConstants.PORT_CHECKER_API_BASE_URL}/{ip}/{port}";
        DateTime timeoutDate = DateTime.UtcNow + TimeSpan.FromSeconds(40);

        while (DateTime.UtcNow < timeoutDate)
        {
            HttpResponseMessage response = await client.GetAsync(url);
            string result = await response.Content.ReadAsStringAsync();

            if (result.Trim().Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            await Task.Delay(5000);
        }

        return false;
    }

}