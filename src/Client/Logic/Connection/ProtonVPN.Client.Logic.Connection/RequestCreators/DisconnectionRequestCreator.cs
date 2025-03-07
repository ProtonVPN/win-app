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

using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.RequestCreators;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Connection.RequestCreators;

public class DisconnectionRequestCreator : RequestCreatorBase, IDisconnectionRequestCreator
{
    public DisconnectionRequestCreator(
        ILogger logger,
        ISettings settings,
        IEntityMapper entityMapper,
        IMainSettingsRequestCreator mainSettingsRequestCreator)
        : base(logger, settings, entityMapper, mainSettingsRequestCreator)
    { }

    public DisconnectionRequestIpcEntity Create(VpnError vpnError = VpnError.None)
    {
        MainSettingsIpcEntity settings = GetSettings();

        DisconnectionRequestIpcEntity request = new()
        {
            RetryId = Guid.NewGuid(),
            Settings = settings,
            ErrorType = EntityMapper.Map<VpnError, VpnErrorTypeIpcEntity>(vpnError)
        };

        return request;
    }
}