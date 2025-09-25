/*
 * Copyright (c) 2025 Proton AG
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

using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media;

namespace ProtonVPN.Client.Core.Models;

public class WebBrowserApp : ExternalApp
{
    public string? PrivateBrowsingArgument { get; }

    public bool SupportsPrivateBrowsing => !string.IsNullOrWhiteSpace(PrivateBrowsingArgument);

    protected WebBrowserApp(
        string appPath,
        string appName,
        string? privateBrowsingArgument)
        : base(appPath, appName)
    {
        PrivateBrowsingArgument = privateBrowsingArgument;
    }

    protected WebBrowserApp(
        string appPath,
        string appName,
        ImageSource? appIcon,
        string? privateBrowsingArgument)
        : base(appPath, appName, appIcon)
    {
        PrivateBrowsingArgument = privateBrowsingArgument;
    }

    public static WebBrowserApp? TryCreate(string appPath, string? privateBrowsingArgument = null)
    {
        ExternalApp? externalApp = ExternalApp.TryCreate(appPath);
        if (externalApp == null)
        {
            return null;
        }

        return new WebBrowserApp(externalApp.AppPath, externalApp.AppName, externalApp.AppIcon, privateBrowsingArgument);
    }

    public static async Task<WebBrowserApp?> TryCreateAsync(string appPath, string? privateBrowsingArgument = null)
    {
        ExternalApp? externalApp = await ExternalApp.TryCreateAsync(appPath);
        if (externalApp == null)
        {
            return null;
        }

        return new WebBrowserApp(externalApp.AppPath, externalApp.AppName, externalApp.AppIcon, privateBrowsingArgument);
    }
}