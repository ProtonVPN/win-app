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
using Newtonsoft.Json.Linq;
using ProtonVPN.Serialization.Contracts;

namespace ProtonVPN.Serialization.Json;

public class JsonSerializer : IJsonSerializer
{
    private readonly Newtonsoft.Json.JsonSerializer _serializer;
    private readonly JsonSerializerSettings _serializationSettings;
    private readonly JsonSerializerSettings _prettySerializationSettings;
    private readonly ContractDeserializationJsonConverter _contractDeserializationJsonConverter;

    public JsonSerializer(IEnumerable<IJsonContractDeserializer> jsonContractDeserializers)
    {
        _contractDeserializationJsonConverter = new(jsonContractDeserializers);
        _serializationSettings = CreateSerializationSettings();
        _prettySerializationSettings = CreatePrettySerializationSettings();
        _serializer = Newtonsoft.Json.JsonSerializer.Create(_serializationSettings);
    }

    private JsonSerializerSettings CreateSerializationSettings()
    {
        JsonSerializerSettings jsonSerializerSettings = new()
        {
            NullValueHandling = NullValueHandling.Ignore
        };
        jsonSerializerSettings.Converters.Add(_contractDeserializationJsonConverter);
        return jsonSerializerSettings;
    }

    private JsonSerializerSettings CreatePrettySerializationSettings()
    {
        JsonSerializerSettings settings = CreateSerializationSettings();
        settings.Formatting = Formatting.Indented;
        return settings;
    }

    public string Serialize(object? json)
    {
        return JsonConvert.SerializeObject(json, _serializationSettings);
    }

    public string SerializePretty(object? json)
    {
        return JsonConvert.SerializeObject(json, _prettySerializationSettings);
    }

    public void Serialize<T>(T json, TextWriter writer)
    {
        using JsonTextWriter jsonWriter = new(writer);
        _serializer.Serialize(jsonWriter, json);
    }

    public T? Deserialize<T>(string json)
    {
        using JsonTextReader jsonReader = new(new StringReader(json));
        return _serializer.Deserialize<T>(jsonReader);
    }

    public T? Deserialize<T>(TextReader source)
    {
        using JsonTextReader jsonReader = new(source);
        return _serializer.Deserialize<T>(jsonReader);
    }

    public object? Deserialize(string json, Type type)
    {
        using JsonTextReader jsonReader = new(new StringReader(json));
        return _serializer.Deserialize(jsonReader, type);
    }

    public IDictionary<string, string?> DeserializeFirstLevel(string json)
    {
        return JObject.Parse(json)
                      .Children<JProperty>()
                      .ToDictionary(p => p.Name, 
                                    p => p.Value?.ToString(Formatting.None));
    }
}