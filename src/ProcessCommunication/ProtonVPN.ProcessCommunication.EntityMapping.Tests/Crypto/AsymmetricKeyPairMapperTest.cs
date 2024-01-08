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
using ProtonVPN.Crypto.Contracts;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Crypto;
using ProtonVPN.ProcessCommunication.EntityMapping.Crypto;

namespace ProtonVPN.ProcessCommunication.EntityMapping.Tests.Crypto;

[TestClass]
public class AsymmetricKeyPairMapperTest
{
    private IEntityMapper _entityMapper;
    private AsymmetricKeyPairMapper _mapper;

    private SecretKeyIpcEntity _expectedSecretKeyIpcEntity;
    private PublicKeyIpcEntity _expectedPublicKeyIpcEntity;
    private SecretKey _expectedSecretKey;
    private PublicKey _expectedPublicKey;

    [TestInitialize]
    public void Initialize()
    {
        _entityMapper = Substitute.For<IEntityMapper>();
        _mapper = new AsymmetricKeyPairMapper(_entityMapper);

        _expectedSecretKeyIpcEntity = new SecretKeyIpcEntity();
        _entityMapper.Map<SecretKey, SecretKeyIpcEntity>(Arg.Any<SecretKey>()).Returns(_expectedSecretKeyIpcEntity);
        _expectedPublicKeyIpcEntity = new PublicKeyIpcEntity();
        _entityMapper.Map<PublicKey, PublicKeyIpcEntity>(Arg.Any<PublicKey>()).Returns(_expectedPublicKeyIpcEntity);

        _expectedSecretKey = new SecretKey("PVPN", KeyAlgorithm.Unknown);
        _entityMapper.Map<SecretKeyIpcEntity, SecretKey>(Arg.Any<SecretKeyIpcEntity>()).Returns(_expectedSecretKey);
        _expectedPublicKey = new PublicKey("PVPN", KeyAlgorithm.Unknown);
        _entityMapper.Map<PublicKeyIpcEntity, PublicKey>(Arg.Any<PublicKeyIpcEntity>()).Returns(_expectedPublicKey);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _entityMapper = null;
        _mapper = null;
    }

    [TestMethod]
    public void TestMapLeftToRight_WhenNull()
    {
        AsymmetricKeyPair entityToMap = null;

        AsymmetricKeyPairIpcEntity result = _mapper.Map(entityToMap);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void TestMapLeftToRight()
    {
        AsymmetricKeyPair entityToMap = new(
            new SecretKey("PVPN", KeyAlgorithm.Unknown),
            new PublicKey("PVPN", KeyAlgorithm.Unknown));

        AsymmetricKeyPairIpcEntity result = _mapper.Map(entityToMap);

        Assert.IsNotNull(result);
        Assert.AreEqual(_expectedSecretKeyIpcEntity, result.SecretKey);
        Assert.AreEqual(_expectedPublicKeyIpcEntity, result.PublicKey);
    }

    [TestMethod]
    public void TestMapRightToLeft_WhenNull()
    {
        TestMapRightToLeft(null, null);
    }

    [TestMethod]
    public void TestMapRightToLeft_WhenInnerPropertiesAreNull()
    {
        TestMapRightToLeft(null, new AsymmetricKeyPairIpcEntity());
    }

    [TestMethod]
    public void TestMapRightToLeft()
    {
        SecretKey secretKey = new("PVPN", KeyAlgorithm.Unknown);
        AsymmetricKeyPairIpcEntity entityToMap = new()
        {
            SecretKey = new SecretKeyIpcEntity()
            {
                Bytes = secretKey.Bytes,
                Base64 = secretKey.Base64,
                Algorithm = (KeyAlgorithmIpcEntity)(int)secretKey.Algorithm,
                Pem = secretKey.Pem,
            },
            PublicKey = new PublicKeyIpcEntity()
            {
                Bytes = secretKey.Bytes,
                Base64 = secretKey.Base64,
                Algorithm = (KeyAlgorithmIpcEntity)(int)secretKey.Algorithm,
                Pem = secretKey.Pem,
            }
        };

        AsymmetricKeyPair result = _mapper.Map(entityToMap);

        Assert.IsNotNull(result);
        Assert.AreEqual(_expectedSecretKey, result.SecretKey);
        Assert.AreEqual(_expectedPublicKey, result.PublicKey);
    }

    private void TestMapRightToLeft(AsymmetricKeyPair expectedResult, AsymmetricKeyPairIpcEntity entityToMap)
    {
        AsymmetricKeyPair result = _mapper.Map(entityToMap);

        Assert.AreEqual(expectedResult, result);
    }
}