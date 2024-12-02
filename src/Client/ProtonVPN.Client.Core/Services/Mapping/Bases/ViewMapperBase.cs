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
using ProtonVPN.Client.Core.Models;

namespace ProtonVPN.Client.Core.Services.Mapping.Bases;

public abstract class ViewMapperBase : IViewMapper
{
    private readonly List<ViewMappingPair> _pairs = new();

    protected ViewMapperBase()
    {
        ConfigureMappings();
    }

    public Type GetViewModelType(Type viewType)
    {
        lock (_pairs)
        {
            return _pairs.FirstOrDefault(p => p.ViewType == viewType)?.ViewModelType
                ?? throw new ArgumentException($"Corresponding view model not found for '{viewType}'. Did you forget to configure the mapping in the ViewMapper?");
        }
    }

    public Type GetViewType(Type viewModelType)
    {
        lock (_pairs)
        {
            return _pairs.FirstOrDefault(p => p.ViewModelType == viewModelType)?.ViewType
                ?? throw new ArgumentException($"Corresponding view not found for '{viewModelType}'. Did you forget to configure the mapping in the ViewMapper?");
        }
    }

    protected abstract void ConfigureMappings();

    protected void ConfigureMapping(Type viewModelType, Type viewType)
    {
        lock (_pairs)
        {
            if (_pairs.Any(p => p.ViewType == viewType))
            {
                throw new ArgumentException($"The mapping for '{viewType}' is already configured in the ViewMapper");
            }

            if (_pairs.Any(p => p.ViewModelType == viewModelType))
            {
                throw new ArgumentException($"The mapping for '{viewModelType}' is already configured in the ViewMapper");
            }

            ViewMappingPair pair = new(viewType: viewType, viewModelType: viewModelType);
            _pairs.Add(pair);
        }
    }
}