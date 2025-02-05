/*
 * Copyright (c) 2024 Proton AG
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

using NUnit.Framework;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using static ProtonVPN.UI.Tests.TestsHelper.TestConstants;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("3")]
[Category("ARM")]
public class ProtocolTests : FreshSessionSetUp
{
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
    public void ConnectUsingWireguardTcp()
    {
        PerformProtocolTest(Protocol.WireGuardTcp);
    }

    [Test]
    public void ConnectUsingStealth()
    {
        PerformProtocolTest(Protocol.WireGuardTls);
    }

    [Test]
    public void ConnectUsingWireguardUdp()
    {
        PerformProtocolTest(Protocol.WireGuardUdp);
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

    private void PerformProtocolTest(Protocol protocol) {
        SettingRobot
            .OpenSettings()
            .OpenProtocolSettings()
            .SelectProtocol(protocol)
            .ApplySettings()
            .CloseSettings();

        SidebarRobot.ConnectToFastest();
        HomeRobot.Verify.IsConnected()
            .IsProtocolDisplayed(protocol);
    }
}
