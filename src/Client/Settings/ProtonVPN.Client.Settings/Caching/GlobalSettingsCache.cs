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

using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Settings.Files;
using ProtonVPN.Client.Settings.Repositories.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Serialization.Contracts.Json;

namespace ProtonVPN.Client.Settings.Repositories;

public class GlobalSettingsCache : SettingsCacheBase, IGlobalSettingsCache
{
    public GlobalSettingsCache(ILogger logger,
        IJsonSerializer jsonSerializer,
        IEventMessageSender eventMessageSender,
        IGlobalSettingsFileReaderWriter globalSettingsFileReaderWriter)
        : base(logger, jsonSerializer, eventMessageSender, globalSettingsFileReaderWriter)
    {
    }
}