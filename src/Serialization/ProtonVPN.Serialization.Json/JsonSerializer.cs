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

namespace ProtonVPN.Serialization.Json;

public class JsonSerializer : IJsonSerializer
{
    private readonly Newtonsoft.Json.JsonSerializer _serializer = new();
    private readonly JsonSerializerSettings _serializationSettings;
    private readonly JsonSerializerSettings _prettySerializationSettings;

    public JsonSerializer()
    {
        _serializationSettings = CreateSerializationSettings();
        _prettySerializationSettings = CreatePrettySerializationSettings();
    }

    private JsonSerializerSettings CreateSerializationSettings()
    {
        return new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };
    }

    private JsonSerializerSettings CreatePrettySerializationSettings()
    {
        JsonSerializerSettings settings = CreateSerializationSettings();
        settings.Formatting = Formatting.Indented;
        return settings;
    }

    public string Serialize(object? value)
    {
        return JsonConvert.SerializeObject(value, _serializationSettings);
    }

    public string SerializePretty(object? value)
    {
        return JsonConvert.SerializeObject(value, _prettySerializationSettings);
    }

    public T? Deserialize<T>(string value)
    {
        return JsonConvert.DeserializeObject<T>(value);
    }

    public T? Deserialize<T>(TextReader source)
    {
        using JsonTextReader jsonReader = new(source);
        return _serializer.Deserialize<T>(jsonReader);
    }

    public void Serialize<T>(T value, TextWriter writer)
    {
        using JsonTextWriter jsonWriter = new(writer);
        _serializer.Serialize(jsonWriter, value);
    }
}