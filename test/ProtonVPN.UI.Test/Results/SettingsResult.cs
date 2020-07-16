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

using System.Net.NetworkInformation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.UI.Test.TestsHelper;

namespace ProtonVPN.UI.Test.Results
{
    public class SettingsResult : UIActions
    {
        public SettingsResult CheckIfDnsAddressMatches(string dnsAddress)
        {
            var adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var adapter in adapters)
            {
                var adapterProperties = adapter.GetIPProperties();
                var dnsServers = adapterProperties.DnsAddresses;
                if (dnsServers.Count > 0)
                {
                    if (adapter.Description.Contains("ProtonVPN"))
                    {
                        foreach (var dns in dnsServers)
                        {
                            Assert.AreEqual(dnsAddress, dns.ToString(), "Desired dns address " + dnsAddress + " does not match Windows dns address " + dns.ToString());
                        }
                    }
                }
            }

            return this;
        }

        public SettingsResult VerifySettingsAreDisplayed()
        {
            CheckIfObjectWithNameIsDisplayed("Start Minimized", "'Start minimized' option is not displayed");
            CheckIfObjectWithNameIsDisplayed("Start with Windows", "'Start with windows' option is not displayed");
            CheckIfObjectWithNameIsDisplayed("Show Notifications", "'Show Notifications' option is not displayed");
            CheckIfObjectWithNameIsDisplayed("Early Access", "'Early Access' option is not displayed");
            return this;
        }

        public SettingsResult CheckIfCustomDnsAddressWasNotAdded()
        {
            CheckIfObjectWithAutomationIdDoesNotExist("DeleteButton", "Expected dns address not to be added.");
            return this;
        }
    }
}
