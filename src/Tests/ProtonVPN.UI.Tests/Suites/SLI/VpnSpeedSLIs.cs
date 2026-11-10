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
using System.Threading;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.Annotations;
using ProtonVPN.UI.Tests.Enums.Locations;
using ProtonVPN.UI.Tests.TestsHelper.UiFlows;

namespace ProtonVPN.UI.Tests.Tests.SliTests;

[TestFixture]
[Category("SLI")]
[Workflow("main_measurements")]
public class VpnSpeedSLIs : SliSetUp
{
    private const Country COUNTRY_NAME = Country.Germany;

    [SetUp]
    public void TestInitialize()
    {
        LaunchClient();
        CommonUiFlows.FullLogin(TestUserData.PlusUser, TestConstants.IsProTunVersion);

        try
        {
            HomeRobot
                .Verify.IsUpsellModalDisplayed()
                .DismissUpsellModal();
        }
        catch { }
    }


    [Test]
    [Sli("network_speed_disconnected")]
    [Retry(3)]
    public void ConnectionSpeedDisconnected()
    {
        // let the network settle after launch/login
        Thread.Sleep(TestConstants.TenSecondsTimeout);
        SliHelper.AddNetworkSpeedToMetrics("download_speed_disconnected", "upload_speed_disconnected");
    }

    [Test]
    [TestCaseSource(typeof(TestConstants), nameof(TestConstants.AllNonProTunProtocols))]
    [Retry(3)]
    public void AllNonProTunProtocolsConnectionSpeed(Protocol protocol)
    {
        string sliName = protocol switch
        {
            Protocol.WireGuardUdp => "wireguard_udp_network_speed",
            Protocol.WireGuardTcp => "wireguard_tcp_network_speed",
            Protocol.WireGuardTls => "wireguard_tls_network_speed",
            Protocol.OpenVpnUdp => "openvpn_udp_network_speed",
            Protocol.OpenVpnTcp => "openvpn_tcp_network_speed",
            _ => throw new ArgumentOutOfRangeException(nameof(protocol), protocol, "Unmapped protocol for SLI naming.")
        };

        SliHelper.StartCustomRun(sliName);

        PerformProtocolSpeedTest(protocol);
    }

    [Test]
    [TestCaseSource(typeof(TestConstants), nameof(TestConstants.ProTunProtocols))]
    [Retry(3)]
    public void ProTunConnectionSpeed(Protocol protocol)
    {
        if (TestConstants.IsProTunVersion)
        {
            string sliName = protocol switch
            {
                Protocol.ProTunUdp => "protun_udp_network_speed",
                Protocol.ProTunTcp => "protun_tcp_network_speed",
                Protocol.ProTunTls => "protun_tls_network_speed",
                _ => throw new ArgumentOutOfRangeException(nameof(protocol), protocol, "Unmapped ProTun protocol for SLI naming.")
            };

            SliHelper.StartCustomRun(sliName);

            PerformProtocolSpeedTest(protocol, shouldEnableProTun: true);
        }
        else
        {
            Assert.Ignore("ProTUN is not available on v4");
        }
    }

    private void PerformProtocolSpeedTest(Protocol protocol, bool shouldEnableProTun = false)
    {
        CommonUiFlows.ChangeProtocol(protocol, shouldEnableProTun);

        SidebarRobot
            .SearchFor(COUNTRY_NAME.GetName())
            .ConnectToCountry(COUNTRY_NAME);

        HomeRobot.Verify.IsConnected();

        // Allow some time for the network to settle down.
        Thread.Sleep(TestConstants.TenSecondsTimeout);

        SliHelper.AddNetworkSpeedToMetrics("download_speed_connected", "upload_speed_connected");

        HomeRobot.Disconnect();
    }
}
