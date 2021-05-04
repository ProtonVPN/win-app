/*
 * Copyright (c) 2021 Proton Technologies AG
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

using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Storage;
using ProtonVPN.Common.Text.Serialization;
using ProtonVPN.Core.Api.Contracts;

namespace ProtonVPN.Streaming
{
    internal class StreamingServicesStorage
    {
        private readonly IStorage<StreamingServicesResponse> _storage;

        public StreamingServicesStorage(ILogger logger, ITextSerializerFactory serializers, Common.Configuration.Config config)
        {
            _storage = new SafeStorage<StreamingServicesResponse>(
                new LoggingStorage<StreamingServicesResponse>(
                    logger,
                    new FileStorage<StreamingServicesResponse>(
                        serializers,
                        config.StreamingServicesFilePath)));
        }

        public StreamingServicesResponse Get()
        {
            return _storage.Get();
        }

        public void Set(StreamingServicesResponse value)
        {
            _storage.Set(value);
        }
    }
}