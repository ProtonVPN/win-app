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
using ProtonVPN.UI.Tests.Enums.Locations;
using static ProtonVPN.UI.Tests.Robots.TrayRobot;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("4")]
public class TrayTests : BaseTest
{
    private const Country COUNTRY_NAME = Country.Belgium;
    private const Country SECURE_CORE_COUNTRY_NAME = Country.Australia;
    private const Country VIA_COUNTRY_ICELAND = Country.Iceland;

    private const DefaultProfile PROFILE_NAME = DefaultProfile.StreamingUS;

    private static readonly string _randomCountry = LanguageHelper.GetTranslatedString("Country_Random");

    [OneTimeSetUp]
    public void SetUp()
    {
        LaunchClient();
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
        HomeRobot.MinimizeClientViaMinimizeButton();
        DesktopRobot.Verify.IsTrayIconDisplayed();
    }

    [Test, Order(0)]
    [Property("TestCaseId", "602473")]
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
    [Property("TestCaseId", "609946")]
    [Retry(3)]
    public void ChangeDefaultConnectionFromTray()
    {
        using (TrayApp)
        {
            HomeRobot
                .Verify.AssertAllVpnConnectionOptions()
                .SelectDefaultConnectionOption(VpnConnectionOption.Random)
                .Verify.ConnectionCardTitleEquals(_randomCountry);
        }
    }

    [Test, Order(2)]
    [Property("TestCaseId", "602461")]
    [Retry(3)]
    public void ConnectToAServerFromTray()
    {
        using (TrayApp)
        {
            SidebarRobot
                .Verify.IsNoRecentsLabelDisplayed();
            HomeRobot
                .Verify.ConnectionCardTitleEquals(_randomCountry)
                       .IsDisconnected()
                .ConnectViaConnectionCard()
                .Verify.IsConnecting()
                       .IsConnected()
                       .ConnectionCardTitleEquals(_randomCountry);
        }

        //TODO: assert padlock color - handle in the future
    }

    [Test, Order(3)]
    [Property("TestCaseId", "602462")]
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
    [Property("TestCaseId", "602463")]
    public void ConnectingErrorsInTray()
    {
        try
        {
            ScriptHelper.ConnectToWireGuard();

            using (TrayApp)
            {
                HomeRobot
                    .ConnectViaConnectionCard()
                    .Verify.IsWireGuardErrorDisplayed()
                           .CloseConnectionError();
                CommonUiFlows.EnsureUserIsDisconnected(shouldCancelConnection: true);
                //TODO: assert icon color - handle in the future
            }
        }
        finally
        {
            ScriptHelper.DisconnectFromWireGuard();
        }
    }

    [Test, Order(5)]
    [Property("TestCaseId", "607014")]
    [Retry(3)]
    public void WarningsInTray()
    {
        using (TrayApp)
        {
            TrayRobot
                .DoubleClickTrayApp();
        }

        try
        {
            HomeRobot.CloseConnectionError();
            Thread.Sleep(TestConstants.TwoSecondsTimeout);
        }
        catch { }

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
    [Property("TestCaseId", "602464")]
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
    [Property("TestCaseId", "602469")]
    [Retry(3)]
    public void RecentIsAddedToListInTray()
    {
        PopulateRecentsList();

        HomeRobot.MinimizeClientViaMinimizeButton();

        using (TrayApp)
        {
            SidebarRobot
                .Verify.HasNoRecentsLabel()
                       .IsConnectionOptionDisplayed(COUNTRY_NAME.GetName())
                       .IsConnectionOptionDisplayed(SECURE_CORE_COUNTRY_NAME.GetName())
                       .IsConnectionOptionDisplayed(PROFILE_NAME.GetEnumValue())
                       .IsRecentsCountDisplayed(4);
        }
    }

    [Test, Order(8)]
    [Property("TestCaseId", "602470")]
    [Retry(3)]
    public void RemoveRecentFromListInTray()
    {
        using (TrayApp)
        {
            SidebarRobot
                .Verify.IsConnectionOptionDisplayed(COUNTRY_NAME.GetName())
                .ExpandSecondaryActionsForRecents(COUNTRY_NAME.GetName())
                .RemoveRecent()
                .Verify.IsConnectionOptionMissing(COUNTRY_NAME.GetName());
            TrayRobot
                .DoubleClickTrayApp();
        }

        SidebarRobot
           .NavigateToRecents()
           .Verify.IsConnectionOptionMissing(COUNTRY_NAME.GetName());

        HomeRobot.MinimizeClientViaMinimizeButton();
    }

    [Test, Order(9)]
    [Property("TestCaseId", "602471")]
    [Retry(3)]
    public void PinRecentFromListInTray()
    {
        using (TrayApp)
        {
            SidebarRobot
                .Verify.IsConnectionOptionDisplayed(SECURE_CORE_COUNTRY_NAME.GetName())
                .ExpandSecondaryActionsForRecents(SECURE_CORE_COUNTRY_NAME.GetName())
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
    [Property("TestCaseId", "602472")]
    [Retry(3)]
    public void UnpinRecentFromListInTray()
    {
        using (TrayApp)
        {
            SidebarRobot
                .Verify.IsConnectionOptionDisplayed(SECURE_CORE_COUNTRY_NAME.GetName())
                .ExpandSecondaryActionsForRecents(SECURE_CORE_COUNTRY_NAME.GetName())
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
    [Property("TestCaseId", "602466")]
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
    [Property("TestCaseId", "602460")]
    [Retry(3)]
    public void CheckTrayOnLogin()
    {
        QuickLogout();

        using (TrayApp)
        {
            DesktopRobot.Verify.IsTrayIconDisplayed();
            TrayRobot
                .ClickTaskbar()
                .Verify.IsLoginWindowFocused(false);

            OpenTrayApp();

            TrayRobot
                .Verify.IsLoginWindowFocused(true);
        }
    }

    [Test, Order(13)]
    [Property("TestCaseId", "602465")]
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
        VerifyIsConnectedThenDisconnect();

        SidebarRobot
            .NavigateToSecureCoreCountriesTab()
            .ExpandCities(SECURE_CORE_COUNTRY_NAME)
            .ConnectViaSecureCore(SECURE_CORE_COUNTRY_NAME, VIA_COUNTRY_ICELAND);
        VerifyIsConnectedThenDisconnect();

        SidebarRobot
            .NavigateToProfiles()
            .ConnectToProfile(PROFILE_NAME.GetEnumValue());
        VerifyIsConnectedThenDisconnect();
    }

    private void VerifyIsConnectedThenDisconnect()
    {
        HomeRobot
            .Verify.IsConnected()
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