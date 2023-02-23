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
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Vpn.PortMapping.Messages;
using ProtonVPN.Vpn.PortMapping.Serializers;
using ProtonVPN.Vpn.PortMapping.Serializers.Common;

namespace ProtonVPN.Vpn.Tests.PortMapping.Serializers.Common
{
    [TestClass]
    public class MessageSerializerFactoryTest
    {
        private List<IMessageSerializer> _messageSerializers;

        [TestInitialize]
        public void TestInitialize()
        {
            HelloQueryMessageSerializer helloQueryMessageSerializer = new();
            HelloReplyMessageSerializer helloReplyMessageSerializer = new();
            PortMappingQueryMessageSerializer portMappingQueryMessageSerializer = new();
            PortMappingReplyMessageSerializer portMappingReplyMessageSerializer = new();
            _messageSerializers = new() 
            { 
                helloQueryMessageSerializer, 
                helloReplyMessageSerializer, 
                portMappingQueryMessageSerializer, 
                portMappingReplyMessageSerializer
            };
        }

        [TestMethod]
        public void TestConstructor()
        {
            MessageSerializerFactory factory = new(_messageSerializers);
        }

        [TestMethod]
        public void TestGet_HelloQueryMessage()
        {
            MessageSerializerFactory factory = new(_messageSerializers);
            IMessageSerializer serializer = factory.Get<HelloQueryMessage>();
            serializer.Should().BeOfType<HelloQueryMessageSerializer>();
        }

        [TestMethod]
        public void TestGet_HelloReplyMessage()
        {
            MessageSerializerFactory factory = new(_messageSerializers);
            IMessageSerializer serializer = factory.Get<HelloReplyMessage>();
            serializer.Should().BeOfType<HelloReplyMessageSerializer>();
        }

        [TestMethod]
        public void TestGet_PortMappingQueryMessage()
        {
            MessageSerializerFactory factory = new(_messageSerializers);
            IMessageSerializer serializer = factory.Get<PortMappingQueryMessage>();
            serializer.Should().BeOfType<PortMappingQueryMessageSerializer>();
        }

        [TestMethod]
        public void TestGet_PortMappingReplyMessage()
        {
            MessageSerializerFactory factory = new(_messageSerializers);
            IMessageSerializer serializer = factory.Get<PortMappingReplyMessage>();
            serializer.Should().BeOfType<PortMappingReplyMessageSerializer>();
        }
    }
}