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

using System.Collections.Generic;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Tests.Storage;
using ProtonVPN.Common.Text.Serialization;
using ProtonVPN.Core.Servers.Contracts;
using ProtonVPN.GuestHoles.FileStoraging;

namespace ProtonVPN.App.Tests.GuestHoles.FileStoraging
{
    public class GuestHoleServersFileStorageTest
        : FileStorageBaseTest<GuestHoleServersFileStorage, IEnumerable<GuestHoleServerContract>>
    {
        protected override GuestHoleServersFileStorage Construct(ILogger logger,
            ITextSerializerFactory serializerFactory, IConfiguration appConfig, string fileName)
        {
            if (appConfig is not null)
            {
                appConfig.GuestHoleServersJsonFilePath = fileName;
            }
            return new GuestHoleServersFileStorage(logger, serializerFactory, appConfig);
        }

        protected override IEnumerable<GuestHoleServerContract> CreateEntity()
        {
            yield return new GuestHoleServerContract()
            {
                Host = "protonvpn.com",
                Ip = "127.0.0.1",
                Label = "123",
            };
            yield return new GuestHoleServerContract()
            {
                Host = "proton.me",
                Ip = "127.0.0.2",
                Label = "456",
            };
        }
    }
}