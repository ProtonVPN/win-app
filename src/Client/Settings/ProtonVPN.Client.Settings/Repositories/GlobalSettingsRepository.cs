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

using System.Collections.Concurrent;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Settings.Files;
using ProtonVPN.Client.Settings.Repositories.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Serialization.Contracts;

namespace ProtonVPN.Client.Settings.Repositories;

public class GlobalSettingsRepository : SettingsRepositoryBase, IGlobalSettingsRepository
{
    private const string FILE_NAME = "GlobalSettings.json";

    private readonly object _lock = new();
    private readonly ISettingsFileManager _settingsFileManager;
    private readonly Lazy<ConcurrentDictionary<string, string?>> _cache;

    public GlobalSettingsRepository(ILogger logger,
        IJsonSerializer jsonSerializer,
        IEventMessageSender eventMessageSender,
        ISettingsFileManager settingsFileManager)
        : base(logger, jsonSerializer, eventMessageSender)
    {
        _settingsFileManager = settingsFileManager;
        _cache = new Lazy<ConcurrentDictionary<string, string?>>(() => new(_settingsFileManager.Read(FILE_NAME)));
    }

    protected override string? Get(string propertyName)
    {
        string? result;
        result = _cache.Value.TryGetValue(propertyName, out string? value) ? value : null;
        return result;
    }

    protected override void Set(string propertyName, string? value)
    {
        lock (_lock)
        {
            _cache.Value.AddOrUpdate(propertyName, value, (_, _) => value);
            _settingsFileManager.Save(FILE_NAME, _cache.Value);
        }
    }
}