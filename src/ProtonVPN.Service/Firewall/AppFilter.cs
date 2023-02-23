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
using System.IO;
using System.Linq;
using ProtonVPN.NetworkFilter;
using Action = ProtonVPN.NetworkFilter.Action;

namespace ProtonVPN.Service.Firewall
{
    public class AppFilter : IFilterCollection
    {
        private readonly IpFilter _ipFilter;
        private readonly IpLayer _ipLayer;
        private readonly Dictionary<string, List<Guid>> _list = new Dictionary<string, List<Guid>>();

        public AppFilter(IpFilter ipFilter, IpLayer ipLayer)
        {
            _ipLayer = ipLayer;
            _ipFilter = ipFilter;
        }

        public void Add(string[] paths, Action action)
        {
            foreach (string path in paths)
            {
                Add(path, action);
            }
        }

        public void Add(string path, Action action)
        {
            if (_list.ContainsKey(path))
            {
                return;
            }

            if (!File.Exists(path))
            {
                return;
            }

            _list[path] = new List<Guid>();

            _ipLayer.ApplyToIpv4(layer =>
            {
                Guid guid = _ipFilter.DynamicSublayer.CreateAppFilter(
                    new DisplayData("ProtonVPN permit app", "Allow app to bypass VPN tunnel"),
                    action,
                    layer,
                    14,
                    path);

                _list[path].Add(guid);
            });
        }

        public void Remove(string path)
        {
            if (!_list.ContainsKey(path))
            {
                return;
            }

            foreach (Guid guid in _list[path])
            {
                _ipFilter.DynamicSublayer.DestroyFilter(guid);
            }

            _list.Remove(path);
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