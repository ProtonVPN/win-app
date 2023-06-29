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
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Client.Settings.Repositories.Contracts;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Crypto.Extensions;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.SettingsLogs;
using ProtonVPN.Serialization.Json;

namespace ProtonVPN.Client.Settings.Repositories;

public class SettingsRepository : ISettingsRepository
{
    private readonly IGlobalSettingsRepository _globalSettingsRepository;
    private readonly IUserSettingsRepository _userSettingsRepository;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly ILogger _logger;

    public SettingsRepository(
        IGlobalSettingsRepository globalSettingsRepository,
        IUserSettingsRepository userSettingsRepository,
        IEventMessageSender eventMessageSender,
        ILogger logger)
    {
        _globalSettingsRepository = globalSettingsRepository;
        _userSettingsRepository = userSettingsRepository;
        _eventMessageSender = eventMessageSender;
        _logger = logger;
    }

    public T? GetValueType<T>(string propertyName, SettingScope scope, SettingEncryption encryption)
        where T : struct
    {
        try
        {
            string? json = GetJson(propertyName, scope, encryption);
            return json is null ? null : Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            _logger.Error<SettingsLog>($"Failed to read the setting '{propertyName}'.", ex);
            return null;
        }
    }

    private string? GetJson(string propertyName, SettingScope scope, SettingEncryption encryption)
    {
        return encryption switch
        {
            SettingEncryption.Unencrypted => GetUnencrypted(scope, propertyName),
            SettingEncryption.Encrypted => GetEncrypted(scope, propertyName),
            _ => throw new NotImplementedException($"The encryption {encryption} is not implemented."),
        };
    }

    public T? GetReferenceType<T>(string propertyName, SettingScope scope, SettingEncryption encryption)
        where T : class
    {
        try
        {
            string? json = GetJson(propertyName, scope, encryption);
            return json is null ? null : Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            _logger.Error<SettingsLog>($"Failed to read the setting '{propertyName}'.", ex);
            return null;
        }
    }

    private T? Deserialize<T>(string json)
    {
        Type type = typeof(T);
        if (type == typeof(string))
        {
            return (dynamic)json;
        }
        if (type.IsEnum && Enum.TryParse(type, json, out dynamic? result))
        {
            return result;
        }
        return JsonSerializer.Deserialize<T>(json);
    }

    private string? GetUnencrypted(SettingScope scope, string propertyName)
    {
        return scope switch
        {
            SettingScope.Global => _globalSettingsRepository.Get(propertyName),
            SettingScope.User => _userSettingsRepository.Get(propertyName),
            _ => throw new NotImplementedException($"The scope {scope} is not implemented."),
        };
    }

    private string? GetEncrypted(SettingScope scope, string propertyName)
    {
        string? encryptedJson = scope switch
        {
            SettingScope.Global => _globalSettingsRepository.Get(propertyName),
            SettingScope.User => _userSettingsRepository.Get(propertyName),
            _ => throw new NotImplementedException($"The scope {scope} is not implemented."),
        };
        return encryptedJson?.Decrypt();
    }

    public void SetValueType<T>(string propertyName, T? newValue, SettingScope scope, SettingEncryption encryption)
        where T : struct
    {
        try
        {
            T? oldValue = default;
            Type? toType = UnwrapNullable(typeof(T));
            if (IsValueTypeOrString(toType))
            {
                oldValue = GetValueType<T>(propertyName, scope, encryption);
                if (Equals(oldValue, newValue))
                {
                    return;
                }
            }

            Set(propertyName, oldValue, newValue, scope, encryption);
        }
        catch (Exception ex)
        {
            _logger.Error<SettingsLog>($"Failed to write the setting '{propertyName}'.", ex);
        }
    }

    public void SetReferenceType<T>(string propertyName, T? newValue, SettingScope scope, SettingEncryption encryption)
        where T : class
    {
        try
        {
            T? oldValue = default;
            Type? toType = UnwrapNullable(typeof(T));
            if (IsValueTypeOrString(toType))
            {
                oldValue = GetReferenceType<T>(propertyName, scope, encryption);
                if (EqualityComparer<T>.Default.Equals(oldValue, newValue))
                {
                    return;
                }
            }

            Set(propertyName, oldValue, newValue, scope, encryption);
        }
        catch (Exception ex)
        {
            _logger.Error<SettingsLog>($"Failed to write the setting '{propertyName}'.", ex);
        }
    }

    private void Set<T>(string propertyName, T? oldValue, T? newValue, SettingScope scope, SettingEncryption encryption)
    {
        string? json = newValue is null ? null : Serialize(newValue);
        if (encryption is SettingEncryption.Encrypted)
        {
            json = json?.Encrypt();
        }
        SetScoped(propertyName, json, scope);

        OnPropertyChanged(oldValue, newValue, propertyName);
        LogChange(propertyName, oldValue, newValue);
    }

    private Type? UnwrapNullable(Type type)
    {
        return IsNullableType(type) ? Nullable.GetUnderlyingType(type) : type;
    }

    private bool IsValueTypeOrString(Type? toType)
    {
        return toType is not null && (toType.IsValueType || toType == typeof(string));
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
        return JsonSerializer.Serialize(newValue);
    }

    private void SetScoped(string propertyName, string? json, SettingScope scope)
    {
        switch (scope)
        {
            case SettingScope.Global:
                _globalSettingsRepository.Set(propertyName, json);
                break;
            case SettingScope.User:
                _userSettingsRepository.Set(propertyName, json);
                break;
            default:
                throw new NotImplementedException($"The scope {scope} is not implemented.");
        };
    }

    private void OnPropertyChanged<T>(T? oldValue, T? newValue, string propertyName)
    {
        _eventMessageSender.Send(new SettingChangedMessage(propertyName, typeof(T), oldValue, newValue));
    }

    private void LogChange<T>(string propertyName, T oldValue, T newValue)
    {
        string oldValueJson = JsonSerializer.Serialize(oldValue).GetLastChars(64);
        string newValueJson = JsonSerializer.Serialize(newValue).GetLastChars(64);
        _logger.Info<SettingsChangeLog>($"Setting '{propertyName}' " +
            $"changed from '{oldValueJson}' to '{newValueJson}'.");
    }
}