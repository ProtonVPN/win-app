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

namespace ProtonVPN.Crypto.Tests
{
    [TestClass]
    public class X25519KeyGeneratorTest
    {
        private const string Ed25519Asn1Base64SecretKey = "MC4CAQAwBQYDK2VwBCIEIJ1oDfMF/1sQ21ug3MtsR4gS/fSKrQjpltGaKPOzLrJW"; 
        private const string X25519Base64SecretKey = "uDiY1T9gYZO90r2fC63At9T2CnV1X8/NfWaQ/v/gT2g="; 

        private IX25519KeyGenerator _generator;

        [TestInitialize]  
        public void Initialize()  
        {  
            _generator = new X25519KeyGenerator();
        }

        [TestCleanup]  
        public void Cleanup()  
        {  
            _generator = null;
        }

        [TestMethod]
        public void TestFromEd25519SecretKey()
        {
            SecretKey ed25519Asn1SecretKey = new SecretKey(Ed25519Asn1Base64SecretKey, KeyAlgorithm.Ed25519);

            SecretKey x25519SecretKey = _generator.FromEd25519SecretKey(ed25519Asn1SecretKey);
            
            Assert.AreEqual(KeyAlgorithm.X25519, x25519SecretKey.Algorithm);
            Assert.AreEqual(32, x25519SecretKey.Bytes.Length);
            Assert.AreEqual(X25519Base64SecretKey, x25519SecretKey.Base64);
        }
    }
}
