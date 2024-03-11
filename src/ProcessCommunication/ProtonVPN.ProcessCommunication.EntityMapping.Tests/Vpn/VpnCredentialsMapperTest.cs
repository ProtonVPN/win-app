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
using NSubstitute;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Crypto.Contracts;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Auth;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Crypto;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;
using ProtonVPN.ProcessCommunication.EntityMapping.Vpn;

namespace ProtonVPN.ProcessCommunication.EntityMapping.Tests.Vpn;

[TestClass]
public class VpnCredentialsMapperTest
{
    private IEntityMapper _entityMapper;
    private VpnCredentialsMapper _mapper;

    private AsymmetricKeyPairIpcEntity _expectedAsymmetricKeyPairIpcEntity;
    private AsymmetricKeyPair _expectedAsymmetricKeyPair;

    [TestInitialize]
    public void Initialize()
    {
        _entityMapper = Substitute.For<IEntityMapper>();
        _mapper = new(_entityMapper);

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
        _entityMapper = null;
        _mapper = null;

        _expectedAsymmetricKeyPairIpcEntity = null;
        _expectedAsymmetricKeyPair = null;
    }

    [TestMethod]
    public void TestMapLeftToRight_WithUsernameAndPassword()
    {
        VpnCredentials entityToTest = new(DateTime.UtcNow.Ticks.ToString(), DateTime.UtcNow.Millisecond.ToString());

        VpnCredentialsIpcEntity result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(entityToTest.Username, result.Username);
        Assert.AreEqual(entityToTest.Password, result.Password);
        Assert.IsNull(result.Certificate);
    }

    [TestMethod]
    public void TestMapLeftToRight_WithCertificate()
    {
        VpnCredentials entityToTest = new("CERT", DateTime.UtcNow.AddDays(1), new AsymmetricKeyPair(
            new SecretKey("PVPN", KeyAlgorithm.Ed25519), new PublicKey("PVPN", KeyAlgorithm.Ed25519)));

        VpnCredentialsIpcEntity result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.IsNull(result.Username);
        Assert.IsNull(result.Password);
        Assert.AreEqual(entityToTest.ClientCertificatePem, result.Certificate.Pem);
        Assert.AreEqual(entityToTest.ClientCertificateExpirationDateUtc, result.Certificate.ExpirationDateUtc);
        Assert.AreEqual(_expectedAsymmetricKeyPairIpcEntity, result.ClientKeyPair);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestMapRightToLeft_ThrowsWhenNull()
    {
        VpnCredentialsIpcEntity entityToTest = null;

        _mapper.Map(entityToTest);
    }

    [TestMethod]
    public void TestMapRightToLeft_WithUsernameAndPassword()
    {
        VpnCredentialsIpcEntity entityToTest = new()
        {
            Username = DateTime.UtcNow.Ticks.ToString(),
            Password = DateTime.UtcNow.Millisecond.ToString()
        };

        VpnCredentials result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(entityToTest.Username, result.Username);
        Assert.AreEqual(entityToTest.Password, result.Password);
        Assert.IsNull(result.ClientCertificatePem);
        Assert.IsFalse(result.IsCertificateCredentials);
    }

    [TestMethod]
    public void TestMapRightToLeft_WithCertificate()
    {
        VpnCredentialsIpcEntity entityToTest = new()
        {
            Certificate = CreateCertificate(),
            ClientKeyPair = new AsymmetricKeyPairIpcEntity()
        };

        VpnCredentials result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.IsNull(result.Username);
        Assert.IsNull(result.Password);
        Assert.AreEqual(entityToTest.Certificate.Pem, result.ClientCertificatePem);
        Assert.AreEqual(entityToTest.Certificate.ExpirationDateUtc, result.ClientCertificateExpirationDateUtc);
        Assert.AreEqual(_expectedAsymmetricKeyPair, result.ClientKeyPair);
        Assert.IsTrue(result.IsCertificateCredentials);
    }

    private ConnectionCertificateIpcEntity CreateCertificate()
    {
        return new()
        {
            Pem = DateTime.UtcNow.Ticks.ToString(),
            ExpirationDateUtc = DateTime.UtcNow.AddDays(1),
        };
    }
}