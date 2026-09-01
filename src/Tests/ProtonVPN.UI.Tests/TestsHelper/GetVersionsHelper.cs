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
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Net.Sockets;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Security.Authentication;
using NUnit.Framework;

namespace ProtonVPN.UI.Tests.TestsHelper;

public static class GetVersionsHelper
{
    public const string NEW_VERSION = "9.9.9";
    public const string OLD_VERSION = "4.4.0";

    private const string AUTH_HEADER_NAME = "JOB-TOKEN";
    private static readonly string _authHeaderValue = Environment.GetEnvironmentVariable("CI_JOB_TOKEN") ?? throw new Exception("CI_JOB_TOKEN/CI_API_V4_URL/CI_PROJECT_ID are required; update tests cannot run locally");
    private static readonly string _apiUrl = Environment.GetEnvironmentVariable("CI_API_V4_URL")!;
    private static readonly string _projectId = Environment.GetEnvironmentVariable("CI_PROJECT_ID")!;

    private const string BUILD_INSTALLER_JOB_NAME = "build-installer-release-x64";
    private static readonly string _newVersionFolder = @"C:\NewVersion";
    private static readonly string _newInstallerPath = Path.Combine(_newVersionFolder, $"ProtonVPN_v{NEW_VERSION}_x64.exe");

    private const string OLD_VERSION_URL = $"https://github.com/ProtonVPN/win-app/releases/download/v{OLD_VERSION}/ProtonVPN_v{OLD_VERSION}_x64.exe";
    private static readonly string _oldVersionFolder = @"C:\OldVersions";
    private static readonly string _oldInstallerPath = Path.Combine(_oldVersionFolder, $"ProtonVPN_v{OLD_VERSION}_x64.exe");

    public static string GetOldVersionPath()
    {
        if (File.Exists(_oldInstallerPath))
        {
            return _oldInstallerPath;
        }

        DownloadOldVersionAsync().GetAwaiter().GetResult();

        Assert.That(File.Exists(_oldInstallerPath), Is.True, $"Old installer was not downloaded correctly: {_oldInstallerPath}");

        return _oldInstallerPath;
    }

    public static string GetNewVersionPath(string currentVersion)
    {
        if (File.Exists(_newInstallerPath))
        {
            //because each release creates a new v9.9.9 so we shouldnt use the old 9.9.9
            File.Delete(_newInstallerPath);
        }

        FetchAndDownloadNewVersionAsync(currentVersion).GetAwaiter().GetResult();

        Assert.That(File.Exists(_newInstallerPath), Is.True, $"New installer was not downloaded correctly: {_newInstallerPath}");

        return _newInstallerPath;
    }

