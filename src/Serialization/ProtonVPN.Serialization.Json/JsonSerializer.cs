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

using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProtonVPN.Serialization.Contracts;
using ProtonVPN.Serialization.Contracts.Json;

namespace ProtonVPN.Serialization.Json;

public class JsonSerializer : JsonSerializerBase, IJsonSerializer
{
    public Serializers Type => Serializers.Json;

    public JsonSerializer(IEnumerable<IJsonContractDeserializer> jsonContractDeserializers)
        : base(jsonContractDeserializers)
    {
    }

    protected override JsonSerializerSettings CreateSerializerSettings()
    {
        return new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore
        };
    }

    public string SerializeToString<T>(T? json)
    {
        using (MemoryStream memoryStream = Serialize(json))
        using (StreamReader streamReader = new(memoryStream))
        {
            return streamReader.ReadToEnd();
        }
    }

    public T? DeserializeFromString<T>(string json)
    {
        using (MemoryStream memoryStream = new(Encoding.UTF8.GetBytes(json ?? string.Empty)))
        {
            return Deserialize<T>(memoryStream);
        }
    }

    public object? DeserializeFromStringAndType(string json, Type type)
    {
        using (JsonTextReader jsonReader = new(new StringReader(json)))
        {
            return Serializer.Deserialize(jsonReader, type);
        }
    }

    public IDictionary<string, string?> DeserializeFirstLevel(string json)
    {
        return JObject.Parse(json)
                      .Children<JProperty>()
                      .ToDictionary(p => p.Name,
                                    p => p.Value?.ToString(Formatting.None));
    }
}