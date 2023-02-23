/*
 * Copyright (c) 2023 Proton AG
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
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using FlaUI.Core.AutomationElements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.UI.Tests.ApiClient;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.Windows;

namespace ProtonVPN.UI.Tests.Results
{
    public class HomeResult : UIActions
    {
        private Label IpAddressLabel => ElementByAutomationId("IPAddressTextBlock").FindFirstChild().AsLabel();
        private string IpAddressLabelText => IpAddressLabel.Text.Replace("IP: ", "");
        public HomeResult CheckIfNetshieldIsDisabled() => CheckIfDisplayedByClassName("Shield");

        public HomeResult CheckIfDnsIsResolved(string url)
        {
            Assert.IsTrue(TryToResolveDns(url), $"Dns was not resolved for {url}.");
            return this;
        }

        public HomeResult CheckIfDnsIsNotResolved(string url)
        {
            Assert.IsFalse(TryToResolveDns(url), $"DNS was resolved for {url}");
            return this;
        }

        public HomeResult KillClientAndCheckIfConnectionIsKept()
        {
            string ipAddress = IpAddressLabelText;
            KillAndRestartProtonVpnClient();
            new HomeWindow().WaitUntilConnected();
            Assert.IsTrue(ipAddress == IpAddressLabelText, "IP Address: " + IpAddressLabelText + " does not match previous " + ipAddress + " address");
            return this;
        }

        public HomeResult CheckIfLocalNetworkingWorks(string localIpAddress)
        {
            PingReply reply = new Ping().Send(localIpAddress);
            Assert.IsTrue(reply.Status == IPStatus.Success);
            return this;
        }

        public async Task CheckIfCorrectIpAddressIsDisplayed()
        {
            TestsApiClient client = new TestsApiClient("https://api.ipify.org/");
            string currentIpAddress = await client.GetIpAddress();
            Assert.IsTrue(currentIpAddress == IpAddressLabelText, $"IP Address: {IpAddressLabelText} does not match expected {currentIpAddress} address from API");
        }

        private static bool TryToResolveDns(string url)
        {
            bool isConnected = true;
            try
            {
                Dns.GetHostEntry(url);
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
            CheckIfDoesNotExistsByName("Connect");
            return this;
        }

        public HomeResult CheckIfUpgradeRequiredModalIsShown()
        {
            WaitUntilElementExistsByName("Upgrade", TestConstants.VeryShortTimeout);
            CheckIfDisplayedByClassName("PlusCountries");
            return this;
        }

        public HomeResult CheckIfUpgradeRequiredModalIsShownSecureCore()
        {
            WaitUntilElementExistsByName("Upgrade", TestConstants.VeryShortTimeout);
            CheckIfDisplayedByClassName("SecureCore");
            return this;
        }

        public HomeResult CheckIfSidebarModeIsEnabled()
        {
            CheckIfNotDisplayedByAutomationId("Logo");
            return this;
        }

        public HomeResult CheckIfPortForwardingQuickSettingIsNotVisible()
        {
            CheckIfNotDisplayedByAutomationId("PortForwardingButton");
            return this;
        }

        public HomeResult CheckIfPortForwardingQuickSettingIsVisible()
        {
            WaitUntilDisplayedByAutomationId("PortForwardingButton", TestConstants.VeryShortTimeout);
            return this;
        }
    }
}
