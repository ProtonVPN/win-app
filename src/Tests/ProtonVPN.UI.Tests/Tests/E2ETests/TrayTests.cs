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
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using static ProtonVPN.UI.Tests.Robots.TrayRobot;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("4")]
public class TrayTests : BaseTest
{
    private const string PROFILE_NAME = "Streaming US";

    private const string RANDOM_COUNTRY = "Random country";

    private const string COUNTRY_NAME = "Belgium";
    private const string SECURE_CORE_COUNTRY_NAME = "Australia";
    private const string VIA_COUNTRY_ICELAND = "via Iceland";

    [OneTimeSetUp]
    public void SetUp()
    {
        LaunchApp();
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
        HomeRobot.MinimizeClientViaMinimizeButton();
        TrayRobot.Verify.IsTrayIconDisplayed();
    }

    [Test, Order(0)]
    [Retry(3)]
    public void TriggerTrayIconOnClick()
    {
        using (TrayApp)
        {
            TrayRobot
                .Verify.IsTrayFocused(true)
                .ClickTaskbar()
                .Verify.IsTrayFocused(false)
                .DoubleClickTrayApp()
                .Verify.IsHomeFocused(true);
        }

        NavigationRobot
            .Verify.IsOnHomePage()
                   .IsOnLocationDetailsPage();

        SidebarRobot
            .Verify.IsSidebarAvailable();

        HomeRobot.MinimizeClientViaMinimizeButton();
    }

    [Test, Order(1)]
    [Retry(3)]
    public void ChangeDefaultConnectionFromTray()
    {
        using (TrayApp)
        {
            HomeRobot
                .Verify.AssertAllVpnConnectionOptions()
                .SelectDefaultConnectionOption(VpnConnectionOption.Random)
                .Verify.ConnectionCardTitleEquals(RANDOM_COUNTRY);
        }
    }

    [Test, Order(2)]
    [Retry(3)]
    public void ConnectToAServerFromTray()
    {
        using (TrayApp)
        {
            SidebarRobot
                .Verify.IsNoRecentsLabelDisplayed();
            HomeRobot
                .Verify.ConnectionCardTitleEquals(RANDOM_COUNTRY)
                       .IsDisconnected()
                .ConnectViaConnectionCard()
                .Verify.IsConnecting()
                       .IsConnected()
                       .ConnectionCardTitleEquals(RANDOM_COUNTRY);
        }

        //TODO: assert padlock color - handle in the future
    }

    [Test, Order(3)]
    [Retry(3)]
    public void DisconnectFromAServerFromTray()
    {

        using (TrayApp)
        {
            HomeRobot
                .Verify.IsConnected()
                .Disconnect()
                .Verify.IsDisconnected();
        }

        //TODO: assert padlock color - handle in the future
    }

    [Test, Order(4)]
    [Ignore("Native WireGuard causes infinite connecting on the ProTUN build")]
    public void ConnectingErrorsInTray()
    {
        ScriptHelper.CreateWireGuardConfigFile();
        ScriptHelper.ConnectToWireGuard();
        Thread.Sleep(TestConstants.TwoSecondsTimeout);
        ScriptHelper.VerifyWireGuardIsConnected();

        try
        {
            using (TrayApp)
            {
                HomeRobot
                    .ConnectViaConnectionCard()
                    .Verify.IsWireGuardErrorDisplayed()
                           .CloseConnectionError()
                    .Verify.IsDisconnected();
                //TODO: assert icon color - handle in the future
            }
        }
        finally
        {
            ScriptHelper.DisconnectFromWireGuard();
            ScriptHelper.RemoveWireGuardConfigFile();
        }
    }

    [Test, Order(5)]
    [Retry(3)]
    public void WarningsInTray()
    {
        using (TrayApp)
        {
            TrayRobot
                .DoubleClickTrayApp();
        }

        ToggleKillSwitch(shouldBeEnabled: true);
        HomeRobot.MinimizeClientViaMinimizeButton();

        using (TrayApp)
        {
            HomeRobot
                .Verify.IsAdvancedKillSwitchActivated(true);

            //TODO: assert tray icon color and taskbar icon color - handle in the future

            TrayRobot
                .DoubleClickTrayApp();
        }

        ToggleKillSwitch(shouldBeEnabled: false);
        HomeRobot.MinimizeClientViaMinimizeButton();

        using (TrayApp)
        {
            HomeRobot
                .Verify.IsAdvancedKillSwitchActivated(false);

            //TODO: assert tray icon color and taskbar icon color - handle in the future
        }
    }

    [Test, Order(6)]
    [Retry(3)]
    public void OpenAppFromTray()
    {
        using (TrayApp)
        {
            TrayRobot
                .Verify.IsTrayFocused(true)
                       .IsHomeFocused(false)
                .ClickOpenAppButton()
                .Verify.IsTrayFocused(false)
                       .IsHomeFocused(true);
        }

        NavigationRobot
            .Verify.IsOnHomePage()
                   .IsOnLocationDetailsPage();

        SidebarRobot
            .Verify.IsSidebarAvailable();
    }

    [Test, Order(7)]
    [Retry(3)]
    public void RecentIsAddedToListInTray()
    {
        PopulateRecentsList();

        HomeRobot.MinimizeClientViaMinimizeButton();

        using (TrayApp)
        {
            SidebarRobot
                .Verify.HasNoRecentsLabel()
                       .IsConnectionOptionDisplayed(COUNTRY_NAME)
                       .IsConnectionOptionDisplayed(SECURE_CORE_COUNTRY_NAME)
                       .IsConnectionOptionDisplayed(PROFILE_NAME)
                       .IsRecentsCountDisplayed(4);
        }
    }

