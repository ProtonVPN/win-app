/*
 * Copyright (c) 2025 Proton AG
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
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("2")]
[Category("ARM")]
public class KillSwitchTests : FreshSessionSetUp
{
    [SetUp]
    public void TestInitialize()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test, Order(0)]
    public void KillSwitchEnabled()
    {
        EnableKillSwitch(KillSwitchMode.Standard);
        ConnectAndVerify();
    }

    [Test, Order(1)]
    public void SignOutWithStandardKillSwitchEnabled()
    {
        EnableKillSwitch(KillSwitchMode.Standard);
        
        NavigationRobot
            .Verify.IsOnHomePage()
                   .IsOnLocationDetailsPage();

        ConnectAndVerify();

        NavigationRobot
            .Verify.IsOnConnectionDetailsPage();

        HomeRobot.ExpandKebabMenuButton();
        SettingRobot
            .SignOut()
            .ConfirmSignOut();

        LoginRobot.Verify.IsLoginWindowDisplayed();

        AssertInternetAvailability(true);
    }

    [Test, Order(2)]
    public void ExitTheAppWithStandardKillSwitchEnabled()
    {
        EnableKillSwitch(KillSwitchMode.Standard);

        NavigationRobot
            .Verify.IsOnHomePage()
                   .IsOnLocationDetailsPage();

        ConnectAndVerify();

        NavigationRobot
            .Verify.IsOnConnectionDetailsPage();

        HomeRobot.ExpandKebabMenuButton();
        SettingRobot.ExitTheApp();

        AssertInternetAvailability(true);
    }

    [Test, Order(3)]
    public void InternetConnectionBlockedAdvancedKillSwitchEnabled()
    {
        EnableKillSwitch(KillSwitchMode.Advanced);

        EnsureVpnConnectedFromHome();

        HomeRobot
            .Disconnect()
            .Verify.IsAdvancedKillSwitchActivated();

        AssertInternetAvailability(false);
    }

    [Test, Order(4)]
    public void ExitTheAppWithAdvancedKillSwitchEnabled()
    {
        EnableKillSwitch(KillSwitchMode.Advanced);

        EnsureVpnConnectedFromHome();

        HomeRobot.ExpandKebabMenuButton();
        SettingRobot.ExitTheAppWithConfirmation();

        AssertInternetAvailability(false);
    }

    [Test, Order(5)]
    public void DisableAdvancedKillSwitchFromSignInPage()
    {
        EnableKillSwitch(KillSwitchMode.Advanced);

        EnsureVpnConnectedFromHome();

        HomeRobot.ExpandKebabMenuButton();
        SettingRobot.SignOut()
            .ConfirmSignOut();

        NavigationRobot
            .Verify.IsOnLoginPage();

        AssertInternetAvailability(false);

        LoginRobot
            .Verify.IsAdvancedKillSwitchDisplayed()
            .DisableKillSwitch();

        AssertInternetAvailability(true);
    }

    [Test, Order(6)]
    public void DisableKillSwitchFromSettings()
    {
        DisableKillSwitch();
    }

    private void EnableKillSwitch(KillSwitchMode mode)
    {
        SettingRobot
            .OpenSettings()
            .Verify.IsKillSwitchDisabledStateDisplayed()
            .OpenKillSwitchSettings();

        NavigationRobot
            .Verify.IsOnKillSwitchPage();

        SettingRobot
            .ToggleKillSwitchSetting()
            .SelectKillSwitchMode(mode)
            .ApplySettings()
            .CloseSettings();

        NavigationRobot
            .Verify.IsOnHomePage()
                   .IsOnLocationDetailsPage();
    }

    private void DisableKillSwitch()
    {
        SettingRobot
            .OpenSettings()
            .OpenKillSwitchSettings();

        NavigationRobot
            .Verify.IsOnKillSwitchPage();

        SettingRobot
            .DisableKillSwitch()
            .CloseSettings();

        NavigationRobot
            .Verify.IsOnHomePage()
                   .IsOnLocationDetailsPage();
    }

    private static (string ipAddressBefore, string ipAddressAfter) ConnectAndVerify()
    {
        string ipAddressBefore = NetworkUtils.GetIpAddressWithRetry();

        HomeRobot
            .Verify.IsDisconnected()
            .ConnectViaConnectionCard()
            .Verify.IsConnecting()
                   .IsConnected();

        string ipAddressAfter = NetworkUtils.GetIpAddressWithRetry();
        HomeRobot.Verify.AssertVpnConnectionEstablished(ipAddressBefore, ipAddressAfter);

        return (ipAddressBefore, ipAddressAfter);
    }

    private static void EnsureVpnConnectedFromHome()
    {
        NavigationRobot
            .Verify.IsOnHomePage()
                   .IsOnLocationDetailsPage();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnecting()
                   .IsConnected();

        NavigationRobot
            .Verify.IsOnConnectionDetailsPage();
    }

    private static void AssertInternetAvailability(bool shouldBeAvailable)
    {
        bool isAvailable = NetworkUtils.IsInternetAvailable();
        if (shouldBeAvailable)
        {
            Assert.That(isAvailable, Is.True, "Expected internet to be available.");
        }
        else
        {
            Assert.That(isAvailable, Is.False, "Expected internet to not be available.");
        }
    }
}
