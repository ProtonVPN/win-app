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
using System.IO;
using System.Net.Http;
using System.Diagnostics;
using System.Threading.Tasks;
using FlaUI.Core.Tools;
using NUnit.Framework;

namespace ProtonVPN.UI.Tests.TestsHelper;

public static class TorrentHelper
{
    private const string TORRENT_PROCESS_NAME = "qbittorrent";

    private const string PORT_CHECKER_API_BASE_URL = "https://portchecker.io/api";
    private const string TORRENT_URL = "https://releases.ubuntu.com/24.04/ubuntu-24.04.4-desktop-amd64.iso.torrent";

    private static readonly string _qbittorrentFolder = @"C:\qBittorrent";
    private static readonly string _qbittorrentExePath = @"C:\Program Files\qBittorrent\qbittorrent.exe";
    private static readonly string _torrentsPath = $@"{_qbittorrentFolder}\torrents";
    private static readonly string _profilePath = $@"{_qbittorrentFolder}\profile";
    private static readonly string _cachedTorrentFile = Path.Combine(_qbittorrentFolder, "test.torrent");
    private static readonly string _runTorrentFile = Path.Combine(_torrentsPath, "test.torrent");

    private static readonly string _qbittorrentRuleName = "ProtonVPN UI Tests - Allow qbittorrent";
    private static readonly string _qBittorrentFirewallScript = $@"
    if (-not (Get-NetFirewallRule -DisplayName '{_qbittorrentRuleName} - TCP' -ErrorAction SilentlyContinue))
    {{
    New-NetFirewallRule -DisplayName '{_qbittorrentRuleName} - TCP' -Direction Inbound -Program '{_qbittorrentExePath}' -Action Allow -Profile Private,Public -Protocol TCP
    }}

    if (-not (Get-NetFirewallRule -DisplayName '{_qbittorrentRuleName} - UDP' -ErrorAction SilentlyContinue))
    {{
    New-NetFirewallRule -DisplayName '{_qbittorrentRuleName} - UDP' -Direction Inbound -Program '{_qbittorrentExePath}' -Action Allow -Profile Private,Public -Protocol UDP
    }}
    ";

    public static void AllowTorrentFirewall()
    {
        ScriptHelper.RunPowerShellScript(_qBittorrentFirewallScript);
    }

    public static void StartTorrentOnPort(int port)
    {
        RetryResult<bool> retry = Retry.WhileFalse(
            () => StartTorrentOnPortAsync(port).GetAwaiter().GetResult(),
            TestConstants.ThirtySecondsTimeout, TestConstants.ApiRetryInterval);

        Assert.That(retry.Result, Is.True, $"Failed to start qBittorrent with port: {port}");
    }

    public static void IsPortOpen(string ip, int port)
    {
        RetryResult<bool> retry = Retry.WhileFalse(
            () => IsPortOpenAsync(ip, port).GetAwaiter().GetResult(),
            TestConstants.ThirtySecondsTimeout, TestConstants.ApiRetryInterval);

        if (!retry.Result)
        {
            TestContext.WriteLine($"WARNING: Port {port} was not reported as open on {ip} within timeout");
        }
    }

    public static void IsPortClosed(string ip, int port)
    {
        RetryResult<bool> retry = Retry.WhileTrue(
            () => IsPortOpenAsync(ip, port).GetAwaiter().GetResult(),
            TestConstants.ThirtySecondsTimeout, TestConstants.ApiRetryInterval);

        if (!retry.Result)
        {
            TestContext.WriteLine($"WARNING: Port {port} is still reported as open on {ip} within timeout");
        }
    }

    public static void StopAndCleanup()
    {
        foreach (Process process in Process.GetProcessesByName(TORRENT_PROCESS_NAME))
        {
            try
            {
                process.Kill(entireProcessTree: true);
                process.WaitForExit(TestConstants.ThirtySecondsTimeout);
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Could not kill '{TORRENT_PROCESS_NAME}' (PID {process.Id}): {ex.Message}");
            }
            finally
            {
                process.Dispose();
            }
        }

        DeleteDirectoryWithRetry(_torrentsPath);
        DeleteDirectoryWithRetry(_profilePath);
    }

    // qBittorrent can keep its files locked for a moment after the process exits.
    private static void DeleteDirectoryWithRetry(string path)
    {
        RetryResult<bool> retry = Retry.WhileFalse(() =>
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }

            return true;
        },
        TestConstants.TenSecondsTimeout, TestConstants.OneSecondTimeout, ignoreException: true);

        Assert.That(retry.Success, Is.True, $"Could not delete '{path}': {retry.LastException?.Message}");
    }

    private static async Task<bool> StartTorrentOnPortAsync(int port)
    {
        Directory.CreateDirectory(_torrentsPath);
        Directory.CreateDirectory(_profilePath);

        if (!File.Exists(_cachedTorrentFile))
        {
            using HttpClient client = new() { Timeout = TestConstants.ThirtySecondsTimeout };
            byte[] data = await client.GetByteArrayAsync(TORRENT_URL);
            File.WriteAllBytes(_cachedTorrentFile, data);
        }

        File.Copy(_cachedTorrentFile, _runTorrentFile, overwrite: true);

        Process.Start(new ProcessStartInfo
        {
            FileName = _qbittorrentExePath,
            Arguments = $"--confirm-legal-notice --skip-dialog=true --profile=\"{_profilePath}\" --torrenting-port={port}  --save-path=\"{_torrentsPath}\" \"{_runTorrentFile}\"",
            UseShellExecute = false,
            CreateNoWindow = true
        });

        return true;
    }

    private static async Task<bool> IsPortOpenAsync(string ip, int port)
    {
        using HttpClient client = new() { Timeout = TestConstants.TenSecondsTimeout };
        string url = $"{PORT_CHECKER_API_BASE_URL}/{ip}/{port}";

        try
        {
            HttpResponseMessage response = await client.GetAsync(url);
            string result = await response.Content.ReadAsStringAsync();

            DeferredLog.Add($"DEBUG: Port check for {ip}:{port} returned: {result}");
            return result.Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            DeferredLog.Add($"WARNING: Port check request failed: {ex.Message}");
            return false;
        }
    }
}