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
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using FlaUI.Core.Tools;
using NUnit.Framework;

namespace ProtonVPN.UI.Tests.TestsHelper;

public class DnsHelper
{
    private const string DNS_LEAK_TEST_URL = "https://bash.ws/";
    private static readonly HttpClient _httpClient = new();
    private static List<string> WireGuardDnsAddress => GetDnsAddressesForAdapterByName("ProtonVPN");
    private static List<string> ProTunDnsAddress => GetDnsAddressesForAdapterByName(TestConstants.IsProTunVersion ? "ProTUN" : "ProtonVPN");
    private static List<string> OpenVpnDnsAddress => GetDnsAddressesForAdapterByName("ProtonVPN TUN");

    [DllImport("dnsapi.dll", EntryPoint = "DnsFlushResolverCache")]

    private static extern uint DnsFlushResolverCache();

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
        _ = DnsFlushResolverCache();
    }

    public static void IsCustomDnsAddressSet(string dnsAddress, int order = 0)
    {
        string? wg = null;
        string? ovpn = null;
        string? protun = null;

        RetryResult<bool> retry = Retry.WhileFalse(
            () =>
            {
                wg = WireGuardDnsAddress.ElementAtOrDefault(order);
                ovpn = OpenVpnDnsAddress.ElementAtOrDefault(order);
                protun = ProTunDnsAddress.ElementAtOrDefault(order);
                return wg == dnsAddress || ovpn == dnsAddress || protun == dnsAddress;
            },
            TestConstants.FiveSecondsTimeout, TestConstants.RetryInterval);

        if (!retry.Success)
        {
            throw new Exception(
                $"WireGuard dns address: {wg}. " +
                $"OpenVPN dns address: {ovpn}. " +
                $"ProTUN dns address: {protun}. " +
                $"Expected dns value: {dnsAddress} (order {order})");
        }
    }

    public static void IsCustomDnsAddressNotSet(string dnsAddress)
    {
        List<string> wg = [];
        List<string> ovpn = [];
        List<string> protun = [];

        RetryResult<bool> retry = Retry.WhileTrue(
            () =>
            {
                wg = WireGuardDnsAddress;
                ovpn = OpenVpnDnsAddress;
                protun = ProTunDnsAddress;
                return wg.Contains(dnsAddress) || ovpn.Contains(dnsAddress) || protun.Contains(dnsAddress);
            },
            TestConstants.FiveSecondsTimeout, TestConstants.RetryInterval);

        if (!retry.Success)
        {
            static string Fmt(List<string> list) => $"[{string.Join(", ", list)}]";

            throw new Exception(
                $"WireGuard dns addresses: {Fmt(wg)}. " +
                $"OpenVPN dns addresses: {Fmt(ovpn)}. " +
                $"ProTUN dns addresses: {Fmt(protun)}. " +
                $"Expected dns value: {dnsAddress} to be absent.");
        }
    }

    public static void VerifyDnsIsNotLeaking(List<string> dnsListNotConnected)
    {
        List<string> currentDnsList = GetDnsServers();
        bool isLeaking = AnalyzeIsLeaking(currentDnsList, dnsListNotConnected);
        Assert.That(isLeaking, Is.False, "DNS Requests are being leaked while connected to VPN server.");
    }

    public static List<string> GetDnsServers()
    {
        RetryResult<List<string>> retry = Retry.WhileEmpty(
            () =>
            {
                return GetDnsServersAsync().Result;
            },
            TestConstants.OneMinuteTimeout, TestConstants.RetryInterval, ignoreException: true);
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
        using Process process = new();
        process.StartInfo.FileName = "ping";
        process.StartInfo.Arguments = $"-n 1 {domain}";
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.Start();

        if (!process.WaitForExit(TestConstants.ThirtySecondsTimeout))
        {
            try
            { process.Kill(); }
            catch { }
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
            foreach (string test in dnsListToCompare)
            {
                if (currentDnsList.Contains(test))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private static List<string> GetDnsAddressesForAdapterByName(string adapterName)
    {
        List<string> dnsAddresses = [];
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