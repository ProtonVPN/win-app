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

using ProtonVPN.Client.Files.Contracts.Images;
using ProtonVPN.Client.Logic.Announcements.Contracts.Entities;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.SerializableEntities.Intents;
using ProtonVPN.Client.Logic.Profiles.Contracts.SerializableEntities;
using ProtonVPN.Client.Logic.Recents.Contracts.SerializableEntities;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Common.Core.Geographical;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Core.StatisticalEvents;
using ProtonVPN.Serialization.Contracts;
using ProtonVPN.StatisticalEvents.Contracts.Models;

namespace ProtonVPN.Serialization.Protobuf.Entities;

public class ProtobufSerializableEntities : IProtobufSerializableEntities
{
    public List<Type> Types { get; } = CreateTypeList().ToList();

    private static IEnumerable<Type> CreateTypeList()
    {
        yield return typeof(ServersFile);
        yield return typeof(PhysicalServer);
        yield return typeof(Server);
        yield return typeof(ServerFeatures);
        yield return typeof(ServerTiers);
        yield return typeof(VpnProtocol);
        yield return typeof(DeviceLocation);
        yield return typeof(SelectionStrategy);
        yield return typeof(ServerInfo);
        yield return typeof(GatewayServerInfo);

        yield return typeof(SerializableConnectionIntent);
        yield return typeof(SerializableFeatureIntent);
        yield return typeof(SerializableLocationIntent);
        yield return typeof(SerializableRecentConnection);

        yield return typeof(CachedImage);

        yield return typeof(Announcement);
        yield return typeof(AnnouncementType);
        yield return typeof(FullScreenImage);
        yield return typeof(Panel);
        yield return typeof(PanelButton);
        yield return typeof(PanelFeature);

        yield return typeof(SerializableProfile);
        yield return typeof(SerializableProfileIcon);
        yield return typeof(SerializableProfileSettings);
        yield return typeof(SerializableProfileOptions);

        yield return typeof(StatisticalEventsFile);
        yield return typeof(StatisticalEvent);
    }
}