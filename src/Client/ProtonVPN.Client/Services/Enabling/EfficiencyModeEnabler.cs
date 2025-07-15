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

using H.NotifyIcon.EfficiencyMode;
using ProtonVPN.Client.Core.Services.Enabling;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Core.Helpers;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Services.Enabling;

public class EfficiencyModeEnabler : IEfficiencyModeEnabler
{
    private readonly ILogger _logger;
    private readonly ISettings _settings;

    public EfficiencyModeEnabler(
        ILogger logger,
        ISettings settings)
    {
        _logger = logger;
        _settings = settings;
    }

    public bool TryEnableEfficiencyMode()
    {
        return TrySetEfficiencyMode(true);
    }

    public bool TryDisableEfficiencyMode()
    {
        return TrySetEfficiencyMode(false);
    }

    private bool TrySetEfficiencyMode(bool value)
    {
        try
        {
            // Important note: in .Net Framework if your executable assembly manifest doesn't explicitly state
            // that your exe assembly is compatible with Windows 8.1 and Windows 10.0, System.Environment.OSVersion
            // will return Windows 8 version, which is 6.2, instead of 6.3 and 10.0!
            bool isEfficiencyModeSupported =
                Environment.OSVersion.Platform == PlatformID.Win32NT &&
                OSVersion.IsOrHigherThan(OSVersion.EfficiencyModeMinimumWindowsVersion);

            // Check if efficiency mode is allowed by settings
            bool isEfficiencyModeAllowed = _settings.IsEfficiencyModeAllowed;

            if (isEfficiencyModeSupported && isEfficiencyModeAllowed)
            {
                EfficiencyModeUtilities.SetEfficiencyMode(value);
                return true;
            }
        }
        catch (Exception e)
        {
            _logger.Error<AppLog>($"Failed to {(value ? "enable" : "disable")} efficiency mode.", e);
        }

        return false;
    }
}