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
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using Windows.Networking.Connectivity;
using static ProtonVPN.UI.Tests.TestsHelper.TestConstants;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("3")]
[Category("ARM")]
public class ProtocolTests : FreshSessionSetUp
{
    private const string LINE_TO_LOOK_FOR = "OpenVpnAdapter";
    private const string OPENVPN_TUN_ADAPTER_LOG_LINE = "VpnProtocol: 'OpenVpnUdp', OpenVpnAdapter: 'Tun'";
    private const string OPENVPN_TAP_ADAPTER_LOG_LINE = "VpnProtocol: 'OpenVpnUdp', OpenVpnAdapter: 'Tap'";

    private const string OPENVPN_TUN_ADAPTER_FULL_NAME = "ProtonVPN TUN Tunnel";
    private const string OPENVPN_TAP_ADAPTER_FULL_NAME = "TAP-ProtonVPN Windows Adapter V9";

    private static readonly List<Protocol> _wireGuardProtocols = [Protocol.WireGuardUdp, Protocol.WireGuardTcp, Protocol.WireGuardTls];

    private static readonly string _serviceLogsPath = TestEnvironment.GetServiceLogsPath();

    [SetUp]
    public void TestInitialize()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test]
    public void ConnectUsingOpenVpnUdp()
    {
        PerformProtocolTest(Protocol.OpenVpnUdp);
    }

    [Test]
    public void ConnectUsingOpenVpnTcp()
    {
        PerformProtocolTest(Protocol.OpenVpnTcp);
    }

    [Test]
    public void ConnectUsingWireGuardTcp()
    {
        PerformProtocolTest(Protocol.WireGuardTcp);
    }

    [Test]
    public void ConnectUsingStealth()
    {
        PerformProtocolTest(Protocol.WireGuardTls);
    }

    [Test]
    public void ConnectUsingWireGuardUdp()
    {
        PerformProtocolTest(Protocol.WireGuardUdp);
    }

    [Test]
    public void ConnectUsingProtunWireGuardTcp()
    {
        PerformProtocolTest(Protocol.WireGuardTcp, shouldEnableProTun: true);
    }

    [Test]
    public void ConnectUsingProtunStealth()
    {
        PerformProtocolTest(Protocol.WireGuardTls, shouldEnableProTun: true);
    }

    [Test]
    public void ConnectUsingProtunWireGuardUdp()
    {
        PerformProtocolTest(Protocol.WireGuardUdp, shouldEnableProTun: true);
    }

    [Test]
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
    [Ignore("JIRA - VPNWIN-2605")]
    public void ConnectUsingWireGuardWhileConnectedToNativeWireGuard()
    {
        ScriptHelper.ConnectToWireGuard();
        ScriptHelper.VerifyWireGuardIsConnected();

        try
        {
            foreach (Protocol wireGuardProtocol in _wireGuardProtocols)
            {
                SettingRobot
                    .OpenSettings()
                    .OpenProtocolSettings()
                    .SelectProtocol(wireGuardProtocol)
                    .ApplySettings()
                    .CloseSettings();

                HomeRobot
                    .ConnectViaConnectionCard()
                    .Verify.IsWireGuardErrorDisplayed()
                    .CloseConnectionError()
                    .Verify.IsDisconnected();
            }
        }
        finally
        {
            ScriptHelper.DisconnectFromWireGuard();
        }
    }

    [Test]
    public void OpenVpnAdapterTAP()
    {
        VerifyOpenVpnAdapter(
            OpenVpnAdapter.TAP, 
            OPENVPN_TAP_ADAPTER_LOG_LINE, 
            OPENVPN_TAP_ADAPTER_FULL_NAME);
    }

    [Test]
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
        SettingRobot
            .OpenSettings()
            .OpenProtocolSettings()
            .SelectProtocol(Protocol.OpenVpnUdp)
            .ApplySettings()
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
        SettingRobot
            .OpenSettings()
            .OpenProtocolSettings();

        HandleProtun(shouldEnableProTun);

        SettingRobot
            .SelectProtocol(protocol)
            .ApplySettings()
            .CloseSettings();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected()
                   .IsProtocolDisplayed(protocol, shouldEnableProTun);
    }

    private void HandleProtun(bool shouldEnableProTun)
    {
        if (TestConstants.IsProtunVersion)
        {
            if (shouldEnableProTun)
            {
                SettingRobot.EnableProtunToggle();
            }
            else
            {
                SettingRobot.DisableProtunToggle();
            }
        }
    }
}