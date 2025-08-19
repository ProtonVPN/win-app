/*
 * Copyright (c) 2025 Proton AG
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

using System;
using System.Collections.Generic;
using System.Linq;
using ProtonVPN.OperatingSystems.Network.Contracts;

namespace ProtonVPN.Service.Vpn;

public class ObservableNetworkInterfaces : IObservableNetworkInterfaces
{
    private readonly ISystemNetworkInterfaces _networkInterfaces;

    private List<string> _interfaces = [];

    public ObservableNetworkInterfaces(ISystemNetworkInterfaces networkInterfaces)
    {
        _networkInterfaces = networkInterfaces;

        networkInterfaces.NetworkAddressChanged += NetworkInterfaces_NetworkAddressChanged;
    }

    public event EventHandler NetworkInterfacesAdded;

    private void NetworkInterfaces_NetworkAddressChanged(object sender, EventArgs e)
    {
        INetworkInterface[] adapters = _networkInterfaces.GetInterfaces();
        if (adapters.Length == 0)
        {
            return;
        }

        List<string> list = adapters.Where(a => !a.IsLoopback).Select(a => a.Id).ToList();

        if (_interfaces.Count != 0 && list.Except(_interfaces).Any())
        {
            NetworkInterfacesAdded?.Invoke(this, EventArgs.Empty);
        }

        _interfaces = list;
    }
}