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
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("2")]
[Category("ARM")]
public class NetShieldTests : FreshSessionSetUp
{
    [SetUp]
    public void TestInitialize()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test]
    public void NetshieldOnLevelOne()
    {
        ConnectAndVerifyIsConnected();

        SettingRobot
            .Verify.NetshieldIsBlocking(NetShieldMode.BlockMalwareOnly);
    }

    [Test]
    public void NetshieldOnLevelTwo()
    {
        SettingRobot
            .OpenSettings()
            .OpenNetShieldSettings()
            .SelectNetShieldMode(NetShieldMode.BlockAdsMalwareTrackers)
            .ApplySettings()
            .CloseSettings();

        ConnectAndVerifyIsConnected();

        SettingRobot
            .Verify.NetshieldIsBlocking(NetShieldMode.BlockAdsMalwareTrackers);
    }

    [Test]
    public void NetshieldOff()
    {
        SettingRobot
            .OpenSettings()
            .OpenNetShieldSettings()
            .ToggleNetShieldSetting()
            .ApplySettings()
            .CloseSettings();

        ConnectAndVerifyIsConnected();

        SettingRobot
            .Verify.NetshieldIsNotBlocking();
    }

    [Test]
    public void PaidSettingsAreNotTransferedOnPaidToFreeUserSwitch()
    {
        SettingRobot
            .OpenSettings()
            .OpenNetShieldSettings()
            .SelectNetShieldMode(NetShieldMode.BlockAdsMalwareTrackers)
            .ApplySettings()
            .CloseSettings();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();
        SettingRobot
            .Verify.NetshieldIsBlocking(NetShieldMode.BlockAdsMalwareTrackers);

        SettingRobot.OpenSettings()
            .ExpandAccountDropdown()
            .SignOut()
            .ConfirmSignOut();

        CommonUiFlows.FullLogin(TestUserData.FreeUser);

        HomeRobot.ConnectViaConnectionCard()
            .Verify.IsConnected();

        SettingRobot.Verify.NetshieldIsNotBlocking()
            .OpenSettings()
            .Verify.NetshieldShowsDisableState();
    }

    private void ConnectAndVerifyIsConnected()
    {
        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();
    }
}