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

using ProtonVPN.Common.Legacy.Abstract;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Auth;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Services.Contracts;

public interface IVpnServiceCaller
{
    Task RegisterClientAsync(int appServerPort, CancellationToken cancellationToken);

    Task ConnectAsync(ConnectionRequestIpcEntity connectionRequest);

    Task DisconnectAsync(DisconnectionRequestIpcEntity connectionRequest);

    Task<Result<TrafficBytesIpcEntity>> GetTrafficBytesAsync();

    Task UpdateAuthCertificateAsync(AuthCertificateIpcEntity certificate);

    Task ApplySettingsAsync(MainSettingsIpcEntity settings);
}