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
using System.Runtime.CompilerServices;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Settings.Caching;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Client.Settings.Files;
using ProtonVPN.Client.Settings.Repositories.Contracts;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Core.Helpers;
using ProtonVPN.Crypto.Contracts.Extensions;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.SettingsLogs;
using ProtonVPN.Serialization.Contracts.Json;

namespace ProtonVPN.Client.Settings.Repositories;

public abstract class SettingsCacheBase : ISettingsCache
{
    protected ILogger Logger { get; }

    private readonly IJsonSerializer _jsonSerializer;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly ISettingsFileReaderWriter _settingsFileReaderWriter;

    protected readonly ResettableLazy<ConcurrentDictionary<string, string?>> JsonCache;
    protected readonly ConcurrentDictionary<string, dynamic?> Cache;

    public SettingsCacheBase(ILogger logger,
        IJsonSerializer jsonSerializer,
        IEventMessageSender eventMessageSender,
        ISettingsFileReaderWriter settingsFileReaderWriter)
    {
        Logger = logger;
        _jsonSerializer = jsonSerializer;
        _eventMessageSender = eventMessageSender;
        _settingsFileReaderWriter = settingsFileReaderWriter;

        JsonCache = new ResettableLazy<ConcurrentDictionary<string, string?>>(() => new(_settingsFileReaderWriter.Read()));
        Cache = new();
    }

    public T? GetValueType<T>(SettingEncryption encryption, [CallerMemberName] string propertyName = "")
        where T : struct
    {
        try
        {
            return Get<T?>(encryption, propertyName, (pjp) => DeserializeAndCache<T?>(pjp, () => null));
        }
        catch (Exception ex)
        {
            Logger.Error<SettingsLog>($"Failed to read the setting '{propertyName}'.", ex);
            return null;
        }
    }

    private T? Get<T>(SettingEncryption encryption, string propertyName,
        Func<PropertyJsonPair, T?> deserializationFunction)
    {
        if (Cache.TryGetValue(propertyName, out dynamic? value))
        {
            return value;
        }

        PropertyJsonPair propertyJsonPair = new()
        {
            PropertyName = propertyName,
            JsonValue = GetJson(propertyName, encryption)
        };
        return deserializationFunction(propertyJsonPair);
    }

    private string? GetJson(string propertyName, SettingEncryption encryption)
    {
        return encryption switch
        {
            SettingEncryption.Unencrypted => GetUnencrypted(propertyName),
            SettingEncryption.Encrypted => GetEncrypted(propertyName),
            _ => throw new NotImplementedException($"The encryption {encryption} is not implemented."),
        };
    }

    public T? GetReferenceType<T>(SettingEncryption encryption, [CallerMemberName] string propertyName = "")
        where T : class
    {
        try
        {
            return Get<T?>(encryption, propertyName, (pjp) => DeserializeAndCache<T?>(pjp, () => null));
        }
        catch (Exception ex)
        {
            Logger.Error<SettingsLog>($"Failed to read the setting '{propertyName}'.", ex);
            return null;
        }
    }

    public List<T>? GetListValueType<T>(SettingEncryption encryption,
        [CallerMemberName] string propertyName = "")
        where T : struct
    {
        try
        {
            return Get<List<T>?>(encryption, propertyName, (pjp) => DeserializeAndCache<List<T>>(pjp, () => null));
        }
        catch (Exception ex)
        {
            Logger.Error<SettingsLog>($"Failed to read the setting '{propertyName}'.", ex);
            return new();
        }
    }

    private T? DeserializeAndCache<T>(PropertyJsonPair propertyJsonPair, Func<T?> nullJsonResult)
    {
        T? value = Deserialize<T>(propertyJsonPair, nullJsonResult);
        Cache.AddOrUpdate(propertyJsonPair.PropertyName, value, (_, _) => value);
        return value;
    }

    private T? Deserialize<T>(PropertyJsonPair propertyJsonPair, Func<T?> nullJsonResult)
    {
        if (propertyJsonPair.JsonValue is null)
        {
            return nullJsonResult();
        }

        Type type = typeof(T);
        if (type == typeof(string))
        {
            return (dynamic)propertyJsonPair.JsonValue;
        }

        if (IsEnum(type, propertyJsonPair, out dynamic? enumResult))
        {
            return enumResult;
        }

        if (IsNullableEnum(type, propertyJsonPair, out dynamic? underlyingEnumResult))
        {
            return underlyingEnumResult;
        }

        return _jsonSerializer.DeserializeFromString<T>(propertyJsonPair.JsonValue);
    }

    private bool IsEnum(Type type, PropertyJsonPair propertyJsonPair, out dynamic? enumResult)
    {
        enumResult = null;
        return type.IsEnum && Enum.TryParse(type, propertyJsonPair.JsonValue, out enumResult);
    }

    private bool IsNullableEnum(Type type, PropertyJsonPair propertyJsonPair, out dynamic? underlyingEnumResult)
    {
        underlyingEnumResult = null;
        Type? underlyingType = Nullable.GetUnderlyingType(type);
        return underlyingType != null && IsEnum(underlyingType, propertyJsonPair, out underlyingEnumResult);
    }

    private string? GetUnencrypted(string propertyName)
    {
        return Get(propertyName);
    }

    private string? Get(string propertyName)
    {
        return JsonCache.Value.TryGetValue(propertyName, out string? value) ? value : null;
    }

    private string? GetEncrypted(string propertyName)
    {
        return GetUnencrypted(propertyName)?.Decrypt();
    }

