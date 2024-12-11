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

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FlaUI.Core.Tools;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Robots;

namespace ProtonVPN.UI.Tests.TestsHelper;

public class NetworkUtils
{
    private static HttpClient _httpClient = new();

    [DllImport("dnsapi.dll", EntryPoint = "DnsFlushResolverCache")]
    public static extern uint DnsFlushResolverCache();

    public static string GetDnsAddress(string adapterName)
    {
        string dnsAddress = null;
        RetryResult<string> retry = Retry.WhileNull(
            () =>
            {
                dnsAddress = GetDnsAddressForAdapterByName(adapterName);
                return dnsAddress;
            },
            TestConstants.FiveSecondsTimeout, TestConstants.RetryInterval);

        if (!retry.Success)
        {
            dnsAddress = null;
        }
        return dnsAddress;
    }

    public static void FlushDns()
    {
        DnsFlushResolverCache();
    }

    public static void VerifyIfLocalNetworkingWorks()
    {
        PingReply reply = new Ping().Send(GetDefaultGatewayAddress().ToString());
        Assert.That(reply.Status == IPStatus.Success, Is.True);
    }

    public static string GetIpAddress(string endpoint = "https://api.ipify.org/")
    {
        RetryResult<string> retry = Retry.WhileEmpty(
            () => {
                return GetExternalIpAddressAsync(endpoint).Result; },
            TestConstants.ThirtySecondsTimeout, TestConstants.RetryInterval, ignoreException: true);
        return retry.Result ?? throw new HttpRequestException("Failed to get IP Address.");
    }

    public static string GetIpAddressBti()
    {
        string ipMeBtiUrl = Environment.GetEnvironmentVariable("IP_ENDPOINT_BTI");
        return GetIpAddress(ipMeBtiUrl);
    }

    private static IPAddress GetDefaultGatewayAddress()
    {
        return NetworkInterface
            .GetAllNetworkInterfaces()
            .Where(n => n.Name.EndsWith("Wi-Fi") || n.Name.EndsWith("Ethernet"))
            .SelectMany(n => n.GetIPProperties()?.GatewayAddresses)
            .Select(g => g?.Address)
            .FirstOrDefault(a => a != null);
    }

    private static async Task<string> GetExternalIpAddressAsync(string endpoint)
    {
        try
        {
            string externalIpString = await _httpClient.GetStringAsync(endpoint);
            string ipAddress = externalIpString
                .Replace("\\r\\n", "")
                .Replace("\\n", "")
                .Trim();

            return ipAddress;
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    private static string GetDnsAddressForAdapterByName(string adapterName)
    {
        string dnsAddress = null;
        NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
        foreach (NetworkInterface adapter in adapters)
        {
            IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
            IPAddressCollection dnsServers = adapterProperties.DnsAddresses;
            if (adapter.Name.Equals(adapterName))
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