﻿/*
 * Copyright (c) 2024 Proton AG
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

using ProtonVPN.Client.Logic.Servers.Contracts.Enums;

namespace ProtonVPN.Client.Logic.Servers.Contracts.Models;

public interface IFeatureLocation : ILocation
{
    ServerFeatures Features { get; init; }

    bool IsStandardUnderMaintenance { get; init; }
    bool IsP2PUnderMaintenance { get; init; }
    bool IsSecureCoreUnderMaintenance { get; init; }
    bool IsTorUnderMaintenance { get; init; }
}