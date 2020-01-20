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
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Storage;
using ProtonVPN.Common.Text.Serialization;
using ProtonVPN.Core.Api.Contracts;

namespace ProtonVPN.Core.Servers
{
    public class ServerCache
    {
        private readonly ICollectionStorage<LogicalServerContract> _origin;

        public ServerCache(ILogger logger, ITextSerializerFactory serializerFactory, Config config)
        {
            _origin = 
                new CollectionStorage<LogicalServerContract>(
                    new SafeStorage<IEnumerable<LogicalServerContract>>(
                        new LoggingStorage<IEnumerable<LogicalServerContract>>(
                            logger,
                            new FileStorage<IEnumerable<LogicalServerContract>>(
                                serializerFactory,
                                config.ServersJsonCacheFilePath))));
        }

        public IReadOnlyCollection<LogicalServerContract> GetAll()
        {
            return _origin.GetAll();
        }

        public void SetAll(IEnumerable<LogicalServerContract> value)
        {
            _origin.SetAll(value);
        }
    }
}
