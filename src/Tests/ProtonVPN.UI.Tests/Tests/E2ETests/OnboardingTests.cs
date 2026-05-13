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
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("4")]
public class OnboardingTests : BaseTest
{
    private const string EXCLUDED_LOCATIONS_TIP_PROMPT = "Avoid unwanted locations";
    private const string EXCLUDED_LOCATIONS_TIP_ACTION = "Exclude locations";
    private const string EXCLUDED_LOCATIONS_TIP_CANCEL = "Maybe later";

    private const string EXCLUDED_LOCATIONS_DISCOVERY_PROMPT = "Not the country you wanted?";
    private const string EXCLUDED_LOCATIONS_DISCOVERY_ACTION = "Exclude locations";
    private const string EXCLUDED_LOCATIONS_DISCOVERY_CANCEL = "Skip";

    private const string P2P_INFO_BANNER_DESCRIPTION = "Download files over P2P";
    private const string SECURE_CORE_INFO_BANNER_DESCRIPTION = "Add another layer of encryption";
    private const string TOR_INFO_BANNER_DESCRIPTION = "Use the Tor network";

    [OneTimeSetUp]
    public void SetUp()
    {
        LaunchApp(skipOnboarding: false);
    }

    [Test, Order(0)]
    public void ConfirmWelcomeModalIsDisplayed()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);

        HomeRobot
            .Verify.IsWelcomeModalDisplayed()
            .DismissWelcomeModal();
    }

    [Test, Order(1)]
    public void ConfirmInfoBannersAreDisplayed()
    {
        NavigationRobot
            .Verify.IsOnConnectionsPage()
                   .IsOnCountriesPage();

        SidebarRobot
            .NavigateToP2PCountriesTab()
            .Verify.IsCountryInfoBannerDisplayed(P2P_INFO_BANNER_DESCRIPTION)
            .NavigateToSecureCoreCountriesTab()
            .Verify.IsCountryInfoBannerDisplayed(SECURE_CORE_INFO_BANNER_DESCRIPTION)
            .NavigateToTorCountriesTab()
            .Verify.IsCountryInfoBannerDisplayed(TOR_INFO_BANNER_DESCRIPTION);
    }

    [Test, Order(2)]
    public void ConfirmExcludingLocationsTipsAreDisplayed()
    {
        CommonUiFlows.Logout();
        CommonUiFlows.FullLogin(TestUserData.PlusUser);

        TeachingTipRobot
            .Verify.IsTeachingTipDisplayed()
                   .TeachingTipTextContains(EXCLUDED_LOCATIONS_TIP_PROMPT)
                   .TeachingTipButtonEquals(
                        primary: EXCLUDED_LOCATIONS_TIP_ACTION,
                        close: EXCLUDED_LOCATIONS_TIP_CANCEL)
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
                   .OverlayTextContains(EXCLUDED_LOCATIONS_DISCOVERY_PROMPT)
                   .OverlayButtonsEquals(
                        primary: EXCLUDED_LOCATIONS_DISCOVERY_ACTION,
                        cancel: EXCLUDED_LOCATIONS_DISCOVERY_CANCEL)
            .PrimaryAction();

        NavigationRobot
            .Verify.IsOnSettingsPage()
                   .IsOnConnectionPreferencesPage();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        Cleanup();
    }
}