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

using ProtonVPN.Client.Settings.Contracts;

namespace ProtonVPN.Client.Settings;

/// <summary>
/// Settings that are not persisted and only valid for the current app session.
/// </summary>
public class TransientSettings : ITransientSettings
{
    public bool IsDebugModeEnabled
    {
        get
        {
            #if DEBUG
                return true;
            #else
                return false;
            #endif
        }
    }

    public bool SkipNoConnectionsPage { get; set; }

    public bool SkipOnboarding { get; set; }
}