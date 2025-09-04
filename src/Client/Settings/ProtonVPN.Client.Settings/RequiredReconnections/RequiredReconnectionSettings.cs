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

    public RequiredReconnectionSettings(
        IConnectionManager connectionManager,
        ISettings settings)
    {
        _settings = new()
        {
            {nameof(ISettings.IsSplitTunnelingEnabled), () => true},
            {nameof(ISettings.SplitTunnelingMode), () => settings.IsSplitTunnelingEnabled},
            {nameof(ISettings.SplitTunnelingStandardAppsList), () => settings.IsSplitTunnelingEnabled},
            {nameof(ISettings.SplitTunnelingInverseAppsList), () => settings.IsSplitTunnelingEnabled},
            {nameof(ISettings.SplitTunnelingStandardIpAddressesList), () => settings.IsSplitTunnelingEnabled},
            {nameof(ISettings.SplitTunnelingInverseIpAddressesList), () => settings.IsSplitTunnelingEnabled},

            {nameof(ISettings.VpnProtocol), () => true},
            {nameof(ISettings.OpenVpnAdapter), () => true},
            {nameof(ISettings.IsIpv6LeakProtectionEnabled), () => true},
            {nameof(ISettings.IsIpv6Enabled), () => true},
            {nameof(ISettings.DnsBlockMode), () => true},

            {nameof(ISettings.IsCustomDnsServersEnabled), () => true},
            {nameof(ISettings.CustomDnsServersList), () => settings.IsCustomDnsServersEnabled},

            {nameof(ISettings.IsPortForwardingEnabled), () =>
                !settings.IsPortForwardingEnabled && // May need to reconnect to enable port forwarding only              
                connectionManager.IsConnected &&
                connectionManager.CurrentConnectionIntent?.IsPortForwardingSupported() == true && // May need to reconnect if the current intent could support PF but...
                connectionManager.CurrentConnectionDetails?.IsP2P != true // ...is not already connected to a P2P server
            },
        };
    }

    public bool IsReconnectionRequired(string settingName)
    {
        return _settings.ContainsKey(settingName) && _settings[settingName]();
    }
}