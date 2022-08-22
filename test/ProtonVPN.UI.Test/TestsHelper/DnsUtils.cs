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
using System.Runtime.InteropServices;
using FlaUI.Core.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProtonVPN.UI.Test.TestsHelper
{
    public class DnsUtils
    {
        [DllImport("dnsapi.dll", EntryPoint = "DnsFlushResolverCache")]
        public static extern uint DnsFlushResolverCache();

        public static string GetDnsAddress(string adapterName)
        {
            string dnsAddress = null;
            RetryResult<string> retry = Retry.WhileNull(
                () => {
                    dnsAddress = GetDnsAddressForAdapter(adapterName);
                    return dnsAddress;
                },
                TestConstants.ShortTimeout, TestConstants.RetryInterval);

            if (!retry.Success)
            {
                Assert.Fail($"Failed to get DNS address in {TestConstants.ShortTimeout}");
            }
            return dnsAddress;
        }

        private static string GetDnsAddressForAdapter(string adapterName)
        {
            string dnsAddress = null;
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in adapters)
            {
                IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                IPAddressCollection dnsServers = adapterProperties.DnsAddresses;
                if (adapter.Description.Contains(adapterName))
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