    [Test, Order(8)]
    [Retry(3)]
    public void RemoveRecentFromListInTray()
    {
        using (TrayApp)
        {
            SidebarRobot
                .Verify.IsConnectionOptionDisplayed(COUNTRY_NAME)
                .ExpandSecondaryActionsForRecents(COUNTRY_NAME)
                .RemoveRecent()
                .Verify.IsConnectionOptionMissing(COUNTRY_NAME);
            TrayRobot
                .DoubleClickTrayApp();
        }

        SidebarRobot
           .NavigateToRecents()
           .Verify.IsConnectionOptionMissing(COUNTRY_NAME);

        HomeRobot.MinimizeClientViaMinimizeButton();
    }

    [Test, Order(9)]
    [Retry(3)]
    public void PinRecentFromListInTray()
    {
        using (TrayApp)
        {
            SidebarRobot
                .Verify.IsConnectionOptionDisplayed(SECURE_CORE_COUNTRY_NAME)
                .ExpandSecondaryActionsForRecents(SECURE_CORE_COUNTRY_NAME)
                .PinRecent()
                .Verify.IsPinnedCountDisplayed(1);
            TrayRobot
                .DoubleClickTrayApp();
        }

        SidebarRobot
            .NavigateToRecents()
            .Verify.IsPinnedCountDisplayed(1);

        HomeRobot.MinimizeClientViaMinimizeButton();
    }

    [Test, Order(10)]
    [Retry(3)]
    public void UnpinRecentFromListInTray()
    {
        using (TrayApp)
        {
            SidebarRobot
                .Verify.IsConnectionOptionDisplayed(SECURE_CORE_COUNTRY_NAME)
                .ExpandSecondaryActionsForRecents(SECURE_CORE_COUNTRY_NAME)
                .UnpinRecent()
                .Verify.IsPinnedCountMissing();
            TrayRobot
                .DoubleClickTrayApp();
        }

        SidebarRobot
            .NavigateToRecents()
            .Verify.IsPinnedCountMissing();
    }

    [Test, Order(11)]
    [Retry(3)]
    public void ChangeServerFromTray()
    {
        QuickLogout();
        CommonUiFlows.FullLogin(TestUserData.FreeUser);
        HomeRobot.MinimizeClientViaMinimizeButton();

        using (TrayApp)
        {
            HomeRobot
                .ConnectViaConnectionCard()
                .Verify.IsConnected()
                .ChangeServer()
                .Verify.IsConnected()
                       .IsChangeServerLocked()
                       .IsNotTheCountryWantedBannerDisplayed()
                .ClickLockedChangedServer()
                .Verify.IsConnected()
                       .IsUnlimitedServersChangesUpsellDisplayed();
        }
    }

    [Test, Order(12)]
    [Retry(3)]
    public void CheckTrayOnLogin()
    {
        QuickLogout();

        using (TrayApp)
        {
            TrayRobot
                .Verify.IsTrayIconDisplayed()
                .ClickTaskbar()
                .Verify.IsLoginWindowFocused(false);

            OpenTrayApp();

            TrayRobot
                .Verify.IsLoginWindowFocused(true);
        }
    }

    [Test, Order(13)]
    [Retry(3)]
    public void ExitAppFromTray()
    {
        try
        {
            QuickLogout();
        }
        catch { }

        CommonUiFlows.FullLogin(TestUserData.PlusUser);
        HomeRobot.MinimizeClientViaMinimizeButton();

        using (TrayApp)
        {
            TrayRobot
                .ClickExitAppButton();
        }

        // give it time to exit
        Thread.Sleep(TestConstants.FiveSecondsTimeout);

        CommonAssertions.VerifyAppIsNotRunning();
    }

    private void QuickLogout()
    {
        using (TrayApp)
        {
            TrayRobot
                .DoubleClickTrayApp();
        }

        CommonUiFlows.Logout();
    }

    private void ToggleKillSwitch(bool shouldBeEnabled)
    {
        SettingRobot
            .OpenSettings()
            .OpenKillSwitchSettings();

        if (shouldBeEnabled)
        {
            SettingRobot
                .EnableKillSwitchToggle()
                .SelectKillSwitchMode(KillSwitchMode.Advanced);
        }
        else
        {
            SettingRobot
                .DisableKillSwitchToggle();
        }

        SettingRobot
            .ApplySettings()
            .CloseSettings();
    }

    private void PopulateRecentsList()
    {
        using (TrayApp)
        {
            TrayRobot
                .DoubleClickTrayApp();
        }

        SidebarRobot
            .NavigateToAllCountriesTab()
            .ConnectToCountry(COUNTRY_NAME);
        HomeRobot
            .Verify.IsConnected();

        SidebarRobot
            .NavigateToSecureCoreCountriesTab()
            .ExpandCities(SECURE_CORE_COUNTRY_NAME)
            .ConnectViaSecureCore(SECURE_CORE_COUNTRY_NAME, VIA_COUNTRY_ICELAND);
        HomeRobot
            .Verify.IsConnected();

        SidebarRobot
            .NavigateToProfiles()
            .ConnectToProfile(PROFILE_NAME);
        HomeRobot
            .Verify.IsConnected();

        HomeRobot
            .Disconnect()
            .Verify.IsDisconnected();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        try
        {
            TrayRobot.DoubleClickTrayApp();
        }
        catch (TimeoutException)
        {
            //Ignore
        }
        catch (NullReferenceException)
        {
            //Ignore
        }
        catch { }
        Cleanup();
    }
}