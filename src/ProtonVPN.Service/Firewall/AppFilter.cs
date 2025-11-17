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
using System.IO;
using System.Linq;
using ProtonVPN.NetworkFilter;
using Action = ProtonVPN.NetworkFilter.Action;

namespace ProtonVPN.Service.Firewall;

public class AppFilter : IAppFilter
{
    private readonly IpFilter _ipFilter;
    private readonly IpLayer _ipLayer;
    private readonly Dictionary<string, List<Guid>> _list = [];

    public AppFilter(IpFilter ipFilter, IpLayer ipLayer)
    {
        _ipLayer = ipLayer;
        _ipFilter = ipFilter;
    }

    public void Add(string[] paths, Tuple<Layer, Action>[] filters)
    {
        foreach (string path in paths)
        {
            Add(path, filters);
        }
    }

    private void Add(string path, Tuple<Layer, Action>[] filters)
    {
        if (_list.ContainsKey(path))
        {
            return;
        }

        _list[path] = [];

        foreach ((Layer layer, Action action) in filters)
        {
            _ipLayer.Apply(appliedLayer =>
            {
                Guid guid = _ipFilter.DynamicSublayer.CreateAppFilter(
                    new DisplayData("ProtonVPN app filter", ""),
                    action,
                    appliedLayer,
                    14,
                    path,
                    isDnsPortExcluded: action == Action.HardPermit);

                _list[path].Add(guid);
            }, [layer]);
        }
    }

    public void Remove(string path)
    {
        if (!_list.TryGetValue(path, out List<Guid> guids))
        {
            return;
        }

        foreach (Guid guid in guids)
        {
            _ipFilter.DynamicSublayer.DestroyFilter(guid);
        }

        _list.Remove(path);
    }

    public void RemoveAll()
    {
        foreach (KeyValuePair<string, List<Guid>> element in _list.ToList())
        {
            Remove(element.Key);
        }
    }
}