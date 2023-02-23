/*
 * Copyright (c) 2023 Proton AG
 *
 * This file is part of ProtonVPN.
 *
 * ProtonVPN is free software:
 you can redistribute it and/or modify
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

using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProtonVPN.Crypto.Tests
{
    [TestClass]
    public class Ed25519Asn1KeyGeneratorTest
    {
        private IEd25519Asn1KeyGenerator _generator;

        [TestInitialize]  
        public void Initialize()  
        {  
            _generator = new Ed25519Asn1KeyGenerator();
        }

        [TestCleanup]  
        public void Cleanup()  
        {  
            _generator = null;
        }

        [TestMethod]
        public void TestGenerate()
        {
            AsymmetricKeyPair result = _generator.Generate();
            
            int secretKeyHeaderLength = Ed25519Asn1KeyGenerator.SecretKeyAsn1Header.Length;
            Assert.AreEqual(KeyAlgorithm.Ed25519, result.SecretKey.Algorithm);
            Assert.AreEqual(secretKeyHeaderLength + 32, result.SecretKey.Bytes.Length);
            CollectionAssert.AreEqual(Ed25519Asn1KeyGenerator.SecretKeyAsn1Header, result.SecretKey.Bytes.Take(secretKeyHeaderLength).ToArray());
            
            int publicKeyHeaderLength = Ed25519Asn1KeyGenerator.PublicKeyAsn1Header.Length;
            Assert.AreEqual(KeyAlgorithm.Ed25519, result.PublicKey.Algorithm);
            Assert.AreEqual(publicKeyHeaderLength + 32, result.PublicKey.Bytes.Length);
            CollectionAssert.AreEqual(Ed25519Asn1KeyGenerator.PublicKeyAsn1Header, result.PublicKey.Bytes.Take(publicKeyHeaderLength).ToArray());
        }

        [TestMethod]
        public void TestGenerateRandomness()
        {
            int numOfKeys = 100;
            int maxNumOfCollisions = 1; // Just in case we win the key collision lottery?

            IList<string> base64SecretKeys = new List<string>();
            for(int i = 0; i < numOfKeys; i++)
            {
                base64SecretKeys.Add(_generator.Generate().SecretKey.Base64);
            }

            Assert.IsTrue(base64SecretKeys.Distinct().Count() >= numOfKeys - maxNumOfCollisions, 
                $"Expected number of distinct secret keys to be greater or equal than {numOfKeys - maxNumOfCollisions}.");
        }
    }
}
