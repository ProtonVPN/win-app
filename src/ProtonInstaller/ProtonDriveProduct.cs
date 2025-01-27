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

using System.Runtime.InteropServices;
using System.Text.Json;
using ProtonInstaller.Contracts.ProtonDrive;

namespace ProtonInstaller;

public class ProtonDriveProduct : ProtonProductBase
{
    private const string STABLE_CHANNEL = "Stable";
    private const string FEED_URL = "https://proton.me/download/drive/windows/{0}/v1/version.json";

    public ProtonDriveProduct(HttpClient downloadHttpClient, HttpClient versionHttpClient) : base(downloadHttpClient, versionHttpClient, GetFeedUrl())
    {
    }

    private static string GetFeedUrl()
    {
        string architecture = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.Arm64 => "arm64",
            _ => throw new NotSupportedException($"Unsupported architecture {RuntimeInformation.ProcessArchitecture}.")
        };

        return string.Format(FEED_URL, architecture);
    }

    protected override async Task<ILatestRelease?> GetLatestReleaseAsync()
    {
        HttpResponseMessage response = await VersionHttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, FeedUrl));
        Stream stream = await response.Content.ReadAsStreamAsync();
        ProtonDriveReleasesResponse? releasesResponse = await JsonSerializer.DeserializeAsync(stream, JsonContext.Default.ProtonDriveReleasesResponse);
        ProtonDriveReleaseResponse? release = releasesResponse?.Releases?
            .Where(r => r.CategoryName == STABLE_CHANNEL && (r.RolloutRatio is null || r.RolloutRatio > 0.00))
            .MaxBy(r => GetVersion(r.Version));

        return release is not null
            ? new LatestRelease
            {
                BinaryUrl = release.File.Url,
                Sha512Checksum = release.File.Sha512Checksum,
                SilentArgument = release.File.SilentArguments,
                NoAutoLaunchArgument = "NoAutoLaunch=1",
                NoDesktopShortcutArgument = "NoDesktopShortcut=1",
            }
            : null;
    }

    protected override string GetSourceArgument()
    {
        return "Source=vpn";
    }
}