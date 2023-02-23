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

using System;
using System.Collections.Generic;
using System.Linq;
using ProtonVPN.NetworkFilter;
using Action = ProtonVPN.NetworkFilter.Action;

namespace ProtonVPN.Service.Firewall
{
    public class PermittedRemoteAddress : IFilterCollection
    {
        private readonly IpLayer _ipLayer;
        private readonly IpFilter _ipFilter;

        private readonly Dictionary<string,List<Guid>> _list = new Dictionary<string, List<Guid>>();

        public PermittedRemoteAddress(IpFilter ipFilter, IpLayer ipLayer)
        {
            _ipLayer = ipLayer;
            _ipFilter = ipFilter;
        }

        public void Add(string[] addresses, Action action)
        {
            foreach (string address in addresses)
            {
                Add(address, action);
            }
        }

        public void Add(string address, Action action)
        {
            if (_list.ContainsKey(address))
            {
                return;
            }

            var networkAddress = new Common.OS.Net.NetworkAddress(address);
            if (!networkAddress.Valid())
            {
                return;
            }

            _list[address] = new List<Guid>();

            _ipLayer.ApplyToIpv4(layer =>
            {
                Guid guid = _ipFilter.DynamicSublayer.CreateRemoteNetworkIPv4Filter(
                    new DisplayData("ProtonVPN permit remote address", ""),
                    action,
                    layer,
                    14,
                    new NetworkAddress(networkAddress.Ip, networkAddress.Mask));

                _list[address].Add(guid);
            });
        }

        public void Remove(string address)
        {
            if (!_list.ContainsKey(address))
            {
                return;
            }

            foreach (Guid guid in _list[address])
            {
                _ipFilter.DynamicSublayer.DestroyFilter(guid);
            }

            _list.Remove(address);
        }

        public void RemoveAll()
        {
            if (_list.Count == 0)
            {
                return;
            }

            foreach (KeyValuePair<string, List<Guid>> element in _list.ToList())
            {
                Remove(element.Key);
            }
        }
    }
}