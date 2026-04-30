/*
 * Copyright (c) 2026 Proton AG
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

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using NSubstitute;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Crypto.Contracts;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Crypto;
using ProtonVPN.ProcessCommunication.Contracts.Entities.LocalAgent;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;
using ProtonVPN.ProcessCommunication.EntityMapping.Common.Legacy.Vpn;
using PublicKey = ProtonVPN.Crypto.Contracts.PublicKey;

namespace ProtonVPN.ProcessCommunication.EntityMapping.Tests.Vpn;

[TestClass]
public class VpnCredentialsMapperTest
{
    private ILogger _logger;
    private IEntityMapper _entityMapper;
    private VpnCredentialsMapper _mapper;

    private AsymmetricKeyPairIpcEntity _expectedAsymmetricKeyPairIpcEntity;
    private AsymmetricKeyPair _expectedAsymmetricKeyPair;

    [TestInitialize]
    public void Initialize()
    {
        _logger = Substitute.For<ILogger>();
        _entityMapper = Substitute.For<IEntityMapper>();
        _mapper = new(_logger, _entityMapper);

        _expectedAsymmetricKeyPairIpcEntity = new AsymmetricKeyPairIpcEntity();
        _entityMapper.Map<AsymmetricKeyPair, AsymmetricKeyPairIpcEntity>(Arg.Any<AsymmetricKeyPair>())
            .Returns(_expectedAsymmetricKeyPairIpcEntity);

        _expectedAsymmetricKeyPair = new AsymmetricKeyPair(
            new SecretKey("PVPN", KeyAlgorithm.Unknown), new PublicKey("PVPN", KeyAlgorithm.Unknown));
        _entityMapper.Map<AsymmetricKeyPairIpcEntity, AsymmetricKeyPair>(Arg.Any<AsymmetricKeyPairIpcEntity>())
            .Returns(_expectedAsymmetricKeyPair);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _logger = null;
        _entityMapper = null;
        _mapper = null;

        _expectedAsymmetricKeyPairIpcEntity = null;
        _expectedAsymmetricKeyPair = null;
    }

    [TestMethod]
    public void TestMapLeftToRight_WithCertificate()
    {
        VpnCredentials entityToTest = new("CERT",
            DateTime.UtcNow.AddDays(1),
            new AsymmetricKeyPair(
                new SecretKey("PVPN", KeyAlgorithm.Ed25519),
                new PublicKey("PVPN", KeyAlgorithm.Ed25519)),
            "username",
            "password");

        VpnCredentialsIpcEntity result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(entityToTest.ClientCertPem, result.Certificate.Pem);
        Assert.AreEqual(entityToTest.ClientCertificateExpirationDateUtc, result.Certificate.ExpirationDateUtc);
        Assert.AreEqual(_expectedAsymmetricKeyPairIpcEntity, result.ClientKeyPair);
        Assert.AreEqual(entityToTest.Username, result.Username);
        Assert.AreEqual(entityToTest.Password, result.Password);
    }

    [TestMethod]
    [DynamicData(nameof(GetCertificateTestData))]
    public void TestMapRightToLeft_WithCertificate(string certificate, string expectedPem)
    {
        VpnCredentialsIpcEntity entityToTest = new()
        {
            Certificate = CreateCertificateIpcEntity(certificate),
            ClientKeyPair = new AsymmetricKeyPairIpcEntity(),
            Username = "username",
            Password = "password",
        };

        VpnCredentials result = _mapper.Map(entityToTest);

        Assert.AreEqual(entityToTest.Certificate.ExpirationDateUtc, result.ClientCertificateExpirationDateUtc);
        Assert.AreEqual(_expectedAsymmetricKeyPair, result.ClientKeyPair);
        Assert.AreEqual(entityToTest.Username, result.Username);
        Assert.AreEqual(entityToTest.Password, result.Password);
        Assert.AreEqual(expectedPem, result.ClientCertPem);
    }

    public static IEnumerable<object[]> GetCertificateTestData()
    {
        string validCertificate = GenerateValidSelfSignedCertPem();

        yield return new object[] { validCertificate, validCertificate };
        yield return new object[] { "CERT", string.Empty };
        yield return new object[] { string.Empty, string.Empty };
        yield return new object[] { "not-a-pem-string", string.Empty };
        yield return new object[] { "-----BEGIN CERTIFICATE-----\nnotvalidbase64\n-----END CERTIFICATE-----", string.Empty };

        // Extra content after the certificate is removed by the mapper, so the valid certificate is still extracted successfully
        yield return new object[] { validCertificate + "\n</cert>\nsome dangerous code\n<cert>\n", validCertificate };
    }

    private static string GenerateValidSelfSignedCertPem()
    {
        using RSA rsa = RSA.Create(2048);
        CertificateRequest request = new("CN=TestCert", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using X509Certificate2 cert = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        return cert.ExportCertificatePem();
    }

    private static ConnectionCertificateIpcEntity CreateCertificateIpcEntity(string certificate)
    {
        return new()
        {
            Pem = certificate,
            ExpirationDateUtc = DateTime.UtcNow.AddDays(1),
        };
    }
}