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
using ProtoBuf;
using ProtonVPN.Common.Legacy.Extensions;
using ProtonVPN.Crypto.Contracts;

namespace ProtonVPN.ProcessCommunication.Contracts.Entities.Crypto
{
    [ProtoContract]
    [ProtoInclude(101, typeof(ServerPublicKeyIpcEntity))]
    [ProtoInclude(102, typeof(PublicKeyIpcEntity))]
    [ProtoInclude(103, typeof(SecretKeyIpcEntity))]
    public abstract class KeyIpcEntity
    {
        [ProtoMember(1)]
        public byte[] Bytes { get; set; }

        [ProtoMember(2)]
        public string? Base64 { get; set; }

        [ProtoMember(3)]
        public KeyAlgorithmIpcEntity Algorithm { get; set; }

        [ProtoMember(4)]
        public string? Pem { get; set; }

        protected abstract int KeyLength { get; }

        protected KeyIpcEntity()
        {
            Algorithm = KeyAlgorithmIpcEntity.Unknown;
            Bytes = Array.Empty<byte>();
        }

        protected KeyIpcEntity(Key key)
        {
            Bytes = key.Bytes;
            Base64 = key.Base64;
            Algorithm = key.Algorithm.ToString().ToEnum<KeyAlgorithmIpcEntity>();
            Pem = key.Pem;
        }
    }
}