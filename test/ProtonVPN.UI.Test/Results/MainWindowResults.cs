/*
 * Copyright (c) 2020 Proton Technologies AG
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

using System.Globalization;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.UI.Test.ApiClient;
using ProtonVPN.UI.Test.TestsHelper;

namespace ProtonVPN.UI.Test.Results
{
    public class MainWindowResults : UIActions
    {
        public MainWindowResults VerifyUserIsLoggedIn()
        {
            CheckIfObjectWithIdIsDisplayed("MenuHamburgerButton", "User was not logged in.");
            return this;
        }

        public async Task<MainWindowResults> CheckIfCorrectIPAddressIsShownAsync()
        {
            var api = new Api(TestUserData.GetPlusUser().Username, TestUserData.GetPlusUser().Password);
            var ipAddress = await api.GetIpAddress();
            var textBlockIpAddress = Session.FindElementByAccessibilityId("IPAddressTextBlock").Text.RemoveExtraText();
            Assert.IsTrue(ipAddress.Equals(textBlockIpAddress), "Incorrect IP address is displayed.");
            return this;
        }
        
        public async Task<MainWindowResults> CheckIfCorrectCountryIsShownAsync()
        {
            var api = new Api(TestUserData.GetPlusUser().Username, TestUserData.GetPlusUser().Password);
            var country = await api.GetCountry();
            var region = new RegionInfo(country);
            var dashboardRegionName = Session.FindElementByAccessibilityId("EntryCountryAndServer").Text;
            dashboardRegionName = dashboardRegionName.Split('»')[0].Replace(" ", "");
            Assert.IsTrue(dashboardRegionName.Contains(region.DisplayName.Replace(" ", "")), "Incorrect country name is displayed.");
            return this;
        }

        public MainWindowResults CheckIfConnected()
        {
            CheckIfElementWithAutomationIdTextMatches("ConnectionState", "CONNECTED", "User did not connect to a VPN server");
            return this;
        }

        public MainWindowResults CheckIfDisconnected()
        {
            CheckIfElementWithAutomationIdTextMatches("ConnectionState", "DISCONNECTED", "User was not disconnected from VPN server");
            return this;
        }

        public MainWindowResults CheckIfSidebarModeIsEnabled()
        {
            CheckIfObjectWithIdIsDisplayed("MapModeButton", "Failed to enable sidebar mode");
            return this;
        }
    }
}