    public void SetValueType<T>(T? newValue, SettingEncryption encryption,
        [CallerMemberName] string propertyName = "")
        where T : struct
    {
        try
        {
            T? oldValue = default;
            Type? toType = UnwrapNullable(typeof(T));
            if (IsValueTypeOrString(toType))
            {
                oldValue = GetValueType<T>(encryption, propertyName);
                if (Equals(oldValue, newValue))
                {
                    return;
                }
            }

            Set(propertyName, oldValue, newValue, encryption);
        }
        catch (Exception ex)
        {
            Logger.Error<SettingsLog>($"Failed to write the setting '{propertyName}'.", ex);
        }
    }

    private bool IsValueTypeOrString(Type? toType)
    {
        return toType is not null && (toType.IsValueType || toType == typeof(string));
    }

    public void SetReferenceType<T>(T? newValue, SettingEncryption encryption,
        [CallerMemberName] string propertyName = "")
        where T : class
    {
        try
        {
            T? oldValue = default;
            Type? toType = UnwrapNullable(typeof(T));

            oldValue = GetReferenceType<T>(encryption, propertyName);
            if (EqualityComparer<T>.Default.Equals(oldValue, newValue))
            {
                return;
            }

            Set(propertyName, oldValue, newValue, encryption);
        }
        catch (Exception ex)
        {
            Logger.Error<SettingsLog>($"Failed to write the setting '{propertyName}'.", ex);
        }
    }

    public void SetListValueType<T>(List<T>? newValue, SettingEncryption encryption,
        [CallerMemberName] string propertyName = "")
        where T : struct
    {
        try
        {
            List<T>? oldValue = GetListValueType<T>(encryption, propertyName);

            if ((oldValue is null && newValue is null) ||
                (oldValue is not null && newValue is not null && oldValue.SequenceEqual(newValue)))
            {
                return;
            }

            Set(propertyName, oldValue, newValue, encryption);
        }
        catch (Exception ex)
        {
            Logger.Error<SettingsLog>($"Failed to write the setting '{propertyName}'.", ex);
        }
    }

    private void Set<T>(string propertyName, T? oldValue, T? newValue, SettingEncryption encryption)
    {
        string? json = newValue is null ? null : Serialize(newValue);
        if (encryption is SettingEncryption.Encrypted)
        {
            json = json?.Encrypt();
        }
        Set(propertyName, newValue, json);

        OnPropertyChanged(oldValue, newValue, propertyName);
        LogChange(propertyName, oldValue, newValue, encryption);
    }

    private void Set<T>(string propertyName, T? newValue, string? json)
    {
        JsonCache.Value.AddOrUpdate(propertyName, json, (_, _) => json);
        _settingsFileReaderWriter.Write(JsonCache.Value);
        Cache.AddOrUpdate(propertyName, newValue, (_, _) => newValue);
    }

    private Type? UnwrapNullable(Type type)
    {
        return IsNullableType(type) ? Nullable.GetUnderlyingType(type) : type;
    }

    private bool IsNullableType(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    private string? Serialize<T>(T newValue)
    {
        if (newValue is string stringValue)
        {
            return stringValue;
        }
        if (newValue is Enum enumValue)
        {
            return enumValue.ToString();
        }
        return _jsonSerializer.SerializeToString(newValue);
    }

    private void OnPropertyChanged<T>(T? oldValue, T? newValue, string propertyName)
    {
        _eventMessageSender.Send(new SettingChangedMessage(propertyName, typeof(T), oldValue, newValue));
    }

    private void LogChange<T>(string propertyName, T oldValue, T newValue, SettingEncryption encryption)
    {
#if DEBUG
        LogChangeUnencrypted(propertyName, oldValue, newValue);
#else
        if (encryption == SettingEncryption.Encrypted)
        {
            Logger.Info<SettingsChangeLog>($"Setting '{propertyName}' changed (encrypted).");
        }
        else
        {
            LogChangeUnencrypted(propertyName, oldValue, newValue);
        }
#endif
    }

    private void LogChangeUnencrypted<T>(string propertyName, T? oldValue, T? newValue)
    {
        string oldValueJson = _jsonSerializer.SerializeToString(oldValue).GetLastChars(64);
        string newValueJson = _jsonSerializer.SerializeToString(newValue).GetLastChars(64);
        Logger.Info<SettingsChangeLog>($"Setting '{propertyName}' " +
            $"changed from '{oldValueJson}' to '{newValueJson}'.");
    }

    public List<T>? GetListReferenceType<T>(SettingEncryption encryption, [CallerMemberName] string propertyName = "")
        where T : class
    {
        try
        {
            return Get<List<T>?>(encryption, propertyName, (pjp) => DeserializeAndCache<List<T>>(pjp, () => []));
        }
        catch (Exception ex)
        {
            Logger.Error<SettingsLog>($"Failed to read the setting '{propertyName}'.", ex);
            return new();
        }
    }

    public void SetListReferenceType<T>(List<T>? newValue, SettingEncryption encryption,
        [CallerMemberName] string propertyName = "")
        where T : class
    {
        try
        {
            List<T>? oldValue = GetListReferenceType<T>(encryption, propertyName);

            if ((oldValue is null && newValue is null) ||
                (oldValue is not null && newValue is not null && oldValue.SequenceEqual(newValue)))
            {
                return;
            }

            Set(propertyName, oldValue, newValue, encryption);
        }
        catch (Exception ex)
        {
            Logger.Error<SettingsLog>($"Failed to write the setting '{propertyName}'.", ex);
        }
    }
}