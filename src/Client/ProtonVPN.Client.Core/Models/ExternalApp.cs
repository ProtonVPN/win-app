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

using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Common.UI.Extensions;
using ProtonVPN.Common.Core.Extensions;

namespace ProtonVPN.Client.Core.Models;

public class ExternalApp : ObservableObject
{
    public string AppPath { get; }
    public string AppName { get; }
    public ImageSource? AppIcon { get; private set; }

    public bool IsValid => AppPath.IsValidPath();

    protected ExternalApp(string appPath, string appName, ImageSource? appIcon)
        : this(appPath, appName)
    {
        AppIcon = appIcon;
    }

    protected ExternalApp(string appPath, string appName)
    {
        AppPath = appPath;
        AppName = appName;
    }

    public static ExternalApp? TryCreate(string appPath)
    {
        appPath = appPath?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(appPath) || !Path.Exists(appPath))
        {
            return null;
        }

        string appName = appPath.GetAppName();

        return new ExternalApp(appPath, appName);
    }

    public static async Task<ExternalApp?> TryCreateAsync(string appPath)
    {
        ExternalApp? app = TryCreate(appPath);

        if (app != null)
        {
            app.AppIcon = await appPath.GetAppIconAsync();
        }

        return app;
    }

    public override string ToString()
    {
        return AppName;
    }
}