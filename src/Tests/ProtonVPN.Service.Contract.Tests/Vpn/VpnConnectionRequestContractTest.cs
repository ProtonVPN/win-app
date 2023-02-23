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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Service.Contract.Crypto;
using ProtonVPN.Service.Contract.Vpn;

namespace ProtonVPN.Service.Contract.Tests.Vpn
{
    [TestClass]
    public class VpnConnectionRequestContractTest
    {
        private const string PublicKey = "public key";
        private const string PrivateKey = "private key";
        private const string ServerPublicKey = "server public key";

        [TestMethod]
        public void ItShouldThrowWhenKeysAreIncorrect()
        {
            // Arrange
            VpnConnectionRequestContract contract = GetConnectionRequestContract();

            // Act
            IList<ValidationResult> result = contract.Validate(new ValidationContext(this)).ToList();

            // Assert
            result.Should().Contain(c => c.ErrorMessage.Contains("Invalid server IP address"));
            result.Should().Contain(c => c.ErrorMessage.Contains($"Incorrect key length {PublicKey.Length}"));
            result.Should().Contain(c => c.ErrorMessage.Contains($"Incorrect key length {PrivateKey.Length}"));
            result.Should().Contain(c => c.ErrorMessage.Contains($"Incorrect key length {ServerPublicKey.Length}"));
        }

        private VpnConnectionRequestContract GetConnectionRequestContract()
        {
            return new()
            {
                Credentials = new VpnCredentialsContract
                {
                    ClientCertPem = "cert",
                    ClientKeyPair = new AsymmetricKeyPairContract
                    {
                        PublicKey = new PublicKeyContract
                        {
                            Algorithm = KeyAlgorithmContract.Ed25519,
                            Base64 = PublicKey,
                            Bytes = new byte[0],
                            Pem = string.Empty
                        },
                        SecretKey = new SecretKeyContract
                        {
                            Algorithm = KeyAlgorithmContract.Ed25519,
                            Base64 = PrivateKey,
                            Bytes = new byte[0],
                            Pem = string.Empty
                        }
                    }
                },
                Servers = new VpnHostContract[]
                {
                    new()
                    {
                        Ip = "127.0.0.999",
                        Label = string.Empty,
                        X25519PublicKey = new ServerPublicKeyContract
                        {
                            Algorithm = KeyAlgorithmContract.X25519,
                            Base64 = ServerPublicKey,
                            Bytes = new byte[0],
                            Pem = string.Empty
                        }
                    },
                },
                VpnConfig = new VpnConfigContract {CustomDns = new List<string>()}
            };
        }
    }
}