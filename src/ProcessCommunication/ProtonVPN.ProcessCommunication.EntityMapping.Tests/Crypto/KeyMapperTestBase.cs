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

namespace ProtonVPN.ProcessCommunication.EntityMapping.Tests.Crypto;

[TestClass]
public abstract class KeyMapperTestBase<TKey, TKeyIpcEntity>
    where TKey : Key
    where TKeyIpcEntity : KeyIpcEntity, new()
{
    private IMapper<TKey, TKeyIpcEntity> _mapper;

    [TestInitialize]
    public void Initialize()
    {
        _mapper = CreateKeyMapper();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _mapper = null;
    }

    [TestMethod]
    public void TestMapLeftToRight_WhenNull()
    {
        TKey entityToMap = null;

        TKeyIpcEntity result = _mapper.Map(entityToMap);

        Assert.AreEqual(null, result);
    }

    [TestMethod]
    public void TestMapLeftToRight()
    {
        TKey entityToMap = CreateKey("PVPN", KeyAlgorithm.Ed25519);

        TKeyIpcEntity result = _mapper.Map(entityToMap);

        Assert.IsNotNull(result);
        Assert.AreEqual(entityToMap.Bytes, result.Bytes);
        Assert.AreEqual(entityToMap.Base64, result.Base64);
        Assert.AreEqual((int)entityToMap.Algorithm, (int)result.Algorithm);
        Assert.AreEqual(entityToMap.Pem, result.Pem);
    }

    [TestMethod]
    public void TestMapRightToLeft_WhenNull()
    {
        TKeyIpcEntity entityToMap = null;

        TKey result = _mapper.Map(entityToMap);

        Assert.AreEqual(null, result);
    }

    [TestMethod]
    public void TestMapRightToLeft()
    {
        string base64 = "PVPN";
        byte[] bytes = Convert.FromBase64String(base64);
        TKeyIpcEntity entityToMap = new()
        {
            Bytes = bytes,
            Base64 = base64,
            Algorithm = KeyAlgorithmIpcEntity.X25519,
            Pem = CreateExpectedPem(base64)
        };

        TKey result = _mapper.Map(entityToMap);

        Assert.IsNotNull(result);
        Assert.AreEqual(entityToMap.Bytes.Count(), result.Bytes.Count());
        for (int i = 0; i < entityToMap.Bytes.Length; i++)
        {
            Assert.AreEqual(entityToMap.Bytes[i], result.Bytes[i]);
        }
        Assert.AreEqual(entityToMap.Base64, result.Base64);
        Assert.AreEqual((int)entityToMap.Algorithm, (int)result.Algorithm);
        Assert.AreEqual(entityToMap.Pem, result.Pem);
    }

    [TestMethod]
    public void TestMapRightToLeft_WhenBytesAreNull()
    {
        string base64 = "PVPN";
        byte[] bytes = Convert.FromBase64String(base64);
        TKeyIpcEntity entityToMap = new()
        {
            Base64 = base64,
            Algorithm = KeyAlgorithmIpcEntity.X25519,
            Pem = CreateExpectedPem(base64)
        };

        TKey result = _mapper.Map(entityToMap);

        Assert.IsNotNull(result);
        Assert.AreEqual(bytes.Count(), result.Bytes.Count());
        for (int i = 0; i < bytes.Length; i++)
        {
            Assert.AreEqual(bytes[i], result.Bytes[i]);
        }
        Assert.AreEqual(entityToMap.Base64, result.Base64);
        Assert.AreEqual((int)entityToMap.Algorithm, (int)result.Algorithm);
        Assert.AreEqual(entityToMap.Pem, result.Pem);
    }

    [TestMethod]
    public void TestMapRightToLeft_WhenBase64IsNull()
    {
        string base64 = "PVPN";
        byte[] bytes = Convert.FromBase64String(base64);
        TKeyIpcEntity entityToMap = new()
        {
            Bytes = bytes,
            Algorithm = KeyAlgorithmIpcEntity.X25519,
            Pem = CreateExpectedPem(base64)
        };

        TKey result = _mapper.Map(entityToMap);

        Assert.IsNotNull(result);
        Assert.AreEqual(entityToMap.Bytes.Count(), result.Bytes.Count());
        for (int i = 0; i < entityToMap.Bytes.Length; i++)
        {
            Assert.AreEqual(entityToMap.Bytes[i], result.Bytes[i]);
        }
        Assert.AreEqual(base64, result.Base64);
        Assert.AreEqual((int)entityToMap.Algorithm, (int)result.Algorithm);
        Assert.AreEqual(entityToMap.Pem, result.Pem);
    }

    protected abstract IMapper<TKey, TKeyIpcEntity> CreateKeyMapper();

    protected abstract TKey CreateKey(string base64, KeyAlgorithm algorithm);

    protected abstract string CreateExpectedPem(string base64);
}