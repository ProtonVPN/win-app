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
        HomeRobot.ConnectViaConnectionCard()
            .Verify.IsConnected();

        string ipAddressConnected = NetworkUtils.GetIpAddressWithRetry();

        HomeRobot.ExpandKebabMenuButton();
        SettingRobot
            .SignOut()
            .ConfirmSignOut();

        LoginRobot.Verify.IsLoginWindowDisplayed();

        string ipAddressAfterLogout = NetworkUtils.GetIpAddressWithRetry();

        Assert.That(ipAddressConnected.Equals(ipAddressAfterLogout), Is.False, "User was not disconnected on logout.");
    }

    [Test]
    public void CancelLogoutWhileConnectedToVpn()
    {
        HomeRobot.ConnectViaConnectionCard()
            .Verify.IsConnected();

        HomeRobot.ExpandKebabMenuButton();
        SettingRobot
            .SignOut()
            .CancelSignOut();

        HomeRobot
            .Verify.IsConnected();
    }

    [Test]
    public void LogoutViaKebabMenu()
    {
        HomeRobot.ExpandKebabMenuButton();
        SettingRobot.SignOut()
            .ConfirmSignOut();
        LoginRobot.Verify.IsLoginWindowDisplayed();
    }

    [Test]
    public void LogoutViaAccountMenu()
    {
        SettingRobot.OpenSettings()
            .Verify.IsSettingsPageDisplayed()
            .ExpandAccountDropdown()
            .SignOut()
            .ConfirmSignOut();
        LoginRobot.Verify.IsLoginWindowDisplayed();
    }
}
