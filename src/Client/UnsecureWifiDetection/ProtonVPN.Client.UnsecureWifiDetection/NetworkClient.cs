/*
 * Copyright (c) 2024 Proton AG
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
using ProtonVPN.Client.UnsecureWifiDetection.Contracts;

namespace ProtonVPN.Client.UnsecureWifiDetection;

public class NetworkClient : INetworkClient
{
    private string _ssid = string.Empty;

    public NetworkClient()
    {
        if (Prelink())
        {
            NetworkChange.NetworkAddressChanged += NetworkAddressChanged;
        }
    }

    public event EventHandler<WifiChangeEventArgs>? WifiChangeDetected;

    public void CheckForInsecureWiFi()
    {
        NetworkAddressChanged(this, EventArgs.Empty);
    }

    private void NetworkAddressChanged(object? sender, EventArgs e)
    {
        IReadOnlyCollection<WifiConnection> activeWifiConnections = WlanClient.GetActiveWifiConnections();
        foreach (WifiConnection connection in activeWifiConnections)
        {
            if (_ssid == connection.Name)
            {
                continue;
            }

            _ssid = connection.Name;
            WifiChangeDetected?.Invoke(this, new WifiChangeEventArgs(connection.Name, connection.IsSecure));
        }

        bool insecureWifi = activeWifiConnections.Any(i => !i.IsSecure);
        if (!insecureWifi)
        {
            _ssid = string.Empty;
        }
    }

    private bool Prelink()
    {
        try
        {
            WlanClient.Prelink();
            return true;
        }
        catch (DllNotFoundException)
        {
            return false;
        }
    }
}