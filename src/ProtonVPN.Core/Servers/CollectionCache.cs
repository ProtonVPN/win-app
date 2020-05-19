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
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Text.Serialization;
using ProtonVPN.Core.Api.Contracts;

namespace ProtonVPN.Core.Servers
{
    public class ServersStorage : ICollectionStorage<LogicalServerContract>
    {
        private readonly ICollectionStorage<T> _origin;

        public ServersStorage(ILogger logger, ITextSerializerFactory serializerFactory, string filePath)
        {
            _origin =
                new CollectionStorage<T>(
                    new SafeStorage<IEnumerable<T>>(
                        new LoggingStorage<IEnumerable<T>>(
                            logger,
                            new FileStorage<IEnumerable<T>>(serializerFactory, filePath))));
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
