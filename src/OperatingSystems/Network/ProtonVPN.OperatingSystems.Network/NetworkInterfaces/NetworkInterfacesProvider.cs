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

using System.Net.NetworkInformation;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.OperatingSystems.Network.Contracts.NetworkInterfaces;
using ProtonVPN.OperatingSystems.Network.NetworkInterfaces.Registries;

namespace ProtonVPN.OperatingSystems.Network.NetworkInterfaces;

public class NetworkInterfacesProvider : INetworkInterfacesProvider
{
    private readonly ILogger _logger;
    private readonly IRegistryNetworkAdaptersProvider _registryNetworkAdaptersProvider;

    public NetworkInterfacesProvider(ILogger logger,
        IRegistryNetworkAdaptersProvider registryNetworkAdaptersProvider)
    {
        _logger = logger;
        _registryNetworkAdaptersProvider = registryNetworkAdaptersProvider;
    }

    public IReadOnlyList<NetworkInterfaceInfo> Get()
    {
        List<RegistryNetworkAdapter> registryAdapters = _registryNetworkAdaptersProvider.Get();

        NetworkInterface[] networkInterfaces;
        try
        {
            networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
        }
        catch (Exception ex)
        {
            _logger.Error<AppLog>("Exception when fetching network interfaces.", ex);
            return [];
        }

        return CreateNetworkInterfaceInfoList(registryAdapters, networkInterfaces);
    }

    private List<NetworkInterfaceInfo> CreateNetworkInterfaceInfoList(List<RegistryNetworkAdapter> registryAdapters,
        NetworkInterface[] networkInterfaces)
    {
        List<NetworkInterfaceInfo> result = [];

        foreach (NetworkInterface networkInterface in networkInterfaces)
        {
            RegistryNetworkAdapter? registryAdapter = registryAdapters.FirstOrDefault(a => a.NetCfgInstanceId == networkInterface.Id);

            NetworkInterfaceDriverInfo? driver = registryAdapter is null
                ? null
                : new()
                {
                    FileName = registryAdapter.DriverFileName,
                    Provider = registryAdapter.DriverProvider,
                    Description = registryAdapter.DriverDescription,
                    ComponentId = registryAdapter.ComponentId,
                };

            NetworkInterfaceInfo adapter = new()
            {
                Guid = networkInterface.Id,
                Name = networkInterface.Name,
                Description = networkInterface.Description,
                Type = networkInterface.NetworkInterfaceType,
                OperationalStatus = networkInterface.OperationalStatus,
                Driver = driver,
            };

            result.Add(adapter);
        }

        return result;
    }
}
