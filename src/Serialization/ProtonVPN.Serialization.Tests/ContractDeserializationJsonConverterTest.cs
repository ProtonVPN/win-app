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
using ProtonVPN.Serialization.Contracts;
using ProtonVPN.Serialization.Contracts.Json;
using ProtonVPN.Serialization.Json;
using ProtonVPN.Serialization.Tests.Mocks;

namespace ProtonVPN.Serialization.Tests;

[TestClass]
public class ContractDeserializationJsonConverterTest
{
    [TestMethod]
    public void CanConvert()
    {
        // Arrange
        List<InterfaceImplementation> interfaceImplementations1 = new()
        {
            InterfaceImplementation.Create<ISubscription,Subscription>(),
            InterfaceImplementation.Create<IUser,User>(),
        };
        IJsonContractDeserializer jsonContractDeserializer1 = Substitute.For<IJsonContractDeserializer>();
        jsonContractDeserializer1.Get().Returns(interfaceImplementations1);

        List<InterfaceImplementation> interfaceImplementations2 = new()
        {
            InterfaceImplementation.Create<IMachine,Machine>(),
            InterfaceImplementation.Create<ICacheFile,CacheFile>(),
        };
        IJsonContractDeserializer jsonContractDeserializer2 = Substitute.For<IJsonContractDeserializer>();
        jsonContractDeserializer2.Get().Returns(interfaceImplementations2);

        List<IJsonContractDeserializer> jsonContractDeserializers = new()
        {
            jsonContractDeserializer1,
            jsonContractDeserializer2,
        };
        ContractDeserializationJsonConverter jsonConverter = new(jsonContractDeserializers);

        // Act and Assert for each
        Assert.IsTrue(jsonConverter.CanConvert(typeof(ISubscription)));
        Assert.IsTrue(jsonConverter.CanConvert(typeof(IUser)));
        Assert.IsTrue(jsonConverter.CanConvert(typeof(IMachine)));
        Assert.IsTrue(jsonConverter.CanConvert(typeof(ICacheFile)));
        Assert.IsFalse(jsonConverter.CanConvert(typeof(IDevice)));
    }

    [TestMethod]
    public void CanConvert_FailsWithDuplicateContractsFromDifferentJsonContractDeserializers()
    {
        // Arrange
        List<InterfaceImplementation> interfaceImplementations1 = new()
        {
            InterfaceImplementation.Create<IDevice,Device>(),
            InterfaceImplementation.Create<ISubscription,Subscription>(),
            InterfaceImplementation.Create<IUser,User>(),
        };
        IJsonContractDeserializer jsonContractDeserializer1 = Substitute.For<IJsonContractDeserializer>();
        jsonContractDeserializer1.Get().Returns(interfaceImplementations1);

        List<InterfaceImplementation> interfaceImplementations2 = new()
        {
            InterfaceImplementation.Create<IMachine,Machine>(),
            InterfaceImplementation.Create<ISubscription,Subscription>(),
            InterfaceImplementation.Create<ICacheFile,CacheFile>(),
        };
        IJsonContractDeserializer jsonContractDeserializer2 = Substitute.For<IJsonContractDeserializer>();
        jsonContractDeserializer2.Get().Returns(interfaceImplementations2);

        List<IJsonContractDeserializer> jsonContractDeserializers = new()
        {
            jsonContractDeserializer1,
            jsonContractDeserializer2,
        };

        // Act
        Exception exception = Assert.ThrowsException<Exception>(() => new ContractDeserializationJsonConverter(jsonContractDeserializers));

        Assert.IsTrue(exception.Message.Equals($"There is more than a single JSON deserialization implementation" +
                $"for the contract {typeof(ISubscription).FullName}."));
    }

    [TestMethod]
    public void CanConvert_FailsWithDuplicateContractsFromSameJsonContractDeserializer()
    {
        // Arrange
        List<InterfaceImplementation> interfaceImplementations = new()
        {
            InterfaceImplementation.Create<ISubscription,Subscription>(),
            InterfaceImplementation.Create<IDevice,Device>(),
            InterfaceImplementation.Create<IUser,User>(),
            InterfaceImplementation.Create<IMachine,Machine>(),
            InterfaceImplementation.Create<IDevice,Device>(),
        };
        IJsonContractDeserializer jsonContractDeserializer = Substitute.For<IJsonContractDeserializer>();
        jsonContractDeserializer.Get().Returns(interfaceImplementations);

        List<IJsonContractDeserializer> jsonContractDeserializers = new()
        {
            jsonContractDeserializer,
        };

        // Act
        Exception exception = Assert.ThrowsException<Exception>(() => new ContractDeserializationJsonConverter(jsonContractDeserializers));

        Assert.IsTrue(exception.Message.Equals($"There is more than a single JSON deserialization implementation" +
                $"for the contract {typeof(IDevice).FullName}."));
    }
}
