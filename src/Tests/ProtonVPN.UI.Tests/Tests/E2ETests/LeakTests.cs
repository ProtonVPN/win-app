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
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using static ProtonVPN.UI.Tests.TestsHelper.TestConstants;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("2")]
[Category("ARM")]
public class LeakTests : FreshSessionSetUp
{
    private const string COUNTRY_NAME = "Australia";
    private const string SECOND_COUNTRY_NAME = "Argentina";
    private const string APP_TO_CHECK = "Google Chrome";

    private List<string> _dnsListNotConnected = [];

    [SetUp]
    public void TestInitialize()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
        _dnsListNotConnected = DnsLeakHelper.GetDnsServers();
    }

    [Test]
    public void WebRtcIsNotLeakingWhileConnected()
    {
        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        string? ipAddressConnected = HomeRobot.GetVpnServerIp();

        Assert.That(ipAddressConnected, Is.Not.Null);
        BrowserUtils.VerifyWebRtcNotLeaking(APP_TO_CHECK, ipAddressConnected!);

        CommonUiFlows.EnsureUserIsDisconnected();
    }

    [Test]
    public void DnsIsNotLeakingWhileConnected()
    {
        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        DnsLeakHelper.VerifyIsNotLeaking(_dnsListNotConnected);

        CommonUiFlows.EnsureUserIsDisconnected();
    }

    [Test]
    public void DnsIsNotLeakingOnReconnect()
    {
        CheckDnsLeaksWhileReconnecting();
        CommonUiFlows.EnsureUserIsDisconnected();
    }

    [Test]
    public void DnsIsNotLeakingWithKillSwitchOn()
    {
        try
        {
            EnableKillSwitch(KillSwitchMode.Standard);
            CheckDnsLeaksWhileReconnecting();

            EnableKillSwitch(KillSwitchMode.Advanced);
            CheckDnsLeaksWhileReconnecting();
        }
        finally
        {
            CommonUiFlows.EnsureUserIsDisconnected(shouldVerifyKillSwitch: true);
            DisableKillSwitch();
        }
    }

    [Test]
    [TestCaseSource(typeof(TestConstants), nameof(AllProtocols))]
    public void DnsIsNotLeakingUsingDifferentProtocols(Protocol protocol)
    {
        PerformProtocolTest(protocol);
    }

    [Test]
    [TestCaseSource(typeof(TestConstants), nameof(WireGuardProtocols))]
    public void DnsIsNotLeakingUsingDifferentProTunProtocols(Protocol protocol)
    {
        PerformProtocolTest(protocol, shouldEnableProTun: true);
    }

    private void PerformProtocolTest(Protocol protocol, bool shouldEnableProTun = false)
    {
        CommonUiFlows.ChangeProtocol(protocol, shouldEnableProTun);

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected()
                   .IsProtocolDisplayed(protocol, shouldEnableProTun);

        DnsLeakHelper.VerifyIsNotLeaking(_dnsListNotConnected);

        CommonUiFlows.EnsureUserIsDisconnected();
    }

    private void CheckDnsLeaksWhileReconnecting()
    {
        SidebarRobot
            .NavigateToAllCountriesTab()
            .ConnectToCountry(COUNTRY_NAME);
        HomeRobot
            .Verify.IsConnected();

        SidebarRobot
            .NavigateToSecureCoreCountriesTab()
            .ConnectToCountry(SECOND_COUNTRY_NAME);
        HomeRobot
            .Verify.IsConnecting();

        DnsLeakHelper.VerifyIsNotLeaking(_dnsListNotConnected);

        HomeRobot
            .Verify.IsConnected();

        DnsLeakHelper.VerifyIsNotLeaking(_dnsListNotConnected);
    }

    private void EnableKillSwitch(KillSwitchMode mode)
    {
        SettingRobot
            .OpenSettings()
            .OpenKillSwitchSettings()
            .EnableKillSwitchToggle()
            .SelectKillSwitchMode(mode)
            .ApplySettings()
            .CloseSettings();
    }

    private void DisableKillSwitch()
    {
        SettingRobot
            .OpenSettings()
            .OpenKillSwitchSettings()
            .DisableKillSwitchToggle()
            .ApplySettings()
            .CloseSettings();
    }
}
