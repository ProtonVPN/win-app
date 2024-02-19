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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using NSubstitute;
using ProtonVPN.Serialization.Contracts;
using ProtonVPN.Serialization.Contracts.Json;
using ProtonVPN.Serialization.Tests.Mocks;

namespace ProtonVPN.Serialization.Tests;

[TestClass]
public class JsonSerializerTest
{
    [TestMethod]
    public void SerializeToString_ShouldSerialize_ToJson()
    {
        // Arrange
        SampleContract value = new() { Number = 34, Name = "ZgD" };
        IJsonSerializer serializer = new Json.JsonSerializer(new List<IJsonContractDeserializer>());

        // Act
        string result = serializer.SerializeToString(value);

        // Assert
        result.Should().Be("{\"Number\":34,\"Name\":\"ZgD\"}");
    }

    [TestMethod]
    public void DeserializeFromString_ShouldHave_ExpectedPropertyValues()
    {
        // Arrange
        string json = "{\"Number\":199,\"Name\":\"Super Mario\"}";
        IJsonSerializer serializer = new Json.JsonSerializer(new List<IJsonContractDeserializer>());

        // Act
        SampleContract? result = serializer.DeserializeFromString<SampleContract>(json);

        // Assert
        result?.Number.Should().Be(199);
        result?.Name.Should().Be("Super Mario");
    }

    [TestMethod]
    public void DeserializeFromString_ShouldThrow_ExpectedException_WhenNotCorrectJson()
    {
        // Arrange
        string json = "{Segment";
        IJsonSerializer serializer = new Json.JsonSerializer(new List<IJsonContractDeserializer>());

        // Act
        Action action = () => serializer.DeserializeFromString<SampleContract>(json);

        // Assert
        action.Should().Throw<JsonException>();
    }

    [TestMethod]
    public void DeserializeFromString_ShouldDeserializeExplicitContracts()
    {
        // Arrange
        string json = "{\"Name\":\"Hulk\"}";

        List<InterfaceImplementation> interfaceImplementations = new() { InterfaceImplementation.Create<IUser,User>() };
        IJsonContractDeserializer jsonContractDeserializer = Substitute.For<IJsonContractDeserializer>();
        jsonContractDeserializer.Get().Returns(interfaceImplementations);
        IJsonSerializer serializer = new Json.JsonSerializer(new List<IJsonContractDeserializer>() { jsonContractDeserializer });

        // Act
        IUser user = serializer.DeserializeFromString<IUser>(json)!;

        // Assert
        user!.Name.Should().Be("Hulk");
    }

    [TestMethod]
    public void DeserializeFromString_ShouldNotDeserializeNonExplicitContracts()
    {
        // Arrange
        string json = "{\"Name\":\"Hulk\"}";
        IJsonSerializer serializer = new Json.JsonSerializer(new List<IJsonContractDeserializer>());

        // Act
        Exception exception = Assert.ThrowsException<JsonSerializationException>(() => serializer.DeserializeFromString<IUser>(json));
        Assert.IsTrue(exception.Message.Contains($"Could not create an instance of type {typeof(IUser).FullName}. " +
            $"Type is an interface or abstract class and cannot be instantiated."));
    }

    [TestMethod]
    public void DeserializeFirstLevel()
    {
        // Arrange
        string json = "{\"User\":{\"Name\":\"Harry Potter\"},\"Device\":{\"Code\":1893}}";

        List<InterfaceImplementation> interfaceImplementations = new() { /*InterfaceImplementation.Create<IUser, User>()*/ };
        IJsonContractDeserializer jsonContractDeserializer = Substitute.For<IJsonContractDeserializer>();
        jsonContractDeserializer.Get().Returns(interfaceImplementations);
        IJsonSerializer serializer = new Json.JsonSerializer(new List<IJsonContractDeserializer>() { jsonContractDeserializer });

        // Act
        IDictionary<string, string?> configJsonProperties = serializer.DeserializeFirstLevel(json)!;

        // Assert
        Assert.AreEqual(2, configJsonProperties.Count);
        Assert.IsTrue(configJsonProperties.ContainsKey("User"));
        Assert.AreEqual("{\"Name\":\"Harry Potter\"}", configJsonProperties["User"]);
        Assert.IsTrue(configJsonProperties.ContainsKey("Device"));
        Assert.AreEqual("{\"Code\":1893}", configJsonProperties["Device"]);
    }
}