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

using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;

namespace ProtonVPN.Client.Settings.RequiredReconnections;

public class RequiredReconnectionSettings : IRequiredReconnectionSettings
{
    private readonly Dictionary<string, Func<bool>> _settings;

    public RequiredReconnectionSettings(IConnectionManager connectionManager)
    {
        _settings = new()
        {
            {nameof(ISettings.IsSplitTunnelingEnabled), () => true},
            {nameof(ISettings.SplitTunnelingMode), () => true},
            {nameof(ISettings.SplitTunnelingStandardAppsList), () => true},
            {nameof(ISettings.SplitTunnelingInverseAppsList), () => true},
            {nameof(ISettings.SplitTunnelingStandardIpAddressesList), () => true},
            {nameof(ISettings.SplitTunnelingInverseIpAddressesList), () => true},

            {nameof(ISettings.VpnProtocol), () => true},
            {nameof(ISettings.OpenVpnAdapter), () => true},

            {nameof(ISettings.IsCustomDnsServersEnabled), () => true},
            {nameof(ISettings.CustomDnsServersList), () => true},

            {nameof(ISettings.IsPortForwardingEnabled), () =>
                connectionManager.IsConnected &&
                connectionManager.CurrentConnectionIntent is not null &&
                !connectionManager.CurrentConnectionIntent.IsPortForwardingSupported()
            },
        };
    }

    public bool IsReconnectionRequired(string settingName)
    {
        return _settings.ContainsKey(settingName) && _settings[settingName]();
    }
}