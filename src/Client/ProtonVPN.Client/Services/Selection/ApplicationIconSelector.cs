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

using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Core.Helpers;
using ProtonVPN.Client.Core.Models;
using ProtonVPN.Client.Core.Services.Selection;

namespace ProtonVPN.Client.Services.Selection;

public class ApplicationIconSelector : IApplicationIconSelector
{
    public static string ProtonVPNIconPath => "Assets/ProtonVPN.ico";
    public static ImageSource ConnectedIcon => ResourceHelper.GetIcon("ProtonVpnProtectedTrayIcon");
    public static ImageSource ErrorIcon => ResourceHelper.GetIcon("ProtonVpnErrorTrayIcon");
    public static ImageSource DisconnectedIcon => ResourceHelper.GetIcon("ProtonVpnUnprotectedTrayIcon");
    public static ImageSource LoggedOutIcon => ResourceHelper.GetIcon("ProtonVpnLoggedOutTrayIcon");

    public ApplicationIconSelector()
    { }

    public string GetAppIconPath()
    {
        return ProtonVPNIconPath;
    }

    public ImageSource GetStatusIcon(IconStatusParameters parameters)
    {
        return parameters.IsAuthenticated
            ? parameters.IsConnected
                ? ConnectedIcon
                : parameters.HasError
                    ? ErrorIcon
                    : DisconnectedIcon
            : LoggedOutIcon;
    }
}