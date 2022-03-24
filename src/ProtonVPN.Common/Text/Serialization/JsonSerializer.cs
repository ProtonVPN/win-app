/*
 * Copyright (c) 2022 Proton Technologies AG
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
using System.IO;
using Newtonsoft.Json;
using ProtonVPN.Common.Extensions;

namespace ProtonVPN.Common.Text.Serialization
{
    public class JsonSerializer<T> : ITextSerializer<T>, IThrowsExpectedExceptions
    {
        private readonly JsonSerializer _serializer = new();

        public T Deserialize(TextReader source)
        {
            using JsonTextReader jsonReader = new(source);
            return _serializer.Deserialize<T>(jsonReader);
        }

        public void Serialize(T value, TextWriter writer)
        {
            using JsonTextWriter jsonWriter = new(writer);
            _serializer.Serialize(jsonWriter, value);
        }

        public bool IsExpectedException(Exception ex)
        {
            return ex is JsonException;
        }
    }
}
