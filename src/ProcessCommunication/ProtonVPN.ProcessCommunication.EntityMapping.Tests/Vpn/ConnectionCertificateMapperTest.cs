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
using ProtonVPN.Common.Core.LocalAgent;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.LocalAgent;
using ProtonVPN.ProcessCommunication.EntityMapping.Vpn;

namespace ProtonVPN.ProcessCommunication.EntityMapping.Tests.Vpn;

[TestClass]
public class ConnectionCertificateMapperTest
{
    private ILogger _logger;
    private ConnectionCertificateMapper _mapper;

    [TestInitialize]
    public void Initialize()
    {
        _logger = Substitute.For<ILogger>();
        _mapper = new(_logger);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _logger = null;
        _mapper = null;
    }

    [TestMethod]
    public void TestMapLeftToRight_WhenNull()
    {
        ConnectionCertificate entityToTest = null;

        ConnectionCertificateIpcEntity result = _mapper.Map(entityToTest);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void TestMapLeftToRight()
    {
        ConnectionCertificate entityToTest = new("CERT", DateTime.UtcNow.AddDays(1));

        ConnectionCertificateIpcEntity result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(entityToTest.Pem, result.Pem);
        Assert.AreEqual(entityToTest.ExpirationDateUtc, result.ExpirationDateUtc);
    }

    [TestMethod]
    public void TestMapRightToLeft_WhenNull()
    {
        ConnectionCertificateIpcEntity entityToTest = null;

        ConnectionCertificate result = _mapper.Map(entityToTest);

        Assert.IsNull(result);
    }

    [TestMethod]
    [DynamicData(nameof(GetCertificateTestData))]
    public void TestMapRightToLeft_WithCertificate(string certificate, string expectedPem)
    {
        ConnectionCertificateIpcEntity entityToTest = new()
        {
            Pem = certificate,
            ExpirationDateUtc = DateTime.UtcNow.AddDays(1),
        };

        ConnectionCertificate result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(expectedPem, result.Pem);
        Assert.AreEqual(entityToTest.ExpirationDateUtc, result.ExpirationDateUtc);
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
}