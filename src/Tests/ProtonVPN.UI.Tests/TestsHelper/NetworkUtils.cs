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
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Robots;

namespace ProtonVPN.UI.Tests.TestsHelper;

public class NetworkUtils
{
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

    public static string GetCountryName(string ip)
    {
        RetryResult<string> retry = Retry.WhileEmpty(
        () => {
                return GetCountryNameAsync(ip).Result;
            },
            TestConstants.ThirtySecondsTimeout, TestConstants.RetryInterval, ignoreException: true);
        return retry.Result ?? throw new HttpRequestException("Failed to get country name.");
    }

    public static void VerifyUserIsConnectedToExpectedCountry(string countryNameToCompare)
    {
        string ip = GetIpAddress();
        string countryName = GetCountryName(ip);
        Assert.That(countryName.Equals(countryNameToCompare), Is.True, $"User was connected to unexpected country." +
            $"\n API returned: {countryName}" +
            $"\n Expected result: {countryNameToCompare}");
    }

    public static void VerifyIpAddressDoesNotMatchWithRetry(string ipAddressToCompare)
    {
        string ipAddressFomAPI = null;
        RetryResult<bool> retry = Retry.WhileTrue(
           () =>
           {
               ipAddressFomAPI = GetIpAddress();
               return ipAddressFomAPI.Equals(ipAddressToCompare);
           },
           TestConstants.TenSecondsTimeout, TestConstants.ApiRetryInterval);

        if (!retry.Success)
        {
            Assert.Fail($"API IP Address should not match provided IP address.\n" +
                $"API returned IP address: {ipAddressFomAPI}.\n" +
                $"IP to compare: {ipAddressToCompare}");
        }
    }

    public static void VerifyIpAddressMatchesWithRetry(string ipAddressToCompare)
    {
        string ipAddressFomAPI = null; 
        RetryResult<bool> retry = Retry.WhileFalse(
           () =>
           {
               ipAddressFomAPI = GetIpAddress();
               return ipAddressFomAPI.Equals(ipAddressToCompare);
           },
           TestConstants.TenSecondsTimeout, TestConstants.ApiRetryInterval);

        if (!retry.Success)
        {
            Assert.Fail($"API IP Address should match provided IP address.\n" +
                $"API returned IP address: {ipAddressFomAPI}.\n" +
                $"IP to compare: {ipAddressToCompare}");
        }
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

    private static async Task<string> GetCountryNameAsync(string ip)
    {
        string url = $"http://www.geoplugin.net/json.gp?ip={ip}";
        // Make sure that fresh socket is created when getting geolocation.
        using (HttpClient client = new HttpClient())
        {
            try
            {
                string response = await client.GetStringAsync(url);
                JObject json = JObject.Parse(response);
                return json["geoplugin_countryName"]?.ToString();
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }
    }

    private static async Task<string> GetExternalIpAddressAsync(string endpoint)
    {
        // Make sure that fresh socket is created when verifying IP address
        using (HttpClient client = new HttpClient())
        {
            try
            {
                string externalIpString = await client.GetStringAsync(endpoint);
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