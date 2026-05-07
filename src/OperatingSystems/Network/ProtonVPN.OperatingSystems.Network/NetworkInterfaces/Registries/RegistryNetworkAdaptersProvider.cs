/*
 * Copyright (c) 2026 Proton AG
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

namespace ProtonVPN.OperatingSystems.Network.NetworkInterfaces.Registries;

public class RegistryNetworkAdaptersProvider : IRegistryNetworkAdaptersProvider 
{
    private const string CLASS_PATH = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}";

    private readonly ILogger _logger;

    public RegistryNetworkAdaptersProvider(ILogger logger)
    {
        _logger = logger;
    }

    public List<RegistryNetworkAdapter> Get()
    {
        List<RegistryNetworkAdapter> result = [];

        try
        {
            using RegistryKey? adaptersKey = Registry.LocalMachine.OpenSubKey(CLASS_PATH);
            if (adaptersKey == null)
            {
                return [];
            }

            foreach (string adapterSubKeyName in adaptersKey.GetSubKeyNames())
            {
                RegistryNetworkAdapter? adapter = EvaluateSubKey(adaptersKey, adapterSubKeyName);
                if (adapter is not null)
                {
                    result.Add(adapter);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error<AppLog>("Exception when fetching network adapters from the registry.", ex);
            return [];
        }

        return result;
    }

    private static RegistryNetworkAdapter? EvaluateSubKey(RegistryKey adaptersKey, string subKeyName)
    {
        // Subkeys are numbers (0000, 0001, ...)
        if (!int.TryParse(subKeyName, out _))
        {
            return null;
        }

        using RegistryKey? adapterSubKey = adaptersKey.OpenSubKey(subKeyName);
        if (adapterSubKey is null)
        {
            return null;
        }

        string? instanceId = ReadRegistryString(adapterSubKey, "NetCfgInstanceId");
        string? serviceName = ReadRegistryString(adapterSubKey, "Service");
        string? componentId = ReadRegistryString(adapterSubKey, "ComponentId");
        string? driverDesc = ReadRegistryString(adapterSubKey, "DriverDesc");
        string? providerName = ReadRegistryString(adapterSubKey, "ProviderName");
        string? driverFileName = ReadRegistryString(adapterSubKey, "DriverFileName");

        if (string.IsNullOrEmpty(driverFileName))
        {
            using RegistryKey? ndiSubKey = adapterSubKey.OpenSubKey("Ndi");
            if (ndiSubKey is not null)
            {
                string? ndiService = ReadRegistryString(ndiSubKey, "Service");
                driverFileName = GetDriverFileNameFromService(ndiService);

                if (string.IsNullOrEmpty(serviceName))
                {
                    serviceName = ndiService;
                }
            }
        }

        if (string.IsNullOrEmpty(driverFileName))
        {
            driverFileName = GetDriverFileNameFromService(serviceName);
        }

        return new RegistryNetworkAdapter
        {
            NetCfgInstanceId = instanceId,
            ServiceName = serviceName,
            ComponentId = componentId,
            DriverDescription = driverDesc,
            DriverProvider = providerName,
            DriverFileName = driverFileName,
        };
    }

    private static string? ReadRegistryString(RegistryKey key, string name)
    {
        object? value = key.GetValue(name);
        return value is null ? null : value as string;
    }

    private static string? GetDriverFileNameFromService(string? serviceName)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            return null;
        }

        string keyPath = $@"SYSTEM\CurrentControlSet\Services\{serviceName}";
        using RegistryKey? serviceSubKey = Registry.LocalMachine.OpenSubKey(keyPath);
        if (serviceSubKey is null)
        {
            return null;
        }

        string? imagePath = ReadRegistryString(serviceSubKey, "ImagePath");
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            return null;
        }

        try
        {
            return Path.GetFileName(imagePath);
        }
        catch
        {
            return imagePath;
        }
    }
}
