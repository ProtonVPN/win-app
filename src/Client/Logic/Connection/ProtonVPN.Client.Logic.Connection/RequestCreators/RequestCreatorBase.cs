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
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;

namespace ProtonVPN.Client.Logic.Connection.RequestCreators;

public abstract class RequestCreatorBase
{
    protected readonly ILogger Logger;
    protected readonly ISettings Settings;
    protected readonly IEntityMapper EntityMapper;

    private readonly IMainSettingsRequestCreator _mainSettingsRequestCreator;

    protected RequestCreatorBase(
        ILogger logger,
        ISettings settings,
        IEntityMapper entityMapper,
        IMainSettingsRequestCreator mainSettingsRequestCreator)
    {
        Logger = logger;
        Settings = settings;
        EntityMapper = entityMapper;

        _mainSettingsRequestCreator = mainSettingsRequestCreator;
    }

    protected MainSettingsIpcEntity GetSettings(IConnectionIntent? connectionIntent = null)
    {
        return _mainSettingsRequestCreator.Create(connectionIntent);
    }
}