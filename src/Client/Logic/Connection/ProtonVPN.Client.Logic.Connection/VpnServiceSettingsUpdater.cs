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

using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.RequestCreators;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Connection;

public class VpnServiceSettingsUpdater : IVpnServiceSettingsUpdater
{
    private readonly IVpnServiceCaller _vpnServiceCaller;
    private readonly IMainSettingsRequestCreator _mainSettingsRequestCreator;

    public VpnServiceSettingsUpdater(IVpnServiceCaller vpnServiceCaller, IMainSettingsRequestCreator mainSettingsRequestCreator)
    {
        _vpnServiceCaller = vpnServiceCaller;
        _mainSettingsRequestCreator = mainSettingsRequestCreator;
    }

    public async Task DisableKillSwitchAsync()
    {
        MainSettingsIpcEntity settings = _mainSettingsRequestCreator.Create();
        settings.KillSwitchMode = KillSwitchModeIpcEntity.Off;
        await _vpnServiceCaller.ApplySettingsAsync(settings);
    }

    public async Task EnableAdvancedKillSwitchAsync()
    {
        MainSettingsIpcEntity settings = _mainSettingsRequestCreator.Create();
        settings.KillSwitchMode = KillSwitchModeIpcEntity.Hard;
        await _vpnServiceCaller.ApplySettingsAsync(settings);
    }
}