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
    public void WarningIsDisplayedWhenAdvancedKillSwitchIsOn()
    {
        HomeRobot
            .Verify.IsDisconnected();

        SettingRobot
            .OpenSettings()
            .Verify.IsKillSwitchDisabledStateDisplayed()
            .CloseSettings();

        EnableKillSwitch(KillSwitchMode.Advanced);

        HomeRobot
            .Verify.IsAdvancedKillSwitchActivated()
            .HoverOverKillSwitchFlyoutWidget()
            .Verify.IsAdvancedKillSwitchTextInFlyoutMenu();

        SettingRobot
            .OpenSettings()
            .Verify.IsKillSwitchEnabledStateDisplayed(KillSwitchMode.Advanced);

        //TODO: A Warning sign is added as a badge to the Kill Switch icon;
    }

    [Test, Order(1)]
    public void EnableKillSwitchFromSettings()
    {
        EnableKillSwitch(KillSwitchMode.Standard);
        NetworkUtils.AssertInternetAvailability(true);

        EnableKillSwitch(KillSwitchMode.Advanced);
        NetworkUtils.AssertInternetAvailability(false);
    }

    [Test, Order(2)]
    public void SignOutWithStandardKillSwitchEnabled()
    {
        EnableKillSwitch(KillSwitchMode.Standard);

        NavigationRobot
            .Verify.IsOnHomePage()
                   .IsOnLocationDetailsPage();

        ConnectAndVerify();

        NavigationRobot
            .Verify.IsOnConnectionDetailsPage();

        CommonUiFlows.Logout();

        NetworkUtils.AssertInternetAvailability(true);
    }

    [Test, Order(3)]
    public void ExitTheAppWithStandardKillSwitchEnabled()
    {
        EnableKillSwitch(KillSwitchMode.Standard);

        NavigationRobot
            .Verify.IsOnHomePage()
                   .IsOnLocationDetailsPage();

        ConnectAndVerify();

        NavigationRobot
            .Verify.IsOnConnectionDetailsPage();

        HomeRobot
            .ExpandKebabMenuButton()
            .ExitViaKebabMenuWithConfirmation();

        NetworkUtils.AssertInternetAvailability(true);
    }

    [Test, Order(4)]
    public void SignOutWithAdvancedKillSwitchEnabled()
    {
        EnableKillSwitch(KillSwitchMode.Advanced);

        NavigationRobot
            .Verify.IsOnHomePage()
                   .IsOnLocationDetailsPage();

        ConnectAndVerify();

        NavigationRobot
            .Verify.IsOnConnectionDetailsPage();

        CommonUiFlows.Logout();

        LoginRobot
            .Verify.IsAdvancedKillSwitchDisplayed();

        NetworkUtils.AssertInternetAvailability(false);
    }

    [Test, Order(5)]
    public void InternetConnectionBlockedAdvancedKillSwitchEnabled()
    {
        EnableKillSwitch(KillSwitchMode.Advanced);

        EnsureVpnConnectedFromHome();

        HomeRobot
            .Disconnect()
            .Verify.IsAdvancedKillSwitchActivated();

        NetworkUtils.AssertInternetAvailability(false);
    }

    [Test, Order(6)]
    public void ExitTheAppWithAdvancedKillSwitchEnabled()
    {
        EnableKillSwitch(KillSwitchMode.Advanced);

        EnsureVpnConnectedFromHome();

        HomeRobot
            .ExpandKebabMenuButton()
            .ExitViaKebabMenuWithConfirmation();

        NetworkUtils.AssertInternetAvailability(false);
    }

    [Test, Order(7)]
    public void DisableAdvancedKillSwitchFromSignInPage()
    {
        EnableKillSwitch(KillSwitchMode.Advanced);

        EnsureVpnConnectedFromHome();

        CommonUiFlows.Logout();

        NetworkUtils.AssertInternetAvailability(false);

        LoginRobot
            .Verify.IsAdvancedKillSwitchDisplayed()
            .DisableKillSwitch();

        NetworkUtils.AssertInternetAvailability(true);
    }

    [Test, Order(8)]
    public void DisableKillSwitchFromSettings()
    {
        EnableKillSwitch(KillSwitchMode.Advanced);
        NetworkUtils.AssertInternetAvailability(false);

        SettingRobot
            .OpenSettings()
            .OpenKillSwitchSettings();

        NavigationRobot
            .Verify.IsOnKillSwitchPage();

        SettingRobot
            .DisableKillSwitchToggle()
            .ApplySettings()
            .CloseSettings();

        NetworkUtils.AssertInternetAvailability(true);
    }

    private void EnableKillSwitch(KillSwitchMode mode)
    {
        SettingRobot
            .OpenSettings()
            .OpenKillSwitchSettings();

        NavigationRobot
            .Verify.IsOnKillSwitchPage();

        SettingRobot
            .EnableKillSwitchToggle()
            .SelectKillSwitchMode(mode)
            .ApplySettings()
            .CloseSettings();

        NavigationRobot
            .Verify.IsOnHomePage()
                   .IsOnLocationDetailsPage();
    }

    private static void ConnectAndVerify()
    {
        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        NetworkUtils.AssertInternetAvailability(true);
    }

    private static void EnsureVpnConnectedFromHome()
    {
        NavigationRobot
            .Verify.IsOnHomePage()
                   .IsOnLocationDetailsPage();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        NavigationRobot
            .Verify.IsOnConnectionDetailsPage();
    }
}