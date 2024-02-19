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
using ProtonVPN.Serialization.Contracts.Json;
using NewtonsoftJsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace ProtonVPN.Serialization.Json;

public abstract class JsonSerializerBase
{
    protected readonly NewtonsoftJsonSerializer Serializer;

    private readonly JsonSerializerSettings _serializationSettings;
    private readonly ContractDeserializationJsonConverter _contractDeserializationJsonConverter;

    public JsonSerializerBase(IEnumerable<IJsonContractDeserializer> jsonContractDeserializers)
    {
        _contractDeserializationJsonConverter = new(jsonContractDeserializers);

        _serializationSettings = CreateSerializerSettings();
        _serializationSettings.Converters.Add(_contractDeserializationJsonConverter);

        Serializer = NewtonsoftJsonSerializer.Create(_serializationSettings);
    }

    protected abstract JsonSerializerSettings CreateSerializerSettings();

    public MemoryStream Serialize<T>(T? instance)
    {
        MemoryStream memoryStream = new();
        using (StreamWriter streamWriter = new(memoryStream, leaveOpen: true))
        using (JsonTextWriter jsonWriter = new(streamWriter))
        {
            Serializer.Serialize(jsonWriter, instance);
            jsonWriter.Flush();
        }
        memoryStream.Position = 0;
        return memoryStream;
    }

    public T? Deserialize<T>(MemoryStream memoryStream)
    {
        memoryStream.Position = 0;
        using (StreamReader streamReader = new(memoryStream))
        using (JsonTextReader jsonReader = new(streamReader))
        {
            return Serializer.Deserialize<T>(jsonReader);
        }
    }
}