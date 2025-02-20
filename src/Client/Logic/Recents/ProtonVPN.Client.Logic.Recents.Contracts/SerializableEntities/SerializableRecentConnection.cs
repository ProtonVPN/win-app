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

namespace ProtonVPN.Client.Logic.Recents.Contracts.SerializableEntities;

public class SerializableRecentConnection
{
    public Guid? RecentId { get; set; }
    public SerializableConnectionIntent? ConnectionIntent { get; set; }
    public bool IsPinned { get; set; }
    public DateTime? PinTime { get; set; }
    public Guid? ProfileId { get; set; }
}