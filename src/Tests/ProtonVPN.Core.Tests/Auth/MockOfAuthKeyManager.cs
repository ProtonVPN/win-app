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

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using ProtonVPN.Core.Auth;
using ProtonVPN.Crypto;

namespace ProtonVPN.Core.Tests.Auth
{
    public class MockOfAuthKeyManager : IAuthKeyManager
    {
        private SecretKey _secretKey;
        private PublicKey _publicKey;

        public IDictionary<string, int> MethodCalls;

        public MockOfAuthKeyManager()
        {
            _secretKey = null;
            _publicKey = null;

            MethodCalls = new Dictionary<string, int>();

            Type type = typeof(MockOfAuthKeyManager);
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (MethodInfo method in methods)
            {
                MethodCalls.Add(method.Name, 0);
            }
        }

        public void InitializeTestKeyPair()
        {
            _secretKey = new SecretKey(CreateOrIncrementKeyBytes(_secretKey), KeyAlgorithm.Ed25519);
            _publicKey = new PublicKey(CreateOrIncrementKeyBytes(_publicKey), KeyAlgorithm.Ed25519);
        }

        public void RegenerateKeyPair()
        {
            IncrementMethodCalls();
            InitializeTestKeyPair();
        }

        private byte[] CreateOrIncrementKeyBytes(Key key)
        {
            return key == null ? new byte[32] : BitConverter.GetBytes(BitConverter.ToInt64(key.Bytes, 0) + 1);
        }

        private void IncrementMethodCalls([CallerMemberName] string caller = null)
        {
            MethodCalls[caller]++;
        }

        public void DeleteKeyPair()
        {
            IncrementMethodCalls();
            _secretKey = null;
            _publicKey = null;
        }

        public AsymmetricKeyPair GetKeyPairOrNull()
        {
            IncrementMethodCalls();
            return _secretKey == null || _publicKey == null ? null : new AsymmetricKeyPair(_secretKey, _publicKey);
        }

        public SecretKey GetSecretKey()
        {
            IncrementMethodCalls();
            return _secretKey;
        }

        public PublicKey GetPublicKey()
        {
            IncrementMethodCalls();
            return _publicKey;
        }
    }
}