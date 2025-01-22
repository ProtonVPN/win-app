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
using System.Security.Cryptography;

namespace ProtonInstaller;

public abstract class ProtonProductBase : IProtonProduct
{
    private const int MAX_RETRIES = 3;

    private readonly HttpClient _downloadHttpClient;
    protected readonly HttpClient VersionHttpClient;
    private readonly TimeSpan _maxInstallDuration = TimeSpan.FromMinutes(5);

    public string FeedUrl { get; }

    protected ProtonProductBase(HttpClient downloadHttpClient, HttpClient versionHttpClient, string feedUrl)
    {
        _downloadHttpClient = versionHttpClient;
        VersionHttpClient = versionHttpClient;
        FeedUrl = feedUrl;
    }

    public async Task InstallAsync(bool isToCreateDesktopShortcut)
    {
        try
        {
            ILatestRelease? release = await GetLatestReleaseAsync();
            if (release != null)
            {
                string pathToSave = GetPathToSave(release);
                await DownloadWithRetryAsync(release.BinaryUrl, pathToSave);
                if (File.Exists(pathToSave))
                {
                    string hash = GetSha512(pathToSave);
                    if (hash == release.Sha512Checksum)
                    {
                        StartProcess(pathToSave, GetCommandLineArguments(release, isToCreateDesktopShortcut));
                    }
                    else
                    {
                        File.Delete(pathToSave);
                    }
                }
            }
        }
        catch (Exception)
        {
        }
    }

    protected abstract Task<ILatestRelease?> GetLatestReleaseAsync();

    protected abstract string GetSourceArgument();

    protected Version GetVersion(string releaseVersion)
    {
        return Version.TryParse(releaseVersion, out Version? version) ? version : new();
    }

    private string GetPathToSave(ILatestRelease release)
    {
        Uri fileUri = new(release.BinaryUrl);
        return Path.Combine(Path.GetTempPath(), Path.GetFileName(fileUri.AbsolutePath));
    }

    private async Task DownloadWithRetryAsync(string url, string filename)
    {
        TimeSpan delay = TimeSpan.FromSeconds(2);

        for (int retryCount = 0; retryCount <= MAX_RETRIES; retryCount++)
        {
            try
            {
                await DownloadAsync(url, filename);
                if (File.Exists(filename))
                {
                    return;
                }
            }
            catch (Exception) when (retryCount < MAX_RETRIES)
            {
                await Task.Delay(delay);
                delay += delay;
            }
        }
    }

    private async Task DownloadAsync(string url, string filename)
    {
        HttpResponseMessage response = await _downloadHttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        await using Stream contentStream = await response.Content.ReadAsStreamAsync();
        await using FileStream fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
        byte[] buffer = new byte[8192];

        while (true)
        {
            int bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead <= 0)
            {
                break;
            }

            await fileStream.WriteAsync(buffer, 0, bytesRead);
        }
    }

    private string GetSha512(string filePath)
    {
        using FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using SHA512 sha512 = SHA512.Create();
        byte[] hashBytes = sha512.ComputeHash(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    private void StartProcess(string path, string arguments)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = path,
            Arguments = arguments,
            UseShellExecute = false,
        };

        Process process = new Process
        {
            StartInfo = startInfo
        };

        process.Start();

        if (!process.WaitForExit(_maxInstallDuration))
        {
            process.Kill();
        }
    }

    private string GetCommandLineArguments(ILatestRelease release, bool isToCreateDesktopShortcut)
    {
        List<string> arguments = [GetSourceArgument(), release.SilentArgument, release.NoAutoLaunchArgument];
        if (!isToCreateDesktopShortcut)
        {
            arguments.Add(release.NoDesktopShortcutArgument);
        }

        return string.Join(' ', arguments);
    }
}