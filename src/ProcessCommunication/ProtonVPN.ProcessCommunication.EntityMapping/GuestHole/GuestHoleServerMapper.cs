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

using ProtonVPN.Client.Logic.Connection.Contracts.GuestHole;
using ProtonVPN.Crypto.Contracts;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Crypto;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.ProcessCommunication.EntityMapping.GuestHole;

public class GuestHoleServerMapper : IMapper<GuestHoleServerContract, VpnServerIpcEntity>
{
    private readonly IEntityMapper _entityMapper;

    public GuestHoleServerMapper(IEntityMapper entityMapper)
    {
        _entityMapper = entityMapper;
    }

    public VpnServerIpcEntity Map(GuestHoleServerContract leftEntity)
    {
        return leftEntity is null
            ? null
            : new VpnServerIpcEntity()
            {
                Name = leftEntity.Host,
                Ip = leftEntity.Ip,
                Label = leftEntity.Label,
                Signature = leftEntity.Signature,
                X25519PublicKey = _entityMapper.Map<PublicKey, ServerPublicKeyIpcEntity>(
                    new PublicKey(leftEntity.X25519PublicKey, KeyAlgorithm.X25519)),
            };
    }

    public GuestHoleServerContract Map(VpnServerIpcEntity rightEntity)
    {
        return rightEntity is null
            ? null
            : new GuestHoleServerContract()
            {
                Host = rightEntity.Name,
                Ip = rightEntity.Ip,
                Label = rightEntity.Label,
                Signature = rightEntity.Signature,
                X25519PublicKey = null,
            };
    }
}