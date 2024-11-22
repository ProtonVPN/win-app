/*
 * Copyright (c) 2024 Proton AG
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

using System.Diagnostics;
using System.Threading.Tasks;
using System.Net.Http;
using NUnit.Framework;
using System.Linq;
using System;
using System.Collections.Generic;
using FlaUI.Core.Tools;

namespace ProtonVPN.UI.Tests.TestsHelper;

public class DnsLeakHelper
{
    private const string DNS_LEAK_TEST_URL = "https://bash.ws/";
    private static HttpClient _httpClient = new HttpClient();

    public static void VerifyIsNotLeaking(List<string> dnsListNotConnected)
    {
        List<string> currentDnsList = GetDnsServers();
        bool isLeaking = AnalyzeIsLeaking(currentDnsList, dnsListNotConnected);
        Assert.That(isLeaking, Is.False, "DNS Requests are being leaked while connected to VPN server.");
    }

    public static List<string> GetDnsServers()
    {
        RetryResult<List<string>> retry = Retry.WhileEmpty(
            () => {
                return GetDnsServersAsync().Result;
            },
            TestConstants.ThirtySecondsTimeout, TestConstants.RetryInterval, ignoreException: true);
        return retry.Result ?? throw new HttpRequestException("Failed to get DNS servers.");
    }

    private static async Task<List<string>> GetDnsServersAsync()
    {
        string leakId = await GetTestIdAsync();
        for (int i = 1; i <= 10; i++)
        {
            PingDomain($"{i}.{leakId}.bash.ws");
        }
        string dnsTestResults = await FetchTestResultsAsync(leakId);

        List<string> dnsServers = dnsTestResults
            .Split('\n')
            .SkipLast(2)
            .Select(line => line.Split('|')[3])
            .ToList();
        return dnsServers;
    }

    private static async Task<string> GetTestIdAsync()
    {
          return await _httpClient.GetStringAsync($"{DNS_LEAK_TEST_URL}id");
    }

    private static void PingDomain(string domain)
    {
        using (Process process = new Process())
        {
            process.StartInfo.FileName = "ping";
            process.StartInfo.Arguments = $"-n 1 {domain}";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit();
        }
    }

    private static async Task<string> FetchTestResultsAsync(string leakId)
    {
        string url = $"{DNS_LEAK_TEST_URL}dnsleak/test/{leakId}?txt";
        return await _httpClient.GetStringAsync(url);
    }

    // It checks if NOT CONNECTED DNS server list, does not contain same DNS server names when CONNECTRED.
    private static bool AnalyzeIsLeaking(List<string> currentDnsList, List<string> dnsListToCompare)
    {
        foreach (string dns in currentDnsList)
        {
            foreach(string test in dnsListToCompare)
            {
                if (currentDnsList.Contains(test))
                {
                    return true;
                }
            }
        }
        return false;
    }
}
