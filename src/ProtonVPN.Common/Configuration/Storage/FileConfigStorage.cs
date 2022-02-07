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

using System.IO;
using Newtonsoft.Json;

namespace ProtonVPN.Common.Configuration.Storage
{
    internal class FileConfigStorage : IConfigStorage
    {
        private readonly IStorageFile _file;

        public FileConfigStorage(IStorageFile file)
        {
            _file = file;
        }

        public Config Value()
        {
            var serializer = Serializer();
            using (var stream = new StreamReader(_file.Path()))
            using (var reader = new JsonTextReader(stream))
            {
                return serializer.Deserialize<Config>(reader);
            }
        }

        public void Save(Config value)
        {
            var serializer = Serializer();
            using (var stream = new StreamWriter(_file.Path()))
            using (var writer = new JsonTextWriter(stream))
            {
                serializer.Serialize(writer, value);
            }
        }

        private JsonSerializer Serializer()
        {
            var settings = new JsonSerializerSettings { ContractResolver = new PropertiesContractResolver() };
            return JsonSerializer.CreateDefault(settings);
        }
    }
}