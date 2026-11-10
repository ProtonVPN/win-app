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
using Newtonsoft.Json.Linq;
using FlaUI.Core.Tools;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Enums.Locations;

namespace ProtonVPN.UI.Tests.TestsHelper;

public static class NetworkHelper
{
    public static void VerifyLocalNetworking(bool isLanEnabled)
    {
        IPAddress? ipAddress = GetDefaultGatewayAddress() ?? throw new Exception("Default gateway is null.");
        PingReply reply = new Ping().Send(ipAddress.ToString());
        Assert.That(reply.Status == IPStatus.Success, isLanEnabled ? Is.True : Is.False);
    }

    public static void AssertInternetAvailability(bool shouldBeAvailable)
    {
        bool isAvailable = IsInternetAvailable(shouldBeAvailable);
        Assert.That(isAvailable, Is.EqualTo(shouldBeAvailable), $"Expected internet to {(shouldBeAvailable ? "be" : "not be")} available.");
    }

    public static string GetIpAddressWithRetry()
    {
        RetryResult<string> retry = Retry.WhileEmpty(
            () =>
            {
                DnsHelper.FlushDns();
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
                DnsHelper.FlushDns();
                return GetCountryNameAsync().GetAwaiter().GetResult() ?? string.Empty;
            },
            TestConstants.ThirtySecondsTimeout, TestConstants.ApiRetryInterval, ignoreException: true);
        return retry.Result ?? throw new HttpRequestException($"Failed to get country name. \n {retry.LastException?.Message} \n {retry.LastException?.StackTrace}");
    }

    public static void VerifyUserIsConnectedToExpectedCountry(Country countryNameToCompare)
    {
        string countryName = GetCountryNameWithRetry();
        Assert.That(countryName.Equals(countryNameToCompare.GetName()), Is.True, $"User was connected to unexpected country." +
            $"\n API returned: {countryName}" +
            $"\n Expected result: {countryNameToCompare}");
    }

    public static void VerifyIpAddressDoesNotMatchWithRetry(string? ipAddressToCompare)
    {
        VerifyIpAddressWithRetry(ipAddressToCompare, shouldMatch: false);
    }

    public static void VerifyIpAddressMatchesWithRetry(string? ipAddressToCompare)
    {
        VerifyIpAddressWithRetry(ipAddressToCompare, shouldMatch: true);
    }

    private static void VerifyIpAddressWithRetry(string? ipAddressToCompare, bool shouldMatch)
    {
        if (ipAddressToCompare is null)
        {
            Assert.Fail("ipAddressToCompare is null - was GetIpAddressWithRetry() called before network was ready?");
            return;
        }

        string? ipAddressFromApi = null;
        RetryResult<bool> retry = Retry.WhileFalse(
           () =>
           {
               ipAddressFromApi = GetIpAddressWithRetry();
               return ipAddressFromApi.Equals(ipAddressToCompare) == shouldMatch;
           },
           TestConstants.ThirtySecondsTimeout, TestConstants.ApiRetryInterval);

        if (!retry.Success)
        {
            Assert.Fail($"API IP Address should {(shouldMatch ? string.Empty : "not ")}match provided IP address.\n" +
                $"API returned IP address: {ipAddressFromApi}.\n" +
                $"IP to compare: {ipAddressToCompare}");
        }
    }

    public static void AssertCorrectNetworkAdapter(string adapterName)
    {
        NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
        NetworkInterface? matchingAdapter = adapters.FirstOrDefault(a => a.Description.Contains(adapterName));

        Assert.That(matchingAdapter, Is.Not.Null, $"No network adapter found with description containing '{adapterName}'" +
            $"\nAvailable adapters: {string.Join(", ", adapters.Select(a => a.Description))}");
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
        return response?["query"]?.ToString()
            ?? response?["ip"]?.ToString();
    }

    private static bool IsInternetAvailable(bool shouldBeAvailable)
    {
        DateTime timeoutDate = DateTime.UtcNow + TestConstants.ThirtySecondsTimeout;
        bool lastResult = !shouldBeAvailable;

        while (DateTime.UtcNow < timeoutDate)
        {
            JObject? connectionData = GetConnectionDataAsync(shouldBeAvailable).GetAwaiter().GetResult();
            lastResult = connectionData?["status"]?.ToString() == "success"
            || connectionData?["success"]?.Value<bool>() == true;

            if (lastResult == shouldBeAvailable)
            {
                return lastResult;
            }

            Thread.Sleep(TestConstants.FiveSecondsTimeout);
        }

        return lastResult;
    }

    private static async Task<JObject?> GetConnectionDataAsync(bool errorIsNotExpected = true)
    {
        string[] endpoints = { "http://ip-api.com/json/", "https://ipwho.is/" };

        foreach (string endpoint in endpoints)
        {
            // Make sure that fresh socket is created when requesting connection data
            using HttpClient client = new() { Timeout = TimeSpan.FromSeconds(10) };

            try
            {
                string response = await client.GetStringAsync(endpoint);
                JObject json = JObject.Parse(response);
                return json;
            }
            catch (Exception e)
            {
                if (errorIsNotExpected)
                {
                    DeferredLog.Add($"GetConnectionDataAsync failed for {endpoint}. Result: {e.Message}");
                }
            }
        }
        return null;
    }
}
