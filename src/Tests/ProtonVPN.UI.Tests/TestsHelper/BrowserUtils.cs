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
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.WebSockets;
using System.Diagnostics;
using FlaUI.Core.Tools;
using NUnit.Framework;
using ProtonVPN.Common.Core.Extensions;

namespace ProtonVPN.UI.Tests.TestsHelper;

public class BrowserUtils
{
    private const int CHROME_PORT = 9222;
    private const int EDGE_PORT = 9223;
    private const string CHROME_PATH = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
    private const string EDGE_PATH = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";

    public static void KillAllBrowsers()
    {
        Thread.Sleep(TestConstants.OneSecondTimeout);
        foreach (string? name in new[] { "chrome", "msedge" })
        {
            foreach (Process process in Process.GetProcessesByName(name))
            {
                try
                {
                    process.Kill(true);
                }
                catch { }
            }
        }
        Thread.Sleep(TestConstants.OneSecondTimeout);
    }

    public static void VerifyBrowserIpWithRetry(string browserApp, bool hasVpn, string? ipAddressToCompare)
    {
        string? browserIp = null;
        RetryResult<string> retry = Retry.WhileEmpty(
            () =>
            {
                browserIp = GetBrowserIpWithRetry(browserApp);
                return browserIp;
            },
            TestConstants.ThirtySecondsTimeout, TestConstants.ApiRetryInterval);

        if (retry.Success)
        {
            Assert.That((browserIp == ipAddressToCompare) == hasVpn, $"Expected {browserApp} to have VPN {hasVpn.ToOnOffString()}" +
            $"\n {browserApp} has IP: {browserIp}" +
            $"\n VPN App has IP: {ipAddressToCompare}");
        }
    }

    public static void AssertBrowserInternetAvailability(string browserApp, bool shouldBeAvailable)
    {
        string? browserIp = null;
        RetryResult<string> retry = Retry.WhileEmpty(
            () =>
            {
                browserIp = GetBrowserIpWithRetry(browserApp);
                return browserIp;
            },
            TestConstants.ThirtySecondsTimeout, TestConstants.ApiRetryInterval);

        if (retry.Success)
        {
            if (shouldBeAvailable)
            {
                Assert.That(browserIp, Does.Match(@"\b\d{1,3}(\.\d{1,3}){3}\b"), "Expected internet to be available.");
            }
            else
            {
                Assert.That(browserIp, Does.Contain("Your Internet access is blocked").Or.Contain("This site can’t be reached").Or.Contain("Press space to play"), "Expected internet to not be available.");
            }
        }
    }

    private static void StartBrowserWithCDP(string browserPath, int debugPort)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = browserPath,
            Arguments = $"--remote-debugging-port={debugPort} --headless about:blank"
        });
    }

    private static async Task<string> GetBrowserIpAsync(int debugPort)
    {
        // This method connects to the Browser via CDP and gets the IP that the Browser sees
        // It uses https://api4.my-ip.io/v2/ip.txt instead of http://ip-api.com/json, because the Browser forces HTTPS via HSTS, and ip-api.com does not support HTTPS on the free tier

        string endpoint = "https://api4.my-ip.io/v2/ip.txt";

        await Task.Delay(TestConstants.TwoSecondsTimeout);

        using HttpClient http = new HttpClient();
        string json = await http.GetStringAsync($"http://localhost:{debugPort}/json");
        JsonElement tabs = JsonSerializer.Deserialize<JsonElement>(json);

        // Find the actual page
        string? wsUrl = null;
        foreach (JsonElement tab in tabs.EnumerateArray())
        {
            if (tab.GetProperty("type").GetString() == "page")
            {
                wsUrl = tab.GetProperty("webSocketDebuggerUrl").GetString();
                break;
            }
        }

        using ClientWebSocket ws = new ClientWebSocket();
        await ws.ConnectAsync(new Uri(wsUrl!), CancellationToken.None);

        // Helper function to send commands to the browser
        async Task<JsonElement> Send(object cmd)
        {
            string msg = JsonSerializer.Serialize(cmd);
            await ws.SendAsync(Encoding.UTF8.GetBytes(msg), WebSocketMessageType.Text, true, CancellationToken.None);
            byte[] buffer = new byte[4096];
            WebSocketReceiveResult result = await ws.ReceiveAsync(buffer, CancellationToken.None);
            return JsonSerializer.Deserialize<JsonElement>(Encoding.UTF8.GetString(buffer, 0, result.Count));
        }

        await Send(new { id = 1, method = "Page.navigate", @params = new { url = endpoint } });
        await Task.Delay(TestConstants.TwoSecondsTimeout);

        JsonElement evalResult = await Send(new
        {
            id = 2,
            method = "Runtime.evaluate",
            @params = new { expression = "document.body.innerText.trim()" }
        });

        string rawResult = evalResult
            .GetProperty("result")
            .GetProperty("result")
            .GetProperty("value")
            .GetString() ?? "unknown";

        string ip = rawResult.Split('\n', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
        return ip;
    }

    private static string GetBrowserIpWithRetry(string browserApp)
    {
        string? browserPath = null;
        int debugPort = 0;

        if (browserApp == "Google Chrome")
        {
            browserPath = CHROME_PATH;
            debugPort = CHROME_PORT;
        }
        else if (browserApp == "Edge")
        {
            browserPath = EDGE_PATH;
            debugPort = EDGE_PORT;
        }

        RetryResult<string> retry = Retry.WhileEmpty(
            () =>
            {
                StartBrowserWithCDP(browserPath!, debugPort);
                return GetBrowserIpAsync(debugPort).Result ?? string.Empty;
            },
            TestConstants.ThirtySecondsTimeout, TestConstants.ApiRetryInterval, ignoreException: true);
        return retry.Result ?? "This site can’t be reached";
    }
}