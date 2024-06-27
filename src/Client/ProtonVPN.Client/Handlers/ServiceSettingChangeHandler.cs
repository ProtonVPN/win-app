﻿/*
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

using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;

namespace ProtonVPN.Client.Handlers;

public class ServiceSettingChangeHandler : IHandler, IEventMessageReceiver<SettingChangedMessage>
{
    // Only individual settings used by MainSettingsRequestCreator. Groups of Settings should have a
    // single call made at the end of all changes (Ex.: KillSwitchViewModel, SplitTunnelingViewModel).
    private readonly List<string> _settingNames =
    [
        nameof(ISettings.VpnProtocol),
        nameof(ISettings.NatType),
        nameof(ISettings.IsNetShieldEnabled),
        nameof(ISettings.IsIpv6LeakProtectionEnabled),
        nameof(ISettings.IsPortForwardingEnabled),
        nameof(ISettings.IsVpnAcceleratorEnabled),
        nameof(ISettings.OpenVpnAdapter),
        nameof(ISettings.IsShareCrashReportsEnabled),
    ];

    private readonly IVpnServiceSettingsUpdater _vpnServiceSettingsUpdater;

    public ServiceSettingChangeHandler(IVpnServiceSettingsUpdater vpnServiceSettingsUpdater)
    {
        _vpnServiceSettingsUpdater = vpnServiceSettingsUpdater;
    }

    public async void Receive(SettingChangedMessage message)
    {
        if (_settingNames.Contains(message.PropertyName))
        {
            await _vpnServiceSettingsUpdater.SendAsync();
        }
    }
}