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
using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Api.Handlers.StackBuilders;
using ProtonVPN.Api.Tests.Mocks;

namespace ProtonVPN.Api.Tests.Handlers.StackBuilders
{
    [TestClass]
    public class HttpMessageHandlerStackBuilderTest
    {
        [TestMethod]
        public void TestAddLastHandler()
        {
            MockOfWebRequestHandler mockOfWebRequestHandler = new();

            HttpMessageHandler resultHandler = new HttpMessageHandlerStackBuilder()
                .AddLastHandler(mockOfWebRequestHandler)
                .Build();

            Assert.AreEqual(resultHandler, mockOfWebRequestHandler);
        }

        [TestMethod]
        public void TestAddLastHandler_FailsWithDelegatingHandler()
        {
            MockOfDelegatingHandler mockOfDelegatingHandler = new();

            Action action = () => new HttpMessageHandlerStackBuilder()
                .AddLastHandler(mockOfDelegatingHandler);

            Assert.ThrowsException<ArgumentException>(action);
        }

        [TestMethod]
        public void TestAddLastHandler_FailsWithNullArgument()
        {
            Action action = () => new HttpMessageHandlerStackBuilder()
                .AddLastHandler(null);

            Assert.ThrowsException<ArgumentNullException>(action);
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(12)]
        public void TestAddDelegatingHandler(int numDelegatingHandlers)
        {
            // Arrange
            List<MockOfDelegatingHandler> mockOfDelegatingHandlers = new();
            for (int i = 0; i < numDelegatingHandlers; i++)
            {
                mockOfDelegatingHandlers.Add(new MockOfDelegatingHandler());
            }
            MockOfWebRequestHandler mockOfWebRequestHandler = new();

            // Act
            HttpMessageHandlerStackBuilder stackBuilder = new HttpMessageHandlerStackBuilder();
            foreach (MockOfDelegatingHandler mockOfDelegatingHandler in mockOfDelegatingHandlers)
            {
                stackBuilder = stackBuilder.AddDelegatingHandler(mockOfDelegatingHandler);
            }
            HttpMessageHandler resultHandler = stackBuilder
                .AddLastHandler(mockOfWebRequestHandler)
                .Build();

            // Assert
            HttpMessageHandler handlerToEvaluate = resultHandler;
            foreach (MockOfDelegatingHandler mockOfDelegatingHandler in mockOfDelegatingHandlers)
            {
                Assert.AreEqual(mockOfDelegatingHandler, handlerToEvaluate);
                handlerToEvaluate = mockOfDelegatingHandler.InnerHandler;
            }
            Assert.AreEqual(mockOfWebRequestHandler, handlerToEvaluate);
        }
    }
}