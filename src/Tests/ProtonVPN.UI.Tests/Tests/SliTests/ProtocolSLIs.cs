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

using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Annotations;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.SliTests;

[TestFixture]
[Category("SLI")]
[Workflow("protocol_performance")]
public class ProtocolSLIs : SliSetUp
{
    private static readonly Dictionary<Protocol, Protocol> _proTunProtocolMapping = new()
    {
        { Protocol.WireGuardUdp, Protocol.ProTunUdp },
        { Protocol.WireGuardTcp, Protocol.ProTunTcp },
        { Protocol.WireGuardTls, Protocol.ProTunTls },
    };

    [SetUp]
    public void TestInitialize()
    {
        LaunchClient();
        CommonUiFlows.FullLogin(TestUserData.PlusUser, TestConstants.IsProTunVersion);
    }

    [Test]
    [Duration, TestStatus]
    [Sli("wireguard_udp")]
    public void WireGuardUdpConnectionSpeed()
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
    public void WireGuardTcpConnectionSpeed()
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
    public void WireGuardTlsConnectionSpeed()
    {
        PerformProtocolTest(Protocol.WireGuardTls);
    }

    private void PerformProtocolTest(Protocol protocol)
    {
        bool isProTunWireGuard = TestConstants.IsProTunVersion && SliHelper.SliName?.StartsWith("wireguard") == true;
        
        string? protunPrefix = isProTunWireGuard ? "protun_" : null;
        SliHelper.SliName = protunPrefix + SliHelper.SliName;

        protocol = isProTunWireGuard && _proTunProtocolMapping.TryGetValue(protocol, out Protocol proTunValue)
             ? proTunValue
             : protocol;

        CommonUiFlows.ChangeProtocol(protocol, isProTunWireGuard);

        // Two time connection is needed to test real conditions, when everything was setup.
        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected()
            .Disconnect();

        if (!TestConstants.IsProTunVersion)
        {
            ConfirmationRobot
                .CancelAction();
        }

        // Imitate users delay
        Thread.Sleep(TestConstants.TenSecondsTimeout);

        HomeRobot.ConnectViaConnectionCard();

        SliHelper.MeasureTime(() =>
        {
            HomeRobot.Verify.IsConnected();
        });
        SliHelper.MeasureTestStatus(() =>
        {
            HomeRobot.Verify.IsProtocolDisplayed(protocol);
        });

        HomeRobot
            .Disconnect()
            .Verify.IsDisconnected();
    }
}