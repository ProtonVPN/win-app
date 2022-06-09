/*
 * Copyright (c) 2022 Proton Technologies AG
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
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.UI.Test.ApiClient;
using ProtonVPN.UI.Test.TestsHelper;
using ProtonVPN.UI.Test.Windows;

namespace ProtonVPN.UI.Test.Results
{
    public class MainWindowResults : UIActions
    {
        private readonly MainWindow _mainWindow = new MainWindow();
        private readonly LoginWindow _loginWindow = new LoginWindow();
        private readonly CommonAPI _client = new CommonAPI("http://ipwhois.app");

        public MainWindowResults VerifyUserIsLoggedIn()
        {
            CheckIfObjectWithIdIsDisplayed("MenuHamburgerButton", "User was not logged in.");
            return this;
        }

        public async Task<MainWindowResults> CheckIfCorrectIPAddressIsShownAsync()
        {
            string ipAddress = null;
            for (int i = 0; i < 5; i++)
            {
                ipAddress = await _client.GetIpAddress();
                if (!string.IsNullOrWhiteSpace(ipAddress))
                {
                    break;
                }
                //After connecting there is a period with no connectivity, that is why we attempt to get IP address multiple times.
                Thread.Sleep(1000);
            }
            string textBlockIpAddress = Session.FindElementByAccessibilityId("IPAddressTextBlock").Text.RemoveExtraText();
            Assert.IsTrue(ipAddress.Equals(textBlockIpAddress), "Incorrect IP address is displayed. API returned IP: " + ipAddress + " App IP addres: " + textBlockIpAddress);
            return this;
        }

        public async Task<MainWindowResults> CheckIfIpAddressIsExcluded(string ipAddress)
        {
            string ipAddressAPI = await _client.GetIpAddress();
            Assert.IsTrue(ipAddress.Equals(ipAddressAPI), "IP address was not excluded. API returned IP: " + ipAddressAPI + " Home IP addres: " + ipAddress);
            return this;
        }
        
        public async Task<MainWindowResults> CheckIfCorrectCountryIsShownAsync()
        {
            string region = await _client.GetCountry();
            region = region.Replace(" ", "");
            string dashboardRegionName = Session.FindElementByAccessibilityId("EntryCountryAndServer").Text;
            dashboardRegionName = dashboardRegionName.Split('»')[0].Replace(" ", "");
            Assert.IsTrue(region.Contains(dashboardRegionName.Replace(" ", "")), "Incorrect country name is displayed. API returned country: " + region + " App Country: " +  dashboardRegionName);
            return this;
        }

        public MainWindowResults CheckIfConnected()
        {
            CheckIfElementWithAutomationIdTextMatches("ConnectionState", "CONNECTED", "User did not connect to a VPN server");
            return this;
        }

        public MainWindowResults CheckIfDisconnected()
        {
            WaitUntilTextMatches(Session.FindElementByAccessibilityId("ConnectionState"), "DISCONNECTED", 5);
            CheckIfElementWithAutomationIdTextMatches("ConnectionState", "DISCONNECTED", "User was not disconnected from VPN server");
            return this;
        }

        public MainWindowResults CheckIfSidebarModeIsEnabled()
        {
            CheckIfObjectIsNotDisplayed("Logo", "Failed to enable sidebar mode");
            return this;
        }

        public MainWindowResults CheckIfSameServerIsKeptAfterKillingApp()
        {
            string ipAddress = _mainWindow.GetTextBlockIpAddress();
            KillProtonVpnProcess();
            RefreshSession();
            _loginWindow.WaitUntilLoginIsFinished();
            RefreshSession();
            Assert.AreEqual(ipAddress, _mainWindow.GetTextBlockIpAddress(), "User was not connected to the same server.");
            return this;
        }

        public MainWindowResults CheckIfNetshieldIsDisabled()
        {
            CheckIfObjectWithClassNameIsDisplayed("Shield", "Netshield was not disabled.");
            return this;
        }

        public MainWindowResults CheckIfConnectButtonIsNotDisplayed()
        {
            Assert.IsFalse(CheckIfElementExistsByName("CONNECT"));
            return this;
        }

        public MainWindowResults CheckIfUpgradeRequiredModalIsShown(string serverName)
        {
            CheckIfObjectWithNameIsDisplayed("When you upgrade to Plus", "Free User is able to connect to " + serverName + " server");
            CheckIfObjectWithNameIsDisplayed("Upgrade", "Free User is able to connect to " + serverName + " server");
            return this;
        }
    }
}
