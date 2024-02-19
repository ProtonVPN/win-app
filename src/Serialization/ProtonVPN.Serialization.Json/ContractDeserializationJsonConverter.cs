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

using Newtonsoft.Json;
using ProtonVPN.Serialization.Contracts;
using ProtonVPN.Serialization.Contracts.Json;

namespace ProtonVPN.Serialization.Json;

public class ContractDeserializationJsonConverter : JsonConverter
{
    private readonly IDictionary<Type, Type> _interfaceImplementations;

    public ContractDeserializationJsonConverter(IEnumerable<IJsonContractDeserializer> jsonContractDeserializers)
    {
        IEnumerable<InterfaceImplementation> interfaceImplementations = jsonContractDeserializers.SelectMany(jcd => jcd.Get());
        CheckIfEachInterfaceHasOnlyOneImplementation(interfaceImplementations);
        _interfaceImplementations = interfaceImplementations.ToDictionary(ii => ii.InterfaceType, ii => ii.ImplementationType);
    }

    private void CheckIfEachInterfaceHasOnlyOneImplementation(IEnumerable<InterfaceImplementation> interfaceImplementations)
    {
        var highestCountInterface = interfaceImplementations
            .GroupBy(ii => ii.InterfaceType)
            .Select(group => new { InterfaceType = group.Key, Count = group.Count() })
            .OrderByDescending(o => o.Count)
            .FirstOrDefault();
        
        if (highestCountInterface is not null && highestCountInterface.Count > 1)
        {
            throw new Exception($"There is more than a single JSON deserialization implementation" +
                $"for the contract {highestCountInterface.InterfaceType.FullName}.");
        }
    }

    public override bool CanConvert(Type objectType)
    {
        return _interfaceImplementations.ContainsKey(objectType);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, Newtonsoft.Json.JsonSerializer serializer)
    {
        return serializer.Deserialize(reader, _interfaceImplementations[objectType]);
    }

    public override void WriteJson(JsonWriter writer, object? value, Newtonsoft.Json.JsonSerializer serializer)
    {
        throw new NotImplementedException("Doesn't need to be implemented.");
    }
}