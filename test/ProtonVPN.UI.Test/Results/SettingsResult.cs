/*
 * Copyright (c) 2022 Proton
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
using System.Threading;
using FlaUI.Core.AutomationElements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.UI.Test.TestsHelper;

namespace ProtonVPN.UI.Test.Results
{
    public class SettingsResult : UIActions
    {
        private CheckBox ModerateNatCheckBox => ElementByAutomationId("ModerateNatCheckbox").AsCheckBox();

        public SettingsResult CheckIfDnsAddressDoesNotMatch(string dnsAddress)
        {
            Assert.AreNotEqual(dnsAddress, GetDnsAddressForAdapter());
            return this;
        }

        public SettingsResult CheckIfSettingsAreDisplayed()
        {
            WaitUntilElementExistsByName("Start Minimized", TestConstants.ShortTimeout);
            WaitUntilElementExistsByName("Start on boot", TestConstants.ShortTimeout);
            WaitUntilElementExistsByName("Connect on app start", TestConstants.ShortTimeout);
            WaitUntilElementExistsByName("Show Notifications", TestConstants.ShortTimeout);
            WaitUntilElementExistsByName("Early Access", TestConstants.ShortTimeout);
            return this;
        }

        public SettingsResult CheckIfCustomDnsAddressWasNotAdded()
        {
            CheckIfDoesNotExistsByAutomationId("DeleteButton");
            return this;
        }

        public SettingsResult CheckIfDnsAddressMatches(string dnsAddress)
        {
            if(GetDnsAddressForAdapter() == null)
            {
                //Sometimes windows does not set DNS address fast enough, so some delay might be needed.
                Thread.Sleep(3000);
            }
            Assert.AreEqual(dnsAddress, GetDnsAddressForAdapter(), "Desired dns address " + dnsAddress + " does not match Windows dns address " + GetDnsAddressForAdapter());
            return this;
        }

        public SettingsResult CheckIfModerateNatIsEnabled()
        {
            Assert.IsTrue(ModerateNatCheckBox.IsChecked.Value, "Moderate NAT checkbox status is: " + ModerateNatCheckBox.IsChecked.Value);
            return this;
        }

        public SettingsResult CheckIfModerateNatIsDisabled()
        {
            Assert.IsFalse(ModerateNatCheckBox.IsChecked.Value, "Moderate NAT checkbox status is: " + ModerateNatCheckBox.IsChecked.Value);
            return this;
        }

        private string GetDnsAddressForAdapter()
        {
            string dnsAddress = null;
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in adapters)
            {
                IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                IPAddressCollection dnsServers = adapterProperties.DnsAddresses;
                if (adapter.Description.Contains("WireGuard Tunnel"))
                {
                    foreach (IPAddress dns in dnsServers)
                    {
                        dnsAddress = dns.ToString();
                    }
                }
            }
            return dnsAddress;
        }
    }
}
