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
using NUnit.Framework;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using static ProtonVPN.UI.Tests.TestsHelper.TestConstants;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("3")]
[Category("ARM")]
public class ProtocolTests : FreshSessionSetUp
{
    private const string LINE_TO_LOOK_FOR = "VpnProtocol: ";
    private const string OPENVPN_TUN_ADAPTER_LOG_LINE = "VpnProtocol: 'OpenVpnUdp', OpenVpnAdapter: 'Tun'";
    private const string OPENVPN_TAP_ADAPTER_LOG_LINE = "VpnProtocol: 'OpenVpnUdp', OpenVpnAdapter: 'Tap'";

    private const string OPENVPN_TUN_ADAPTER_FULL_NAME = "ProtonVPN TUN Tunnel";
    private const string OPENVPN_TAP_ADAPTER_FULL_NAME = "TAP-ProtonVPN Windows Adapter V9";

    private static readonly string _serviceLogsPath = TestEnvironment.GetServiceLogsPath();

    [SetUp]
    public void TestInitialize()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test]
	[Property("TestCaseId", "602365")]
	[TestCaseSource(typeof(TestConstants), nameof(AllNonProTunProtocols))]
    public void ConnectUsingDifferentProtocols(Protocol protocol)
    {
        PerformProtocolTest(protocol);
    }

    [Test]
	[Property("TestCaseId", "841907")]
	[TestCaseSource(typeof(TestConstants), nameof(ProTunProtocols))]
    public void ConnectUsingDifferentProTunProtocols(Protocol protocol)
    {
        PerformProtocolTest(protocol, shouldEnableProTun: true);
    }

    [Test]
	[Property("TestCaseId", "602412")]
	public void ChangeProtocolFromConnectionDetails()
    {
        PerformProtocolTest(Protocol.OpenVpnUdp);

        HomeRobot
            .ClickOnProtocolConnectionDetails()
            .ClickChangeProtocolButton();
        SettingRobot
            .SelectProtocol(Protocol.WireGuardTls)
            .Reconnect();

        HomeRobot
            .Verify.IsConnected()
            .IsProtocolDisplayed(Protocol.WireGuardTls);
    }

    [Test]
    [Property("TestCaseId", "602354")]
    [Ignore("Native WireGuard causes infinite connecting on the ProTUN build")]
    [TestCaseSource(nameof(WireGuardProtocols))]
    public void ConnectUsingWireGuardWhileConnectedToNativeWireGuard(Protocol wireGuardProtocol)
    {
        ScriptHelper.ConnectToWireGuard();
        ScriptHelper.VerifyWireGuardIsConnected();

        try
        {
            CommonUiFlows.ChangeProtocol(wireGuardProtocol);

            HomeRobot
                .ConnectViaConnectionCard()
                .Verify.IsWireGuardErrorDisplayed()
                .CloseConnectionError()
                .Verify.IsDisconnected();
        }
        finally
        {
            ScriptHelper.DisconnectFromWireGuard();
        }
    }

    [Test]
    [Property("TestCaseId", "602448")]
    public void OpenVpnAdapterTAP()
    {
        VerifyOpenVpnAdapter(
            OpenVpnAdapter.TAP,
            OPENVPN_TAP_ADAPTER_LOG_LINE,
            OPENVPN_TAP_ADAPTER_FULL_NAME);
    }

    [Test]
    [Property("TestCaseId", "602447")]
    public void OpenVpnAdapterTUN()
    {
        VerifyOpenVpnAdapter(
            OpenVpnAdapter.TUN,
            OPENVPN_TUN_ADAPTER_LOG_LINE,
            OPENVPN_TUN_ADAPTER_FULL_NAME);
    }

    private void VerifyOpenVpnAdapter(
        OpenVpnAdapter openVpnAdapter,
        string expectedLogLine,
        string expectedNetworkAdapterName)
    {
        CommonUiFlows.ChangeProtocol(Protocol.OpenVpnUdp, shouldEnableProTun: true);
        SettingRobot
            .OpenSettings()
            .OpenAdvancedSettings()
            .SelectOpenVpnAdapter(openVpnAdapter);

        if (openVpnAdapter == OpenVpnAdapter.TAP)
        {
            SettingRobot.ApplySettings();
        }

        SettingRobot.CloseSettings();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        WindowsUtils.AssertLogFile(_serviceLogsPath, LINE_TO_LOOK_FOR, expectedLogLine);
        NetworkUtils.AssertCorrectNetworkAdapter(expectedNetworkAdapterName);
    }

    private void PerformProtocolTest(Protocol protocol, bool shouldEnableProTun = false)
    {
        CommonUiFlows.ChangeProtocol(protocol, shouldEnableProTun);

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected()
                   .IsProtocolDisplayed(protocol);

        WindowsUtils.AssertLogFile(_serviceLogsPath, LINE_TO_LOOK_FOR, protocol.ToString());
    }
}