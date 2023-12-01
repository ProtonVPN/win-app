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

using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using FlaUI.Core.AutomationElements;
using NUnit.Framework;
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
            Assert.That(TryToResolveDns(url), Is.True, $"Dns was not resolved for {url}.");
            return this;
        }

        public HomeResult CheckIfDnsIsNotResolved(string url)
        {
            Assert.That(TryToResolveDns(url), Is.False, $"DNS was resolved for {url}");
            return this;
        }

        public HomeResult KillClientAndCheckIfConnectionIsKept()
        {
            string ipAddress = IpAddressLabelText;
            KillAndRestartProtonVpnClient();
            new HomeWindow().WaitUntilConnected();
            Assert.That(ipAddress == IpAddressLabelText, Is.True, "IP Address: " + IpAddressLabelText + " does not match previous " + ipAddress + " address");
            return this;
        }

        public HomeResult CheckIfLocalNetworkingWorks(string localIpAddress)
        {
            PingReply reply = new Ping().Send(localIpAddress);
            Assert.That(reply.Status == IPStatus.Success, Is.True);
            return this;
        }

        public async Task CheckIfCorrectIpAddressIsDisplayed()
        {
            ConnectionDataHelper client = new ConnectionDataHelper();
            string currentIpAddress = await client.GetIpAddress();
            Assert.That(currentIpAddress == IpAddressLabelText, Is.True, $"IP Address: {IpAddressLabelText} does not match expected {currentIpAddress} address from API");
        }

        private static bool TryToResolveDns(string url)
        {
            bool isConnected = true;
            try
            {
                System.Net.Dns.GetHostEntry(url);
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
            WaitUntilElementExistsByAutomationId("MenuHamburgerButton", TestData.LongTimeout);
        }

        public HomeResult CheckIfLoggedIn()
        {
            WaitUntilDisplayedByAutomationId("MenuHamburgerButton", TestData.MediumTimeout);
            return this;
        }

        public HomeResult CheckIfUsernameIsDisplayedInAccount(string objectName)
        {
            WaitUntilElementExistsByName(objectName, TestData.MediumTimeout);
            return this;
        }

        public HomeResult CheckIfCorrectPlanIsDisplayed(string planName)
        {
            WaitUntilElementExistsByName(planName, TestData.ShortTimeout);
            return this;
        }

        public HomeResult CheckIfConnectButtonIsNotDisplayed()
        {
            CheckIfDoesNotExistByName("Connect");
            return this;
        }

        public HomeResult CheckIfUpgradeRequiredModalIsShown()
        {
            WaitUntilElementExistsByName("Upgrade", TestData.VeryShortTimeout);
            CheckIfDisplayedByAutomationId("UpsellModalTitle");
            return this;
        }

        public HomeResult CheckIfPortForwardingQuickSettingIsNotVisible()
        {
            CheckIfDoesNotExistByAutomationId("PortForwardingButton");
            return this;
        }

        public HomeResult CheckIfPortForwardingQuickSettingIsVisible()
        {
            WaitUntilDisplayedByAutomationId("PortForwardingButton", TestData.VeryShortTimeout);
            return this;
        }

        public HomeResult CertificateErrorIsDisplayed()
        {
            WaitUntilElementExistsByName("Proton VPN failed to fetch authentication certificate due to network issue.", TestData.ShortTimeout);
            return this;
        }

        public HomeResult DowngradeModalIsDisplayed()
        {
            WaitUntilElementExistsByName("Your VPN subscription plan has expired", TestData.ShortTimeout);
            WaitUntilElementExistsByName("No Thanks", TestData.ShortTimeout);
            WaitUntilElementExistsByName("Upgrade Again", TestData.ShortTimeout);
            return this;
        }
    }
}
