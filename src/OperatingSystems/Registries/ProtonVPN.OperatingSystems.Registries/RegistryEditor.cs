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
        RegistryKey? key = null;
        try
        {
            key = OpenBaseKey(uri.HiveKey).OpenSubKey(uri.Path);
            if (key != null)
            {
                return key.GetValue(uri.Key);
            }
            _logger.Error<OperatingSystemRegistryAccessFailedLog>($"Failed to open registry path before reading {uri}.");
        }
        catch (Exception ex)
        {
            _logger.Error<OperatingSystemRegistryAccessFailedLog>($"Failed to read registry {uri}.", ex);
        }
        finally
        {
            key?.Close();
        }
        return null;
    }

    public int? ReadInt(RegistryUri uri)
    {
        object? registryValue = ReadObject(uri);
        if (registryValue is not null)
        {
            int result;
            try
            {
                result = (int)registryValue;
                return result;
            }
            catch
            {
            }
            string? stringValue = null;
            try
            {
                stringValue = registryValue.ToString();
                if (int.TryParse(stringValue, out result))
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.Error<AppLog>($"Failed when parsing registry value '{stringValue}' to int.", ex);
            }
        }
        return null;
    }

    private RegistryKey OpenBaseKey(RegistryHive registryHive)
    {
        return RegistryKey.OpenBaseKey(registryHive, RegistryView.Registry64);
    }

    public string? ReadString(RegistryUri uri)
    {
        object? registryValue = ReadObject(uri);
        if (registryValue is not null)
        {
            string result;
            try
            {
                result = (string)registryValue;
                return result;
            }
            catch
            {
                return registryValue.ToString();
            }
        }
        return null;
    }

    public bool WriteInt(RegistryUri uri, int value)
    {
        return WriteObject(uri, value, RegistryValueKind.DWord);
    }

    private bool WriteObject(RegistryUri uri, object value, RegistryValueKind valueKind)
    {
        bool isSuccess = false;
        RegistryKey key = OpenBaseKey(uri.HiveKey).CreateSubKey(uri.Path, writable: true);
        if (key == null)
        {
            string errorMessage = $"Failed to open registry path before writing to {uri}.";
            _logger.Error<OperatingSystemRegistryAccessFailedLog>(errorMessage);
        }
        else
        {
            try
            {
                key.SetValue(uri.Key, value, valueKind);
                isSuccess = true;
            }
            catch (Exception ex)
            {
                _logger.Error<OperatingSystemRegistryAccessFailedLog>($"Failed when writing {value} to {uri}", ex);
            }
            finally
            {
                key?.Close();
            }
        }
        return isSuccess;
    }

    public bool WriteString(RegistryUri uri, string value)
    {
        return WriteObject(uri, value, RegistryValueKind.String);
    }

    public bool? Delete(RegistryUri uri)
    {
        bool? isSuccess = null;
        RegistryKey? key = OpenBaseKey(uri.HiveKey).OpenSubKey(uri.Path, RegistryKeyPermissionCheck.ReadWriteSubTree);
        if (key != null)
        {
            try
            {
                key.DeleteValue(uri.Key, throwOnMissingValue: false);
                isSuccess = true;
            }
            catch (Exception ex)
            {
                isSuccess = false;
                _logger.Error<OperatingSystemRegistryAccessFailedLog>($"Failed deleting registry {uri}.", ex);
            }
            finally
            {
                key?.Close();
            }
        }
        return isSuccess;
    }
}