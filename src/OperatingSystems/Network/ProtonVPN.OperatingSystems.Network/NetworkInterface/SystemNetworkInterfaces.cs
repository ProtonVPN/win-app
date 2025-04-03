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

using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.NetworkLogs;
using ProtonVPN.OperatingSystems.Network.Contracts;

namespace ProtonVPN.OperatingSystems.Network.NetworkInterface;

public class SystemNetworkInterfaces : ISystemNetworkInterfaces
{
    private readonly ILogger _logger;
    private readonly INetworkUtilities _networkUtilities;

    public event EventHandler? NetworkAddressChanged;

    public SystemNetworkInterfaces(ILogger logger, INetworkUtilities networkUtilities)
    {
        _logger = logger;
        _networkUtilities = networkUtilities;

        NetworkChange.NetworkAddressChanged += (s, e) => NetworkAddressChanged?.Invoke(s, e);
    }

    public INetworkInterface[] GetInterfaces()
    {
        try
        {
            System.Net.NetworkInformation.NetworkInterface[] interfaces =
                System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();

            return interfaces
                .Select(i => new SystemNetworkInterface(i))
                .Cast<INetworkInterface>()
                .ToArray();
        }
        catch (NetworkInformationException ex)
        {
            _logger.Error<NetworkUnavailableLog>($"Failed to retrieve a system network interface.", ex);
            return [];
        }
    }

    public INetworkInterface GetByDescription(string description)
    {
        return TryGet(() =>
        {
            return GetInterfaces().FirstOrDefault(i => i.Description.Contains(description))
                ?? new NullNetworkInterface();
        });
    }

    public INetworkInterface GetByLocalAddress(IPAddress localAddress)
    {
        return TryGet(() =>
        {
            System.Net.NetworkInformation.NetworkInterface[] interfaces =
            System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();

            foreach (System.Net.NetworkInformation.NetworkInterface i in interfaces)
            {
                if (i.GetIPProperties().UnicastAddresses.FirstOrDefault(a => a.Address.Equals(localAddress)) != null)
                {
                    return new SystemNetworkInterface(i);
                }
            }

            return new NullNetworkInterface();
        });
    }

    public INetworkInterface GetBestInterface(string hardwareIdToExclude)
    {
        return TryGet(() =>
        {
            return GetByLocalAddress(_networkUtilities.GetBestInterfaceIp(hardwareIdToExclude));
        });
    }

    public INetworkInterface GetByName(string name)
    {
        return TryGet(() =>
        {
            return GetInterfaces().FirstOrDefault(i => i.Name == name) ?? new NullNetworkInterface();
        });
    }

    public INetworkInterface GetById(Guid id)
    {
        return TryGet(() =>
        {
            return GetInterfaces().FirstOrDefault(i => AreIdsEqual(i.Id, id)) ?? new NullNetworkInterface();
        });
    }

    private bool AreIdsEqual(string stringId, Guid id)
    {
        return Guid.TryParse(stringId, out Guid result) && result == id;
    }

    public NetworkConnectionType? GetNetworkConnectionType()
    {
        try
        {
            System.Net.NetworkInformation.NetworkInterface[] activeInterfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                             (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                              ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211))
                .ToArray();

            foreach (System.Net.NetworkInformation.NetworkInterface networkInterface in activeInterfaces)
            {
                IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();
                bool hasDefaultGateway = ipProperties.GatewayAddresses.Any(g =>
                    g.Address.AddressFamily == AddressFamily.InterNetwork &&
                    !g.Address.Equals(IPAddress.Any));

                if (!hasDefaultGateway)
                {
                    continue;
                }

                if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    return NetworkConnectionType.Wired;
                }
                else if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {
                    return NetworkConnectionType.Wifi;
                }
            }
        }
        catch (NetworkInformationException ex)
        {
            _logger.Error<NetworkLog>("Failed to retreive a network connection type.", ex);
        }

        return NetworkConnectionType.Other;
    }

    private INetworkInterface TryGet(Func<INetworkInterface> func)
    {
        try
        {
            return func();
        }
        catch (NetworkInformationException ex)
        {
            _logger.Error<NetworkLog>($"Failed to retrieve a system network interface.", ex);
            return new NullNetworkInterface();
        }
    }
}