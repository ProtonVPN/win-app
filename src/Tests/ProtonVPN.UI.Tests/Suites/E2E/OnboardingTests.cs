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

using System.Threading;
using NUnit.Framework;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.TestsHelper.UiFlows;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("4")]
[Category("ARM")]
public class OnboardingTests : BaseTest
{
    private static readonly string _excludedLocationsTipPrompt = LanguageHelper.GetTranslatedString("ExcludedLocations_TeachingTip_Title");
    private static readonly string _excludedLocationsTipAction = LanguageHelper.GetTranslatedString("ExcludedLocations_TeachingTip_ActionButton");
    private static readonly string _excludedLocationsTipCancel = LanguageHelper.GetTranslatedString("ExcludedLocations_TeachingTip_CloseButton");

    private static readonly string _excludedLocationsDiscoveryPrompt = LanguageHelper.GetTranslatedString("ExcludedLocations_SmartDiscovery_Prompt_Title");
    private static readonly string _excludedLocationsDiscoveryAction = LanguageHelper.GetTranslatedString("ExcludedLocations_SmartDiscovery_Prompt_ExcludeLocations");
    private static readonly string _excludedLocationsDiscoveryCancel = LanguageHelper.GetTranslatedString("ExcludedLocations_SmartDiscovery_Prompt_Skip");

    private static readonly string _p2pInfoBannerDesription = LanguageHelper.GetTranslatedString("Countries_P2P_Description");
    private static readonly string _secureCoreInfoBannerDesription = LanguageHelper.GetTranslatedString("Countries_SecureCore_Description");
    private static readonly string _torInfoBannerDesription = LanguageHelper.GetTranslatedString("Countries_Tor_Description");

    [SetUp]
    public void SetUp()
    {
        LaunchClient(ClientLaunchParams.FreshStartWithOnboarding);
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test]
    [Property("TestCaseId", "867498")]
    public void ConfirmWelcomeModalIsDisplayed()
    {
        HomeRobot
            .Verify.IsWelcomeModalDisplayed()
            .DismissWelcomeModal();
    }

    [Test]
    [Property("TestCaseId", "867499")]
    public void ConfirmInfoBannersAreDisplayed()
    {
        HomeRobot.DismissWelcomeModal();

        NavigationRobot
            .Verify.IsOnConnectionsPage()
                   .IsOnCountriesPage();

        SidebarRobot
            .NavigateToP2PCountriesTab()
            .Verify.IsCountryInfoBannerDisplayed(_p2pInfoBannerDesription)
            .NavigateToSecureCoreCountriesTab()
            .Verify.IsCountryInfoBannerDisplayed(_secureCoreInfoBannerDesription)
            .NavigateToTorCountriesTab()
            .Verify.IsCountryInfoBannerDisplayed(_torInfoBannerDesription);
    }

    [Test]
    [Property("TestCaseId", "867755")]
    public void ConfirmExcludingLocationsTipsAreDisplayed()
    {
        HomeRobot.DismissWelcomeModal();

        CommonUiFlows.Logout();
        CommonUiFlows.FullLogin(TestUserData.PlusUser);

        try
        {
            Thread.Sleep(TestConstants.OneSecondTimeout);

            TeachingTipRobot
                .Verify.IsTeachingTipDisplayed()
                       .TeachingTipTextContains(_excludedLocationsTipPrompt)
                       .TeachingTipButtonEquals(
                            primary: _excludedLocationsTipAction,
                            close: _excludedLocationsTipCancel)
                .CloseAction();

            HomeRobot
                .Verify.IsDisconnected()
                .ConnectViaConnectionCard()
                .Verify.IsConnecting()
                       .IsConnected()
                .Disconnect()
                .Verify.IsDisconnected();

            ConfirmationRobot
                .Verify.IsOverlayDisplayed()
                       .OverlayTextContains(_excludedLocationsDiscoveryPrompt)
                       .OverlayButtonsEquals(
                            primary: _excludedLocationsDiscoveryAction,
                            cancel: _excludedLocationsDiscoveryCancel)
                .PrimaryAction();

            NavigationRobot
                .Verify.IsOnSettingsPage()
                       .IsOnConnectionPreferencesPage();
        }
        finally
        {
            Thread.Sleep(TestConstants.AnimationDelay);
            try
            {
                ConfirmationRobot
                    .Verify.IsOverlayDisplayed()
                    .CancelAction();
            }
            catch { }
        }
    }

    [TearDown]
    public void TearDown()
    {
        Cleanup();
    }
}