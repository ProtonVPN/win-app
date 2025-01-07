﻿/*
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

namespace ProtonVPN.Client.Logic.Profiles.Contracts.SerializableEntities;

public class SerializableProfile : SerializableConnectionIntent
{
    public Guid Id { get; set; }

    public DateTime CreationDateTimeUtc { get; set; }

    public DateTime UpdateDateTimeUtc { get; set; }

    public required string Name { get; set; }

    public int ProfileCategory { get; set; }

    public int ProfileColor { get; set; }

    public required SerializableProfileSettings Settings { get; set; }
}