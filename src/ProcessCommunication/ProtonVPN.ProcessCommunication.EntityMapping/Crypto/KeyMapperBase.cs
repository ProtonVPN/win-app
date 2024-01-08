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

using ProtonVPN.Common.Legacy.Extensions;
using ProtonVPN.Crypto.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Crypto;

namespace ProtonVPN.ProcessCommunication.EntityMapping.Crypto;

public abstract class KeyMapperBase<TIpcEntity>
    where TIpcEntity : KeyIpcEntity, new()
{
    protected TIpcEntity MapKeyToIpcEntity(Key key)
    {
        return new()
        {
            Bytes = key.Bytes,
            Base64 = key.Base64,
            Algorithm = key.Algorithm.ToString().ToEnum<KeyAlgorithmIpcEntity>(),
            Pem = key.Pem,
        };
    }
}
