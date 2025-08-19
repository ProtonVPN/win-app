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

public class SettingsRestorer : ISettingsRestorer
{
    private readonly ISettings _settings;

    public SettingsRestorer(ISettings settings)
    {
        _settings = settings;
    }

    public void Restore()
    {
        // Note: Some settings should not be restored, such as Language, Theme, Share statistics...

        _settings.IsNetShieldEnabled = DefaultSettings.IsNetShieldEnabled(_settings.VpnPlan.IsPaid);
        _settings.IsLocalAreaNetworkAccessEnabled = DefaultSettings.IsLocalAreaNetworkAccessAllowed(_settings.VpnPlan.IsPaid);
        _settings.NetShieldMode = DefaultSettings.NetShieldMode;
        _settings.IsKillSwitchEnabled = DefaultSettings.IsKillSwitchEnabled;
        _settings.KillSwitchMode = DefaultSettings.KillSwitchMode;
        _settings.IsPortForwardingEnabled = DefaultSettings.IsPortForwardingEnabled;
        _settings.IsPortForwardingNotificationEnabled = DefaultSettings.IsPortForwardingNotificationEnabled;
        _settings.IsSplitTunnelingEnabled = DefaultSettings.IsSplitTunnelingEnabled;
        _settings.SplitTunnelingMode = DefaultSettings.SplitTunnelingMode;
        _settings.SplitTunnelingStandardAppsList = DefaultSettings.SplitTunnelingAppsList();
        _settings.SplitTunnelingInverseAppsList = DefaultSettings.SplitTunnelingAppsList();
        _settings.SplitTunnelingStandardIpAddressesList = DefaultSettings.SplitTunnelingIpAddressesList;
        _settings.SplitTunnelingInverseIpAddressesList = DefaultSettings.SplitTunnelingIpAddressesList;
        _settings.VpnProtocol = DefaultSettings.VpnProtocol;
        _settings.NatType = DefaultSettings.NatType;
        _settings.IsVpnAcceleratorEnabled = DefaultSettings.IsVpnAcceleratorEnabled;
        _settings.IsAlternativeRoutingEnabled = DefaultSettings.IsAlternativeRoutingEnabled;
        _settings.IsCustomDnsServersEnabled = DefaultSettings.IsCustomDnsServersEnabled;
        _settings.CustomDnsServersList = DefaultSettings.CustomDnsServersList;
        _settings.OpenVpnAdapter = DefaultSettings.OpenVpnAdapter;
        _settings.IsIpv6LeakProtectionEnabled = DefaultSettings.IsIpv6LeakProtectionEnabled;
        _settings.IsSmartReconnectEnabled = DefaultSettings.IsSmartReconnectEnabled;
        _settings.DefaultConnection = DefaultSettings.DefaultConnection;
        _settings.WireGuardConnectionTimeout = DefaultSettings.WireGuardConnectionTimeout;
        _settings.IsIpv6Enabled = DefaultSettings.IsIpv6Enabled;
    }
}