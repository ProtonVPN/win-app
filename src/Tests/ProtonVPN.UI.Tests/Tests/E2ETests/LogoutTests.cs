﻿/*
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
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("2")]
[Category("ARM")]
public class LogoutTests : FreshSessionSetUp
{
    [SetUp]
    public void SetUp()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test]
    public void LogoutWhileConnectedToVpn()
    {
        HomeRobot.ConnectToDefaultConnection()
            .Verify.IsConnected();

        string ipAddressConnected = NetworkUtils.GetIpAddress();

        SettingRobot.OpenSettings()
            .Logout()
            .ConfirmSignOut();

        LoginRobot.Verify.LoginWindowIsDisplayed();

        string ipAddressAfterLogout = NetworkUtils.GetIpAddress();

        Assert.That(ipAddressConnected.Equals(ipAddressAfterLogout), Is.False, "User was not disconnected on logout.");
    }

    [Test]
    public void CancelLogoutWhileConnectedToVpn()
    {
        HomeRobot.ConnectToDefaultConnection()
            .Verify.IsConnected();

        SettingRobot.OpenSettings()
            .Logout()
            .CancelSignOut();

        SettingRobot.Verify.SettingsPageIsDisplayed();
    }
}
