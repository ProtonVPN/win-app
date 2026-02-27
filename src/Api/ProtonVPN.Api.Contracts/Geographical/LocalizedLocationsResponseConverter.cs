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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ProtonVPN.Api.Contracts.Geographical;

public class LocalizedLocationsResponseConverter : JsonConverter<Dictionary<string, Dictionary<string, string?>>>
{
    public override Dictionary<string, Dictionary<string, string?>>? ReadJson(
        JsonReader reader,
        Type objectType,
        Dictionary<string, Dictionary<string, string?>>? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        Dictionary<string, Dictionary<string, string>> result = [];

        if (reader.TokenType == JsonToken.Null)
        {
            return result;
        }

        JObject obj = JObject.Load(reader);

        foreach (JProperty property in obj.Properties())
        {
            string countryCode = property.Name;

            if (property.Value.Type == JTokenType.Array)
            {
                result[countryCode] = new Dictionary<string, string?>();
            }
            else if (property.Value.Type == JTokenType.Object)
            {
                Dictionary<string, string> translations = [];
                foreach (JProperty innerProperty in ((JObject)property.Value).Properties())
                {
                    translations[innerProperty.Name] = innerProperty.Value.Type == JTokenType.Null
                        ? null
                        : innerProperty.Value.ToString();
                }
                result[countryCode] = translations;
            }
        }

        return result;
    }

    public override void WriteJson(JsonWriter writer, Dictionary<string, Dictionary<string, string?>>? value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }
}