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

using ProtonVPN.Api.Contracts.Servers;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.EntityMapping.Contracts;

namespace ProtonVPN.Client.Logic.Servers.Mappers;

public class PhysicalServerMapper : IMapper<PhysicalServerResponse, PhysicalServer>
{
    public PhysicalServer Map(PhysicalServerResponse leftEntity)
    {
        return new(
            id: leftEntity.Id,
            entryIp: leftEntity.EntryIp,
            exitIp: leftEntity.ExitIp,
            domain: leftEntity.Domain,
            label: leftEntity.Label,
            status: leftEntity.Status,
            x25519PublicKey: leftEntity.X25519PublicKey,
            signature: leftEntity.Signature);
    }

    public PhysicalServerResponse Map(PhysicalServer rightEntity)
    {
        throw new NotImplementedException();
    }
}