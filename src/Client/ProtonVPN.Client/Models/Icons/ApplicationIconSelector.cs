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
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;

namespace ProtonVPN.Client.Models.Icons;

public class ApplicationIconSelector : IApplicationIconSelector
{
    private readonly IConnectionManager _connectionManager;
    private readonly IUserAuthenticator _userAuthenticator;

    public static ImageSource ConnectedIcon => ResourceHelper.GetIcon("ProtonVpnProtectedTrayIcon");
    public static ImageSource ErrorIcon => ResourceHelper.GetIcon("ProtonVpnErrorTrayIcon");
    public static ImageSource DisconnectedIcon => ResourceHelper.GetIcon("ProtonVpnUnprotectedTrayIcon");
    public static ImageSource LoggedOutIcon => ResourceHelper.GetIcon("ProtonVpnLoggedOutTrayIcon");

    public ApplicationIconSelector(IConnectionManager connectionManager, IUserAuthenticator userAuthenticator)
    {
        _connectionManager = connectionManager;
        _userAuthenticator = userAuthenticator;
    }

    public ImageSource Get()
    {
        return _userAuthenticator.IsLoggedIn
            ? _connectionManager.IsConnected
                ? ConnectedIcon
                : _connectionManager.HasError
                    ? ErrorIcon
                    : DisconnectedIcon
            : LoggedOutIcon;
    }
}