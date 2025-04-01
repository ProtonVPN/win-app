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

using Microsoft.Win32;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.Logging.Contracts.Events.OperatingSystemLogs;
using ProtonVPN.OperatingSystems.Registries.Contracts;

namespace ProtonVPN.OperatingSystems.Registries;
public class RegistryEditor : IRegistryEditor
{
    private readonly ILogger _logger;

    public RegistryEditor(ILogger logger)
    {
        _logger = logger;
    }

    public object? ReadObject(RegistryUri uri)
    {
        try
        {
            using RegistryKey? key = OpenBaseKey(uri.HiveKey).OpenSubKey(uri.Path);
            return key?.GetValue(uri.Key);
        }
        catch (Exception ex)
        {
            _logger.Error<OperatingSystemRegistryAccessFailedLog>($"Failed to read registry {uri}.", ex);
            return null;
        }
    }

    public int? ReadInt(RegistryUri uri)
    {
        object? value = ReadObject(uri);
        if (value == null)
        {
            return null;
        }

        if (value is int intValue)
        {
            return intValue;
        }

        if (int.TryParse(value.ToString(), out int parsedValue))
        {
            return parsedValue;
        }

        _logger.Error<AppLog>($"Failed to parse registry value '{value}' to int.");
        return null;
    }

    public string? ReadString(RegistryUri uri)
    {
        object? value = ReadObject(uri);
        return value?.ToString();
    }

    private RegistryKey OpenBaseKey(RegistryHive registryHive)
    {
        return RegistryKey.OpenBaseKey(registryHive, RegistryView.Registry64);
    }

    public bool WriteInt(RegistryUri uri, int value)
    {
        return WriteObject(uri, value, RegistryValueKind.DWord);
    }

    public bool WriteString(RegistryUri uri, string value)
    {
        return WriteObject(uri, value, RegistryValueKind.String);
    }

    private bool WriteObject(RegistryUri uri, object value, RegistryValueKind valueKind)
    {
        try
        {
            using RegistryKey? key = OpenBaseKey(uri.HiveKey).CreateSubKey(uri.Path, writable: true);
            if (key == null)
            {
                _logger.Error<OperatingSystemRegistryAccessFailedLog>($"Failed to open registry path before writing to {uri}.");
                return false;
            }

            key.SetValue(uri.Key, value, valueKind);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error<OperatingSystemRegistryAccessFailedLog>($"Failed to write {value} to {uri}.", ex);
            return false;
        }
    }

    public bool? Delete(RegistryUri uri)
    {
        try
        {
            using RegistryKey? key = OpenBaseKey(uri.HiveKey).OpenSubKey(uri.Path, RegistryKeyPermissionCheck.ReadWriteSubTree);
            if (key == null)
            {
                return null;
            }

            key.DeleteValue(uri.Key, throwOnMissingValue: false);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error<OperatingSystemRegistryAccessFailedLog>($"Failed deleting registry {uri}.", ex);
            return false;
        }
    }

    public Dictionary<string, string> ReadAll(RegistryUri uri)
    {
        var result = new Dictionary<string, string>();

        try
        {
            using RegistryKey? key = OpenBaseKey(uri.HiveKey).OpenSubKey(uri.Path);
            if (key != null)
            {
                foreach (string valueName in key.GetValueNames())
                {
                    object? value = key.GetValue(valueName);
                    result[valueName] = ConvertRegistryValueToString(value);
                }
            }
            else
            {
                _logger.Error<OperatingSystemRegistryAccessFailedLog>($"Failed to open registry path before reading all values: {uri}.");
            }
        }
        catch (Exception ex)
        {
            _logger.Error<OperatingSystemRegistryAccessFailedLog>($"Failed to read all registry values from {uri}.", ex);
        }

        return result;
    }

    private static string ConvertRegistryValueToString(object? value)
    {
        return value switch
        {
            null => "NULL",
            string str => str,
            int i => i.ToString(),
            long l => l.ToString(),
            byte[] bytes => BitConverter.ToString(bytes), // Converts bytes to a hex string: "AA-BB-CC"
            _ => value.ToString() ?? "UNKNOWN"
        };
    }
}