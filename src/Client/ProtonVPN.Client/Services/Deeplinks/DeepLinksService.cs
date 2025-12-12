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

using System.Diagnostics;
using ProtonVPN.Client.Contracts.Services.Deeplinks;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Services.Deeplinks;

public class DeepLinksService : IDeepLinksService
{
    private const string MOBILE_HOTSPOT_SETTINGS_URI = "ms-settings:network-mobilehotspot";

    private readonly ILogger _logger;

    public DeepLinksService(ILogger logger)
    {
        _logger = logger;
    }

    public void OpenMobileHotspotSettings()
    {
        OpenDeeplink(MOBILE_HOTSPOT_SETTINGS_URI);
    }

    private void OpenDeeplink(string deeplink)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = deeplink,
                UseShellExecute = true
            });

            _logger.Info<AppLog>($"Deeplink {deeplink} was opened.");
        }
        catch (Exception e)
        {
            _logger.Error<AppLog>($"Failed to open a deeplink {deeplink}.", e);
        }
    }
}