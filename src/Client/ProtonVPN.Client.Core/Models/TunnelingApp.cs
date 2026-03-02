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

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media;

namespace ProtonVPN.Client.Core.Models;

public class TunnelingApp : ExternalApp
{
    public static TunnelingApp NotFound(string appPath, string appName, List<string>? alternateAppPaths = null) => new(appPath, appName, alternateAppPaths);

    public List<string> AlternateAppPaths { get; }

    protected TunnelingApp(
        string appPath,
        string appName,
        ImageSource? appIcon,
        List<string>? alternateAppPaths)
        : base(appPath, appName, appIcon)
    {
        AlternateAppPaths = alternateAppPaths ?? [];
    }

    protected TunnelingApp(
        string appPath,
        string appName,
        List<string>? alternateAppPaths)
        : this(appPath, appName, null, alternateAppPaths)
    { }

    public static async Task<TunnelingApp?> TryCreateAsync(string appPath, List<string>? alternateAppPaths = null)
    {
        ExternalApp? externalApp = await ExternalApp.TryCreateAsync(appPath);
        if (externalApp == null)
        {
            return null;
        }

        return new TunnelingApp(externalApp.AppPath, externalApp.AppName, externalApp.AppIcon, alternateAppPaths);
    }
}