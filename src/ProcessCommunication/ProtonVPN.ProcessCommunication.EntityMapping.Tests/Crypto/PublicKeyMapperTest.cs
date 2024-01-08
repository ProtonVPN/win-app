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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Crypto.Contracts;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Crypto;
using ProtonVPN.ProcessCommunication.EntityMapping.Crypto;

namespace ProtonVPN.ProcessCommunication.EntityMapping.Tests.Crypto;

[TestClass]
public class PublicKeyMapperTest : KeyMapperTestBase<PublicKey, PublicKeyIpcEntity>
{
    protected override IMapper<PublicKey, PublicKeyIpcEntity> CreateKeyMapper()
    {
        return new PublicKeyMapper();
    }

    protected override PublicKey CreateKey(string base64, KeyAlgorithm algorithm)
    {
        return new(base64, algorithm);
    }

    protected override string CreateExpectedPem(string base64)
    {
        return $"-----BEGIN PUBLIC KEY-----\r\n{base64}\r\n-----END PUBLIC KEY-----";
    }
}