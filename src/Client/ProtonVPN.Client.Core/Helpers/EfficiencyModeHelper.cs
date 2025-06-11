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

using System;
using H.NotifyIcon.EfficiencyMode;
using ProtonVPN.Common.Core.Helpers;

namespace ProtonVPN.Client.Core.Helpers;

public static class EfficiencyModeHelper
{
    public static void EnableEfficiencyMode()
    {
        SetEfficiencyMode(true);
    }

    public static void DisableEfficiencyMode()
    {
        SetEfficiencyMode(false);
    }

    private static void SetEfficiencyMode(bool value)
    {
        try
        {
            // Important note: in .Net Framework if your executable assembly manifest doesn't explicitly state
            // that your exe assembly is compatible with Windows 8.1 and Windows 10.0, System.Environment.OSVersion
            // will return Windows 8 version, which is 6.2, instead of 6.3 and 10.0!
            if (Environment.OSVersion.Platform == PlatformID.Win32NT &&
                OSVersion.IsOrHigherThan(OSVersion.EfficiencyModeMinimumWindowsVersion))
            {
                EfficiencyModeUtilities.SetEfficiencyMode(value);
            }
        }
        catch (Exception)
        {
            // Could not set efficiency mode, ignore
        }
    }
}