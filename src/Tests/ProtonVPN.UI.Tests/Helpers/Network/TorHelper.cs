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
using System.Net.Http;
using System.Threading.Tasks;
using FlaUI.Core.Tools;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace ProtonVPN.UI.Tests.TestsHelper;

public static class TorHelper
{
    public static void AssertTorStatus(bool shouldBeAvailable, string? vpnIp = null)
    {
        string? ip = null;
        bool? isTor = null;

        RetryResult<string> retry = Retry.WhileEmpty(
            () =>
            {
                DnsHelper.FlushDns();
                JObject? result = GetTorStatusAsync().GetAwaiter().GetResult();
                ip = result?["IP"]?.Value<string>();
                isTor = result?["IsTor"]?.Value<bool>();
                // Returning only the IP, since IP and IsTor are always returned together
                return ip ?? string.Empty;
            },
            TestConstants.ThirtySecondsTimeout, TestConstants.ApiRetryInterval, ignoreException: true);

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
        using (HttpClient client = new() { Timeout = TestConstants.TenSecondsTimeout })
        {
            try
            {
                string response = await client.GetStringAsync(endpoint);
                JObject json = JObject.Parse(response);
                return json;
            }
            catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
            {
                TestContext.WriteLine($"GetTorStatusWithRetry failed. Result: {e.Message}");
                return null;
            }
        }
    }
}
