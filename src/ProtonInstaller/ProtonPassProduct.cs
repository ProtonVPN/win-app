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

using System.Text.Json;
using ProtonInstaller.Contracts.ProtonPass;

namespace ProtonInstaller;

public class ProtonPassProduct : ProtonProductBase
{
    private const string STABLE_CHANNEL = "Stable";
    private const string FEED_URL = "https://proton.me/download/PassDesktop/win32/x64/version.json";

    public ProtonPassProduct(HttpClient downloadHttpClient, HttpClient versionHttpClient) : base(downloadHttpClient, versionHttpClient, FEED_URL)
    {
    }

    protected override async Task<ILatestRelease?> GetLatestReleaseAsync()
    {
        HttpResponseMessage response = await VersionHttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, FeedUrl));
        Stream stream = await response.Content.ReadAsStreamAsync();
        ProtonPassReleasesResponse? releasesResponse = await JsonSerializer.DeserializeAsync(stream, JsonContext.Default.ProtonPassReleasesResponse);
        ProtonPassReleaseResponse? release = releasesResponse?.Releases?
            .Where(r => r.CategoryName == STABLE_CHANNEL && r.RolloutPercentage is not null && r.RolloutPercentage > 0.00)
            .MaxBy(r => GetVersion(r.Version));

        ProtonPassFileResponse? file = release?.File.FirstOrDefault();

        return file is not null
            ? new LatestRelease
            {
                BinaryUrl = file.Url,
                Sha512Checksum = file.Sha512CheckSum,
                SilentArgument = "--silent=1",
                NoAutoLaunchArgument = "--auto-launch=0",
                NoDesktopShortcutArgument = "--desktop-shortcut=0",
            }
            : null;
    }

    protected override string GetSourceArgument()
    {
        return "--source=vpn";
    }
}