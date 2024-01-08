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

using ProtonVPN.Client.Logic.Connection.Contracts.SerializableEntities.Intents;
using ProtonVPN.Client.Logic.Recents.Contracts.SerializableEntities;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Serialization.Contracts;

namespace ProtonVPN.Serialization.Protobuf.Entities;

public class ProtobufSerializableEntities : IProtobufSerializableEntities
{
    public List<Type> Types { get; } = CreateTypeList().ToList();

    private static IEnumerable<Type> CreateTypeList()
    {
        yield return typeof(PhysicalServer);
        yield return typeof(Server);
        yield return typeof(ServerFeatures);

        yield return typeof(SerializableConnectionIntent);
        yield return typeof(SerializableFeatureIntent);
        yield return typeof(SerializableLocationIntent);
        yield return typeof(SerializableRecentConnection);
    }
}