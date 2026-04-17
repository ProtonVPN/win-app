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
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net.Http;
using NUnit.Framework;

namespace ProtonVPN.UI.Tests.TestsHelper;

public class TorrentHelper
{
    private static Process? _torrentProcess;

    private const string PORT_CHECKER_API_BASE_URL = "https://portchecker.io/api";
    private const string TORRENT_URL = "https://releases.ubuntu.com/24.04/ubuntu-24.04.4-desktop-amd64.iso.torrent";

    private static string _aria2Path = @"C:\aria2\aria2-1.36.0-win-64bit-build1\aria2c.exe";
    private static string _torrentsPath = @"C:\aria2\torrents";
    private static readonly string _aria2RuleName = "ProtonVPN UI Tests - Allow aria2c";
    private static readonly string _allowAria2FirewallScript = $@"
    if (-not (Get-NetFirewallRule -DisplayName '{_aria2RuleName} - TCP' -ErrorAction SilentlyContinue))
    {{
    New-NetFirewallRule -DisplayName '{_aria2RuleName} - TCP' -Direction Inbound -Program '{_aria2Path}' -Action Allow -Profile Private,Public -Protocol TCP
    }}

    if (-not (Get-NetFirewallRule -DisplayName '{_aria2RuleName} - UDP' -ErrorAction SilentlyContinue))
    {{
    New-NetFirewallRule -DisplayName '{_aria2RuleName} - UDP' -Direction Inbound -Program '{_aria2Path}' -Action Allow -Profile Private,Public -Protocol UDP
    }}
    ";

    public static void AllowAriaFirewallScript()
    {
        WindowsUtils.RunPowerShellScript(_allowAria2FirewallScript);
    }

    public static async Task StartTorrentOnPortAsync(int port)
    {
        Directory.CreateDirectory(_torrentsPath);

        string torrentFile = Path.Combine(_torrentsPath, "test.torrent");

        if (!File.Exists(torrentFile))
        {
            using HttpClient client = new();
            byte[] data = await client.GetByteArrayAsync(TORRENT_URL);
            File.WriteAllBytes(torrentFile, data);
        }

        _torrentProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _aria2Path,
                Arguments = $"--listen-port={port} --seed-time=0 --max-download-limit=1K --dir={_torrentsPath} {torrentFile}",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        _torrentProcess.Start();
    }

    public static void StopAndCleanup()
    {
        _torrentProcess?.Kill();
        _torrentProcess?.WaitForExit(5000);
        _torrentProcess?.Dispose();
        _torrentProcess = null;

        if (Directory.Exists(_torrentsPath))
        {
            Directory.Delete(_torrentsPath, recursive: true);
        }
    }

    public static async Task IsPortOpenAsync(string ip, int port)
    {
        using HttpClient client = new();
        string url = $"{PORT_CHECKER_API_BASE_URL}/{ip}/{port}";
        DateTime timeoutDate = DateTime.UtcNow + TestConstants.ThirtySecondsTimeout;
        while (DateTime.UtcNow < timeoutDate)
        {
            HttpResponseMessage response = await client.GetAsync(url);
            string result = await response.Content.ReadAsStringAsync();

            TestContext.WriteLine($"DEBUG: {result}");
            if (result.Trim().Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                TestContext.WriteLine($"SUCCESS: Port {port} is open on {ip}");
                return;
            }

            await Task.Delay(TestConstants.FiveSecondsTimeout);
        }

        TestContext.WriteLine($"WARNING: Port {port} is not reported as open on {ip} by external port-check after 40 seconds");
    }
}