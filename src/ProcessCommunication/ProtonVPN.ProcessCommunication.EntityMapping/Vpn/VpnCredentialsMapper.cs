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

using ProtonVPN.Common.Vpn;
using ProtonVPN.Crypto;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Crypto;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.ProcessCommunication.EntityMapping.Vpn
{
    public class VpnCredentialsMapper : IMapper<VpnCredentials, VpnCredentialsIpcEntity>
    {
        private readonly IEntityMapper _entityMapper;

        public VpnCredentialsMapper(IEntityMapper entityMapper)
        {
            _entityMapper = entityMapper;
        }

        public VpnCredentialsIpcEntity Map(VpnCredentials leftEntity)
        {
            return new()
                {
                    ClientCertPem = leftEntity.ClientCertPem,
                    ClientKeyPair = _entityMapper.Map<AsymmetricKeyPair, AsymmetricKeyPairIpcEntity>(leftEntity.ClientKeyPair)
                };
        }

        public VpnCredentials Map(VpnCredentialsIpcEntity rightEntity)
        {
            return rightEntity is null
                ? throw new ArgumentNullException($"The {nameof(VpnCredentialsIpcEntity)} to be mapped is null.")
                : new(rightEntity.ClientCertPem, _entityMapper.Map<AsymmetricKeyPairIpcEntity, AsymmetricKeyPair>(rightEntity.ClientKeyPair));
        }
    }
}