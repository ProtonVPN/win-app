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

using System.IO;
using Newtonsoft.Json;

namespace ProtonVPN.Common.Configuration.Storage
{
    public class FileConfigStorage : IConfigStorage
    {
        private readonly IStorageFile _file;

        public FileConfigStorage(IStorageFile file)
        {
            _file = file;
        }

        public IConfiguration Value()
        {
            JsonSerializer serializer = Serializer();
            using (StreamReader stream = new(_file.Path()))
            using (JsonTextReader reader = new(stream))
            {
                return serializer.Deserialize<Config>(reader);
            }
        }

        public void Save(IConfiguration value)
        {
            JsonSerializer serializer = Serializer();
            using (StreamWriter stream = new(_file.Path()))
            using (JsonTextWriter writer = new(stream))
            {
                serializer.Serialize(writer, value);
            }
        }

        private JsonSerializer Serializer()
        {
            JsonSerializerSettings settings = new() { ContractResolver = new PropertiesContractResolver() };
            return JsonSerializer.CreateDefault(settings);
        }
    }
}