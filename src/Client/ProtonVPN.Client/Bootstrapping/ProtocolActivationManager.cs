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

using System.Diagnostics;
using Microsoft.Windows.AppLifecycle;

namespace ProtonVPN.Client.Bootstrapping;

public class ProtocolActivationManager
{
    public const string SCHEME = "protonvpn";
    public const string DISPLAY_NAME = "Proton VPN";

    private static readonly string? _clientProcessPath = Process.GetCurrentProcess().MainModule?.FileName;

    public static void Register()
    {
        ActivationRegistrationManager.RegisterForProtocolActivation(SCHEME, _clientProcessPath + ",1", DISPLAY_NAME,
            _clientProcessPath);
    }

    public static void Unregister()
    {
        ActivationRegistrationManager.UnregisterForProtocolActivation(SCHEME, _clientProcessPath);
    }
}