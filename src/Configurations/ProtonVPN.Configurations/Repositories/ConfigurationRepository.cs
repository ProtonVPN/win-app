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

using System.Runtime.CompilerServices;
using ProtonVPN.Configurations.Files;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.SettingsLogs;
using ProtonVPN.Serialization.Contracts.Json;

namespace ProtonVPN.Configurations.Repositories;

public class ConfigurationRepository : IConfigurationRepository
{
    private readonly ILogger _logger;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IConfigurationFileManager _fileManager;
    private readonly Lazy<IDictionary<string, string?>> _cache;

    public ConfigurationRepository(ILogger logger,
        IJsonSerializer jsonSerializer,
        IConfigurationFileManager fileManager)
    {
        _logger = logger;
        _jsonSerializer = jsonSerializer;
        _fileManager = fileManager;
        _cache = CreateCache();
    }

    private Lazy<IDictionary<string, string?>> CreateCache()
    {
#if DEBUG
        return new Lazy<IDictionary<string, string?>>(() => _fileManager.Read());
#else
        return new(new Dictionary<string, string?>());
#endif
    }

    public T? GetValueType<T>([CallerMemberName] string propertyName = "")
        where T : struct
    {
        try
        {
            string? json = Get(propertyName);
            return json is null ? null : Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            _logger.Error<SettingsLog>($"Failed to read the configuration '{propertyName}'.", ex);
            return null;
        }
    }

    private string? Get(string propertyName)
    {
        string? result;
        result = _cache.Value.TryGetValue(propertyName, out string? value) ? value : null;
        return result;
    }

    public T? GetReferenceType<T>([CallerMemberName] string propertyName = "")
        where T : class
    {
        try
        {
            string? json = Get(propertyName);
            return json is null ? null : Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            _logger.Error<SettingsLog>($"Failed to read the configuration '{propertyName}'.", ex);
            return null;
        }
    }

    public List<T> GetListValueType<T>([CallerMemberName] string propertyName = "")
        where T : struct
    {
        try
        {
            string? json = Get(propertyName);
            return Deserialize<List<T>>(json ?? string.Empty) ?? new();
        }
        catch (Exception ex)
        {
            _logger.Error<SettingsLog>($"Failed to read the configuration '{propertyName}'.", ex);
            return new();
        }
    }

    private T? Deserialize<T>(string json)
    {
        return _jsonSerializer.DeserializeFromString<T>(json);
    }

    public object? GetByType(Type type, [CallerMemberName] string propertyName = "")
    {
        try
        {
            string? json = Get(propertyName);
            return json is null ? null : Deserialize(json, type);
        }
        catch (Exception ex)
        {
            _logger.Error<SettingsLog>($"Failed to read the configuration '{propertyName}'.", ex);
            return null;
        }
    }

    private object? Deserialize(string json, Type type)
    {
        return _jsonSerializer.DeserializeFromStringAndType(json, type);
    }
}