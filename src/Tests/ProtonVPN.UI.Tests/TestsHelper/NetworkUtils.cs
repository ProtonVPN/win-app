/*
 * Copyright (c) 2026 Proton AG
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
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using FlaUI.Core.Tools;
using NUnit.Framework;

namespace ProtonVPN.UI.Tests.TestsHelper;

public class NetworkUtils
{
    [DllImport("dnsapi.dll", EntryPoint = "DnsFlushResolverCache")]

    public static extern uint DnsFlushResolverCache();

    public static List<string> GetDnsAddresses(string adapterName)
    {
        RetryResult<List<string>> retry = Retry.WhileEmpty(
            () =>
            {
                return GetDnsAddressesForAdapterByName(adapterName);
            },
            TestConstants.FiveSecondsTimeout, TestConstants.RetryInterval);

        return retry.Result ?? [];
    }

    public static void FlushDns()
    {
        DnsFlushResolverCache();
    }

    public static void VerifyLocalNetworking(bool isLanEnabled)
    {
        IPAddress? ipAddress = GetDefaultGatewayAddress() ?? throw new Exception("Default gateway is null.");
        PingReply reply = new Ping().Send(ipAddress.ToString());
        Assert.That(reply.Status == IPStatus.Success, isLanEnabled ? Is.True : Is.False);
    }

    public static void AssertInternetAvailability(bool shouldBeAvailable)
    {
        bool isAvailable = IsInternetAvailable(shouldBeAvailable);
        if (shouldBeAvailable)
        {
            Assert.That(isAvailable, Is.True, "Expected internet to be available.");
        }
        else
        {
            Assert.That(isAvailable, Is.False, "Expected internet to not be available.");
        }
    }

    private static bool IsInternetAvailable(bool shouldBeAvailable)
    {
        Thread.Sleep(TestConstants.TenSecondsTimeout);
        JObject? connectionData = GetConnectionDataAsync(shouldBeAvailable).GetAwaiter().GetResult();
        return connectionData?["status"]?.ToString() == "success";
    }

    public static string GetIpAddressWithRetry()
    {
        RetryResult<string> retry = Retry.WhileEmpty(
            () =>
            {
                FlushDns();
                return GetIpAddressAsync().GetAwaiter().GetResult() ?? string.Empty;
            },
            TestConstants.ThirtySecondsTimeout, TestConstants.ApiRetryInterval, ignoreException: true);
        return retry.Result ?? throw new HttpRequestException($"Failed to get IP Address. \n {retry.LastException?.Message} \n {retry.LastException?.StackTrace}");
    }

    public static string GetCountryNameWithRetry()
    {
        RetryResult<string> retry = Retry.WhileEmpty(
            () =>
            {
                FlushDns();
                return GetCountryNameAsync().Result ?? string.Empty;
            },
            TestConstants.ThirtySecondsTimeout, TestConstants.ApiRetryInterval, ignoreException: true);
        return retry.Result ?? throw new HttpRequestException($"Failed to get country name. \n {retry.LastException?.Message} \n {retry.LastException?.StackTrace}");
    }

    public static void VerifyUserIsConnectedToExpectedCountry(string countryNameToCompare)
    {
        string countryName = GetCountryNameWithRetry();
        Assert.That(countryName.Equals(countryNameToCompare), Is.True, $"User was connected to unexpected country." +
            $"\n API returned: {countryName}" +
            $"\n Expected result: {countryNameToCompare}");
    }

    public static void VerifyIpAddressDoesNotMatchWithRetry(string? ipAddressToCompare)
    {
        string? ipAddressFomAPI = null;
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

    public static void VerifyIpAddressMatchesWithRetry(string? ipAddressToCompare)
    {
        string? ipAddressFomAPI = null;
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

    public static void AssertCorrectNetworkAdapter(string adapterName)
    {
        NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
        NetworkInterface? matchingAdapter = adapters.FirstOrDefault(a => a.Description.Contains(adapterName));

        Assert.That(matchingAdapter, Is.Not.Null, $"No network adapter found with description containing '{adapterName}'");
        Assert.That(matchingAdapter!.OperationalStatus, Is.EqualTo(OperationalStatus.Up), $"Adapter '{matchingAdapter.Description}' is not up");
    }

    private static IPAddress? GetDefaultGatewayAddress()
    {
        return NetworkInterface
            .GetAllNetworkInterfaces()
            .Where(n => n.Name.EndsWith("Wi-Fi") || n.Name.EndsWith("Ethernet"))
            .SelectMany(n => n.GetIPProperties().GatewayAddresses ?? Enumerable.Empty<GatewayIPAddressInformation>())
            .Select(g => g?.Address)
            .FirstOrDefault(a => a != null);
    }

    private static async Task<string?> GetCountryNameAsync()
    {
        JObject? response = await GetConnectionDataAsync();
        return response?["country"]?.ToString();
    }

    private static async Task<string?> GetIpAddressAsync()
    {
        JObject? response = await GetConnectionDataAsync();
        return response?["query"]?.ToString();
    }

    private static async Task<JObject?> GetConnectionDataAsync(bool errorIsNotExpected = true)
    {
        string endpoint = "http://ip-api.com/json/";
        // Make sure that fresh socket is created when requesting connection data
        using (HttpClient client = new())
        {
            try
            {
                string response = await client.GetStringAsync(endpoint);
                JObject json = JObject.Parse(response);
                return json;
            }
            catch (HttpRequestException e)
            {
                if (errorIsNotExpected)
                {
                    TestContext.WriteLine($"GetIpAddressWithRetry failed. Result: {e.Message}");
                }
                return null;
            }
        }
    }

    public static void AssertTorStatus(bool shouldBeAvailable, string? vpnIp = null)
    {
        string? ip = null;
        bool? isTor = null;

        RetryResult<string> retry = Retry.WhileEmpty(
            () =>
            {
                JObject? result = GetTorStatusAsync().GetAwaiter().GetResult();
                ip = result?["IP"]?.Value<string>();
                isTor = result?["IsTor"]?.Value<bool>();
                // Returning only the IP, since IP and IsTor are always returned together
                return ip ?? string.Empty;
            },
            TestConstants.ThirtySecondsTimeout, TestConstants.ApiRetryInterval);

        Assert.That(retry.Success, Is.True, "Failed to retrieve Tor status within timeout.");

        if (shouldBeAvailable)
        {
            Assert.That(isTor, Is.True);
            Assert.That(ip, Does.Not.Contain(vpnIp));
        }
        else
        {
            Assert.That(isTor, Is.False);
        }
    }

    private static async Task<JObject?> GetTorStatusAsync()
    {
        string endpoint = "https://check.torproject.org/api/ip";
        // Make sure that fresh socket is created when requesting connection data
        using (HttpClient client = new())
        {
            try
            {
                string response = await client.GetStringAsync(endpoint);
                JObject json = JObject.Parse(response);
                return json;
            }
            catch (HttpRequestException e)
            {
                TestContext.WriteLine($"GetTorStatusWithRetry failed. Result: {e.Message}");
                return null;
            }
        }
    }

    private static List<string> GetDnsAddressesForAdapterByName(string adapterName)
    {
        List<string> dnsAddresses = new();
        NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
        foreach (NetworkInterface adapter in adapters)
        {
            IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
            IPAddressCollection dnsServers = adapterProperties.DnsAddresses;
            if (adapter.Name.Equals(adapterName))
            {
                foreach (IPAddress dns in dnsServers)
                {
                    dnsAddresses.Add(dns.ToString());
                }
            }
        }

        return dnsAddresses;
    }
}