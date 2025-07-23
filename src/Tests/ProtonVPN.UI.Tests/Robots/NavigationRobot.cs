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

using System;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.UiTools;

namespace ProtonVPN.UI.Tests.Robots;

public class NavigationRobot
{
    public Verifications Verify => new();

    protected Element LoginPage => Element.ByAutomationId("LoginPage");
    protected Element SignInPage => Element.ByAutomationId("SignInPage");
    protected Element LoadingPage => Element.ByAutomationId("LoadingPage");
    protected Element TwoFactorPage => Element.ByAutomationId("TwoFactorPage");
    protected Element MainPage => Element.ByAutomationId("MainPage");
    protected Element SettingsPage => Element.ByAutomationId("SettingsPage");
    protected Element SearchResultsPage => Element.ByAutomationId("SearchResultsPage");
    protected Element KillSwitchPage => Element.ByAutomationId("KillSwitchPage");
    protected Element NetShieldPage => Element.ByAutomationId("NetShieldPage");
    protected Element PortForwardingPage => Element.ByAutomationId("PortForwardingPage");
    protected Element SplitTunnelingPage => Element.ByAutomationId("SplitTunnelingPage");
    protected Element ConnectionsPage => Element.ByAutomationId("ConnectionsPage");
    protected Element CountriesPage => Element.ByAutomationId("CountriesPage");
    protected Element GatewaysPage => Element.ByAutomationId("GatewaysPage");
    protected Element RecentsPage => Element.ByAutomationId("RecentsPage");
    protected Element ProfilesPage => Element.ByAutomationId("ProfilesPage");
    protected Element ProfilePage => Element.ByAutomationId("ProfilePage");
    protected Element CommonSettingsPage => Element.ByAutomationId("CommonSettingsPage");
    protected Element AdvancedSettingsPage => Element.ByAutomationId("AdvancedSettingsPage");
    protected Element LocationDetailsPage => Element.ByAutomationId("LocationDetailsPage");
    protected Element ConnectionDetailsPage => Element.ByAutomationId("ConnectionDetailsPage");

    public class Verifications : NavigationRobot
    {
        private Verifications IsOnPage(Element page, TimeSpan? timeout = null)
        {
            page.WaitUntilDisplayed(timeout);
            return this;
        }

        public Verifications IsOnLoginPage() => IsOnPage(LoginPage);

        public Verifications IsOnSignInPage() => IsOnPage(SignInPage);

        public Verifications IsOnTwoFactorPage() => IsOnPage(TwoFactorPage);

        public Verifications IsOnLoadingPage() => IsOnPage(LoadingPage);

        public Verifications IsOnMainPage() => IsOnPage(MainPage, TestConstants.TwoMinutesTimeout);

        public Verifications IsOnHomePage()
        {
            SettingsPage.DoesNotExist();
            return this;
        }

        public Verifications IsOnSettingsPage() => IsOnPage(SettingsPage);

        public Verifications IsOnConnectionsPage() => IsOnPage(ConnectionsPage);

        public Verifications IsOnRecentsPage() => IsOnPage(RecentsPage);

        public Verifications IsOnCountriesPage() => IsOnPage(CountriesPage);

        public Verifications IsOnGatewaysPage() => IsOnPage(GatewaysPage);

        public Verifications IsOnProfilesPage() => IsOnPage(ProfilesPage);

        public Verifications IsOnProfilePage() => IsOnPage(ProfilePage);

        public Verifications IsOnLocationDetailsPage() => IsOnPage(LocationDetailsPage);

        public Verifications IsOnConnectionDetailsPage() => IsOnPage(ConnectionDetailsPage);

        public Verifications IsOnKillSwitchPage() => IsOnPage(KillSwitchPage);
    }
}