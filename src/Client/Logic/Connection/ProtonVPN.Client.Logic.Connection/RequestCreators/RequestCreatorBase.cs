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

using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.RequestCreators;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Observers;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Connection.RequestCreators;

public abstract class RequestCreatorBase
{
    protected readonly ILogger Logger;
    protected readonly ISettings Settings;
    protected readonly IEntityMapper EntityMapper;
    protected readonly IFeatureFlagsObserver FeatureFlagsObserver;

    private readonly IMainSettingsRequestCreator _mainSettingsRequestCreator;

    protected RequestCreatorBase(
        ILogger logger,
        ISettings settings,
        IEntityMapper entityMapper,
        IFeatureFlagsObserver featureFlagsObserver,
        IMainSettingsRequestCreator mainSettingsRequestCreator)
    {
        Logger = logger;
        Settings = settings;
        EntityMapper = entityMapper;
        FeatureFlagsObserver = featureFlagsObserver;

        _mainSettingsRequestCreator = mainSettingsRequestCreator;
    }

    protected MainSettingsIpcEntity GetSettings(IConnectionIntent? connectionIntent = null)
    {
        MainSettingsIpcEntity settings = _mainSettingsRequestCreator.Create();

        if (connectionIntent is not null && connectionIntent is IConnectionProfile connectionProfile)
        {
            settings.VpnProtocol = EntityMapper.Map<VpnProtocol, VpnProtocolIpcEntity>(connectionProfile.Settings.Protocol);
        }

        if (!FeatureFlagsObserver.IsStealthEnabled && 
            (settings.VpnProtocol is VpnProtocolIpcEntity.WireGuardTls or VpnProtocolIpcEntity.WireGuardTcp))
        {
            Logger.Warn<AppLog>("Stealth protocol is currently disabled. Switch to Smart protocol instead.");

            settings.VpnProtocol = EntityMapper.Map<VpnProtocol, VpnProtocolIpcEntity>(DefaultSettings.VpnProtocol);
        }

        return settings;
    }
}