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
using System.Threading;
using System.Threading.Tasks;
using FlaUI.Core.Tools;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using OpenQA.Selenium.Remote;
using ProtonVPN.UI.Tests.Robots;

namespace ProtonVPN.UI.Tests.TestsHelper;

public class NetworkUtils
{
    [DllImport("dnsapi.dll", EntryPoint = "DnsFlushResolverCache")]
    public static extern uint DnsFlushResolverCache();
    private static readonly HttpClient _httpClient = new();

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

    public static bool IsInternetAvailable()
    {
        Thread.Sleep(TestConstants.FiveSecondsTimeout);
        JObject connectionData = GetConnectionDataAsync().GetAwaiter().GetResult();
        return connectionData?["status"]?.ToString() == "success";
    }

    public static string GetIpAddressWithRetry()
    {
        RetryResult<string> retry = Retry.WhileEmpty(
            () => {
                FlushDns();
                return GetIpAddressAsync().Result; },
            TestConstants.ThirtySecondsTimeout, TestConstants.ApiRetryInterval, ignoreException: true);
        return retry.Result ?? throw new HttpRequestException($"Failed to get IP Address. \n {retry.LastException.Message} \n {retry.LastException.StackTrace}");
    }

    public static string GetCountryNameWithRetry()
    {
        RetryResult<string> retry = Retry.WhileEmpty(
            () => {
                FlushDns();
                return GetCountryNameAsync().Result;
            },
            TestConstants.ThirtySecondsTimeout, TestConstants.ApiRetryInterval, ignoreException: true);
        return retry.Result ?? throw new HttpRequestException($"Failed to get country name. \n {retry.LastException.Message} \n {retry.LastException.StackTrace}");
    }

    public static void VerifyUserIsConnectedToExpectedCountry(string countryNameToCompare)
    {
        string ip = GetIpAddressWithRetry();
        string countryName = GetCountryNameWithRetry();
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
               FlushDns();
               ipAddressFomAPI = GetIpAddressWithRetry();
               return ipAddressFomAPI.Equals(ipAddressToCompare);
           },
           TestConstants.ThirtySecondsTimeout, TestConstants.ApiRetryInterval);

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
               FlushDns();
               ipAddressFomAPI = GetIpAddressWithRetry();
               return ipAddressFomAPI.Equals(ipAddressToCompare);
           },
           TestConstants.ThirtySecondsTimeout, TestConstants.ApiRetryInterval);

        if (!retry.Success)
        {
            Assert.Fail($"API IP Address should match provided IP address.\n" +
                $"API returned IP address: {ipAddressFomAPI}.\n" +
                $"IP to compare: {ipAddressToCompare}");
        }
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

    private static async Task<string> GetCountryNameAsync()
    {
        JObject response = await GetConnectionDataAsync();
        return response["country"].ToString();
    }

    private static async Task<string> GetIpAddressAsync()
    {
        JObject response = await GetConnectionDataAsync();
        return response["query"].ToString();
    }

    private static async Task<JObject> GetConnectionDataAsync()
    {
        string endpoint = "http://ip-api.com/json/";
        // Make sure that fresh socket is created when requesting connection data
        using (HttpClient client = new HttpClient())
        {
            try
            {
                string response = await client.GetStringAsync(endpoint);
                JObject json = JObject.Parse(response);
                return json;
            }
            catch (HttpRequestException)
            {
                // Return null if API call fails due to networking issues
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