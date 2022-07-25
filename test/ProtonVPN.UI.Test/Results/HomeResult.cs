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

using System.Net;
using System.Net.Sockets;
using FlaUI.Core.AutomationElements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.UI.Test.TestsHelper;
using ProtonVPN.UI.Test.Windows;

namespace ProtonVPN.UI.Test.Results
{
    public class HomeResult : UIActions
    {
        private Label IpAddressLabel => ElementByAutomationId("IPAddressTextBlock").AsLabel();

        public HomeResult CheckIfNetshieldIsDisabled() => CheckIfDisplayedByClassName("Shield");

        public HomeResult CheckIfDnsIsResolved()
        {
            Assert.IsTrue(IsConnectedToInternet(), "User was not connected to internet.");
            return this;
        }

        public HomeResult KillClientAndCheckIfConnectionIsKept()
        {
            string ipAddress = IpAddressLabel.Text;
            KillAndRestartProtonVpnClient();
            new HomeWindow().WaitUntilConnected();
            Assert.IsTrue(ipAddress == IpAddressLabel.Text);
            return new HomeResult();
        }

        private static bool IsConnectedToInternet()
        {
            bool isConnected = true;
            try
            {
                Dns.GetHostEntry("www.google.com");
            }
            catch (SocketException)
            {
                isConnected = false;
            }
            return isConnected;
        }

        private void KillAndRestartProtonVpnClient()
        {
            KillProtonVpnProcess();
            LaunchApp();
            WaitUntilElementExistsByAutomationId("MenuHamburgerButton", TestConstants.LongTimeout);
        }

        public HomeResult CheckIfLoggedIn()
        {
            WaitUntilDisplayedByAutomationId("MenuHamburgerButton", TestConstants.MediumTimeout);
            return this;
        }

        public HomeResult VerifyLoggedInAsTextIs(string objectName)
        {
            WaitUntilElementExistsByName(objectName, TestConstants.MediumTimeout);
            return this;
        }

        public HomeResult CheckIfConnectButtonIsNotDisplayed()
        {
            CheckIfDoesNotExistsByName("CONNECT");
            return this;
        }

        public HomeResult CheckIfUpgradeRequiredModalIsShown()
        {
            WaitUntilElementExistsByName("Upgrade", TestConstants.ShortTimeout);
            CheckIfDisplayedByClassName("PlusCountries");
            return this;
        }

        public HomeResult CheckIfUpgradeRequiredModalIsShownSecureCore()
        {
            WaitUntilElementExistsByName("Upgrade", TestConstants.ShortTimeout);
            CheckIfDisplayedByClassName("SecureCore");
            return this;
        }

        public HomeResult CheckIfSidebarModeIsEnabled()
        {
            CheckIfNotDisplayedByAutomationId("Logo");
            return this;
        }
    }
}
