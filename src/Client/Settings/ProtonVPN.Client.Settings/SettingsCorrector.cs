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

using ProtonVPN.Client.Settings.Contracts;

namespace ProtonVPN.Client.Settings;

public class SettingsCorrector : ISettingsCorrector
{
    private readonly ISettings _settings;

    public SettingsCorrector(ISettings settings)
    {
        _settings = settings;
    }

    public void Correct()
    {
        if (!_settings.VpnPlan.IsPaid)
        {
            // We store setting values for free user as it was paid, but the real value returned
            // by ISettings will be not this one, but a correct one due to its implementation which
            // checks if the user is paid or free and returns the right value.
            _settings.IsNetShieldEnabled = DefaultSettings.IsNetShieldEnabled(true);
            _settings.IsLocalAreaNetworkAccessEnabled = DefaultSettings.IsLocalAreaNetworkAccessAllowed(true);
        }
    }
}