/*
 * Copyright (c) 2020 Proton Technologies AG
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

using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using ProtonVPN.Core.Api.Contracts;

namespace ProtonVPN.Core.Servers
{
    public class ServersFileStorage
    {
        private readonly string _file;

        public ServersFileStorage(string cacheFile)
        {
            _file = cacheFile;
        }

        public List<LogicalServerContract> Get()
        {
            if (!File.Exists(_file))
                return new List<LogicalServerContract>();

            using (var fileStream = File.OpenRead(_file))
            {
                using (var streamReader = new StreamReader(fileStream))
                {
                    using (var jsonTextReader = new JsonTextReader(streamReader))
                    {
                        var serializer = new JsonSerializer();

                        try
                        {
                            var servers = serializer.Deserialize<List<LogicalServerContract>>(jsonTextReader);
                            return servers ?? new List<LogicalServerContract>();
                        }
                        catch (JsonException)
                        {
                            return new List<LogicalServerContract>();
                        }
                    }
                }
            }
        }

        public void Save(ICollection<LogicalServerContract> servers)
        {
            using (var file = File.CreateText(_file))
            {
                new JsonSerializer().Serialize(file, servers);
            }
        }
    }
}
