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

using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;

namespace ProtonVPN.Client.Logic.Recents.Contracts;

public interface IRecentConnection
{
    IConnectionIntent ConnectionIntent { get; set; }

    bool IsPinned { get; set; }

    DateTime? PinTime { get; set; }

    [Obsolete("Active connection is calculated at runtime and should not be saved in the recent connection")]
    bool IsActiveConnection { get; set; }

    [Obsolete("Server under maintenance is calculated at runtime and should not be saved in the recent connection")]
    bool IsServerUnderMaintenance { get; set; }
}