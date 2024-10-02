/*
 * Copyright (c) 2023 Proton AG
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

using Microsoft.Windows.AppLifecycle;
using ProtonVPN.Configurations.Defaults;

namespace ProtonVPN.Client.Services.Bootstrapping.Helpers;

public static class AppProtocolHelper
{
    public const string SCHEME = "proton-vpn";
    public const string APPLICATION_NAME = "Proton VPN";

#if DEBUG
    public static readonly string ClientExePath = DefaultConfiguration.ClientExePath;
#else
    public static readonly string ClientExePath = DefaultConfiguration.ClientLauncherExePath;
#endif

    public static void Register()
    {
        ActivationRegistrationManager.RegisterForProtocolActivation(SCHEME, ClientExePath + ",1", APPLICATION_NAME, ClientExePath);
    }

    public static void Unregister()
    {
        ActivationRegistrationManager.UnregisterForProtocolActivation(SCHEME, ClientExePath);
    }
}