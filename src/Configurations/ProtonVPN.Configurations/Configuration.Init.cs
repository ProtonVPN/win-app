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

using System.Reflection;
using System.Runtime.CompilerServices;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Configurations.BigTestInfra;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Configurations.Defaults;
using ProtonVPN.Configurations.Repositories;

namespace ProtonVPN.Configurations;

public partial class Configuration
{
    private const double RANDOMIZED_INTERVAL_DEVIATION = 0.2;

    private readonly IConfigurationRepository _repository;
    private readonly IDictionary<string, Lazy<object?>> _values;

    public Configuration(IConfigurationRepository repository)
    {
        _repository = repository;
        _values = CreateValueFunctions();
    }

    private IDictionary<string, Lazy<object?>> CreateValueFunctions()
    {
        IList<PropertyInfo> properties = typeof(IConfiguration).GetProperties(BindingFlagsConstants.PUBLIC_DECLARED_ONLY).ToList();
        Dictionary<string, Lazy<object?>> values = new();
        foreach (PropertyInfo property in properties)
        {
            Lazy<object?> value = new(() =>
                GetValueFromRepository(property) ??
                GetValueFromBtiOrDefaultConfiguration(property));
            values.Add(property.Name, value);
        }
        return values;
    }

    private object? GetValueFromRepository(PropertyInfo property)
    {
        return _repository.GetByType(property.PropertyType, property.Name);
    }

    private object? GetValueFromBtiOrDefaultConfiguration(PropertyInfo property)
    {
        object? value = GetValueFromDefaultConfiguration(property);

#if DEBUG
        return BtiConfigurationLoader.GetValue(property, value);
#else
        return value;
#endif
    }

    private object? GetValueFromDefaultConfiguration(PropertyInfo property)
    {
        return typeof(DefaultConfiguration).GetProperty(property.Name)?.GetValue(null);
    }

    private dynamic? Get([CallerMemberName] string propertyName = "")
    {
        return _values[propertyName].Value;
    }

    private TimeSpan GetWithRandomizedDeviation([CallerMemberName] string propertyName = "", double deviation = RANDOMIZED_INTERVAL_DEVIATION)
    {
        if (Get(propertyName) is not TimeSpan interval)
        {
            throw new InvalidCastException($"Property {propertyName} is not of type TimeSpan");
        }

        return interval.AddJitter(deviation);
    }
}