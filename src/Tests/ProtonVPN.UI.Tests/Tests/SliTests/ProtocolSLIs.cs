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
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.Annotations;
using static ProtonVPN.UI.Tests.TestsHelper.TestConstants;

namespace ProtonVPN.UI.Tests.Tests.SliTests;

[TestFixture]
[Category("SLI")]
[Workflow("protocol_performance")]
public class ProtocolSLIs : SliSetUp
{
    private readonly bool _isProtun = Version.TryParse(TestEnvironment.GetAppVersion(), out Version? v) && v.Major >= 5;

    [SetUp]
    public void TestInitialize()
    {
        LaunchApp();
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test]
    [Duration, TestStatus]
    [Sli("wireguard_udp")]
    public void WireguardUdpConnectionSpeed()
    {
        PerformProtocolTest(Protocol.WireGuardUdp);
    }

    [Test]
    [Duration, TestStatus]
    [Sli("openvpn_udp")]
    public void OpenVpnUdpConnectionSpeed()
    {
        PerformProtocolTest(Protocol.OpenVpnUdp);
    }

    [Test]
    [Duration, TestStatus]
    [Sli("wireguard_tcp")]
    public void WireguardTcpConnectionSpeed()
    {
        PerformProtocolTest(Protocol.WireGuardTcp);
    }

    [Test]
    [Duration, TestStatus]
    [Sli("openvpn_tcp")]
    public void OpenVpnTcpConnectionSpeed()
    {
        PerformProtocolTest(Protocol.OpenVpnTcp);
    }

    [Test]
    [Duration, TestStatus]
    [Sli("wireguard_tls")]
    public void WireguardTlsConnectionSpeed()
    {
        PerformProtocolTest(Protocol.WireGuardTls);
    }

    private void PerformProtocolTest(Protocol protocol)
    {
        bool isProtunWireguard = _isProtun && SliHelper.SliName?.StartsWith("wireguard") == true;

        SettingRobot
            .OpenSettings()
            .OpenProtocolSettings();

        if (isProtunWireguard)
        {
            SliHelper.SliName = "protun_" + SliHelper.SliName;

            SettingRobot
                .ToggleProtun()
                .Verify.IsProtunEnabled();
        }

        SettingRobot
            .SelectProtocol(protocol)
            .ApplySettings()
            .CloseSettings();

        // Two time connection is needed to test real conditions, when everything was setup.
        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected()
            .Disconnect();

        // Imitate users delay
        Thread.Sleep(TestConstants.TenSecondsTimeout);

        HomeRobot.ConnectViaConnectionCard();

        SliHelper.MeasureTime(() =>
        {
            HomeRobot.Verify.IsConnected();
        });
        SliHelper.MeasureTestStatus(() =>
        {
            HomeRobot.Verify.IsProtocolDisplayed(protocol, isProtunWireguard);
        });

        HomeRobot
            .Disconnect()
            .Verify.IsDisconnected();
    }
}