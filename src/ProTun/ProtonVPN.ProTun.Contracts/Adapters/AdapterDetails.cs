/*
 * Copyright (c) 2025 Proton AG
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

namespace ProtonVPN.ProTun.Contracts.Adapters;

public class AdapterDetails
{
    public required uint InterfaceIndex { get; init; }

    public required string ClientIpv4Addr { get; init; }
    public required string ServerIpv4Addr { get; init; }

    public required string ClientIpv6Addr { get; init; }
    public required string ServerIpv6Addr { get; init; }
}