    private static async Task DownloadOldVersionAsync()
    {
        Directory.CreateDirectory(_oldVersionFolder);

        using HttpClient client = CreateHttpClient();

        const int maxAttempts = 3;
        Exception? lastException = null;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                byte[] data = await client.GetByteArrayAsync(OLD_VERSION_URL);
                await File.WriteAllBytesAsync(_oldInstallerPath, data);
                return;
            }
            catch (Exception ex) when (ex is HttpRequestException or IOException or SocketException)
            {
                lastException = ex;
                Console.WriteLine($"[DownloadOldVersionAsync] Attempt {attempt}/{maxAttempts} failed: {ex.GetType().Name}: {ex.Message}");

                if (attempt < maxAttempts)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5 * attempt));
                }
            }
        }

        throw new Exception($"Failed to download older version after {maxAttempts} attempts.", lastException);
    }

    private static async Task FetchAndDownloadNewVersionAsync(string currentVersion)
    {
        string prefix = $"release/{NEW_VERSION}-from-{currentVersion}-";

        using HttpClient client = CreateHttpClient();
        client.DefaultRequestHeaders.Add(AUTH_HEADER_NAME, _authHeaderValue);

        string branchName = await GetBranchAsync(client, prefix) ?? throw new InvalidOperationException($"No branch found with prefix '{prefix}'.");

        int pipelineId = await GetPipelineIdAsync(client, branchName) ?? throw new InvalidOperationException($"No pipeline found for branch '{branchName}'.");

        int jobId = await GetJobIdAsync(client, pipelineId) ?? throw new InvalidOperationException($"Job '{BUILD_INSTALLER_JOB_NAME}' has not completed successfully in pipeline {pipelineId}.");

        await DownloadNewVersionAsync(client, jobId);
    }

    private static async Task<string?> GetBranchAsync(HttpClient client, string prefix)
    {
        string branchesUrl = $"{_apiUrl}/projects/{Uri.EscapeDataString(_projectId)}/repository/branches?search={Uri.EscapeDataString(prefix)}&per_page=100";
        using JsonDocument branchesDoc = JsonDocument.Parse(await client.GetStringAsync(branchesUrl));
        string? branchName = branchesDoc.RootElement.EnumerateArray()
            .Select(b => b.GetProperty("name").GetString()!)
            .FirstOrDefault(n => n.StartsWith(prefix, StringComparison.Ordinal));

        if (branchName is null)
        {
            Console.WriteLine($"[GetBranchAsync] No matching branch found for prefix: {prefix}");
        }

        return branchName;
    }

    private static async Task<int?> GetPipelineIdAsync(HttpClient client, string branchName)
    {
        string pipelinesUrl = $"{_apiUrl}/projects/{Uri.EscapeDataString(_projectId)}/pipelines?ref={Uri.EscapeDataString(branchName)}&order_by=id&sort=desc&per_page=1";
        using JsonDocument pipelinesDoc = JsonDocument.Parse(await client.GetStringAsync(pipelinesUrl));

        if (pipelinesDoc.RootElement.GetArrayLength() == 0)
        {
            Console.WriteLine($"[GetPipelineIdAsync] No pipeline found for branch '{branchName}'.");
            return null;
        }

        return pipelinesDoc.RootElement[0].GetProperty("id").GetInt32();
    }

    private static async Task<int?> GetJobIdAsync(HttpClient client, int pipelineId)
    {
        string jobsUrl = $"{_apiUrl}/projects/{Uri.EscapeDataString(_projectId)}/pipelines/{pipelineId}/jobs?per_page=100";
        using JsonDocument jobsDoc = JsonDocument.Parse(await client.GetStringAsync(jobsUrl));
        int? jobId = jobsDoc.RootElement.EnumerateArray()
            .Where(j => j.GetProperty("name").GetString() == BUILD_INSTALLER_JOB_NAME && j.GetProperty("status").GetString() == "success")
            .Select(j => (int?)j.GetProperty("id").GetInt32())
            .FirstOrDefault();

        if (jobId is null)
        {
            Console.WriteLine($"[GetJobIdAsync] Job '{BUILD_INSTALLER_JOB_NAME}' is not successful in pipeline {pipelineId}.");
        }

        return jobId;
    }

    private static async Task DownloadNewVersionAsync(HttpClient client, int jobId)
    {
        Directory.CreateDirectory(_newVersionFolder);

        byte[] zipBytes = await client.GetByteArrayAsync($"{_apiUrl}/projects/{_projectId}/jobs/{jobId}/artifacts");
        using MemoryStream zipStream = new(zipBytes);
        using ZipArchive archive = new(zipStream, ZipArchiveMode.Read);
        ZipArchiveEntry exeEntry = archive.Entries.FirstOrDefault(e => e.FullName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) ?? throw new InvalidOperationException("No .exe found in artifact zip");
        exeEntry.ExtractToFile(_newInstallerPath, overwrite: true);
    }

    private static HttpClient CreateHttpClient()
    {
        SocketsHttpHandler handler = new()
        {
            AllowAutoRedirect = true,
            SslOptions =
            {
                EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
            }
        };

        HttpClient client = new(handler, disposeHandler: true)
        {
            Timeout = TimeSpan.FromMinutes(3)
        };

        client.DefaultRequestHeaders.UserAgent.ParseAdd("ProtonVPN-UI-Tests/1.0");

        return client;
    }
}