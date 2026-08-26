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
using ProtonVPN.UI.Tests.Annotations;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.TestsHelper.UiFlows;

namespace ProtonVPN.UI.Tests.Tests.SliTests;

[TestFixture]
[Category("SLI")]
[Workflow("protocol_performance")]
public class ProtocolSLIs : SliSetUp
{
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
    [Duration, TestStatus]
    [TestCaseSource(typeof(TestConstants), nameof(TestConstants.AllNonProTunProtocols))]
    public void AllNonProTunProtocolsConnectionTime(Protocol protocol)
    {
        string sliName = protocol switch
        {
            Protocol.WireGuardUdp => "wireguard_udp",
            Protocol.WireGuardTcp => "wireguard_tcp",
            Protocol.WireGuardTls => "wireguard_tls",
            Protocol.OpenVpnUdp => "openvpn_udp",
            Protocol.OpenVpnTcp => "openvpn_tcp",
            _ => throw new ArgumentOutOfRangeException(nameof(protocol), protocol, "Unmapped protocol for SLI naming.")
        };

        SliHelper.StartCustomRun(sliName);

        PerformProtocolTest(protocol);
    }

    [Test]
    [Duration, TestStatus]
    [TestCaseSource(typeof(TestConstants), nameof(TestConstants.ProTunProtocols))]
    public void ProTunConnectionTime(Protocol protocol)
    {
        if (TestConstants.IsProTunVersion)
        {
            string sliName = protocol switch
            {
                Protocol.ProTunUdp => "protun_udp",
                Protocol.ProTunTcp => "protun_tcp",
                Protocol.ProTunTls => "protun_tls",
                _ => throw new ArgumentOutOfRangeException(nameof(protocol), protocol, "Unmapped ProTun protocol for SLI naming.")
            };

            SliHelper.StartCustomRun(sliName);

            PerformProtocolTest(protocol, shouldEnableProTun: true);
        }
        else
        {
            Assert.Ignore("ProTUN is not available on v4");
        }
    }

    private void PerformProtocolTest(Protocol protocol, bool shouldEnableProTun = false)
    {
        CommonUiFlows.ChangeProtocol(protocol, shouldEnableProTun);

        // Two time connection is needed to test real conditions, when everything was setup.
        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected()
            .Disconnect();

        if (!TestConstants.IsProTunVersion)
        {
            ConfirmationRobot.CancelAction();
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