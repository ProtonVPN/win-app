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
using System.Linq;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Api.Handlers.StackBuilders;
using ProtonVPN.Api.Tests.Mocks;

namespace ProtonVPN.Api.Tests.Handlers.StackBuilders
{
    [TestClass]
    public class CompleteHttpMessageHandlerStackBuilderTest
    {
        [TestMethod]
        public void Test_WithNullDelegatingHandler()
        {
            MockOfWebRequestHandler mockOfWebRequestHandler = new();

            CompleteHttpMessageHandlerStackBuilder completeStackBuilder = new(mockOfWebRequestHandler);
            HttpMessageHandler resultHandler = completeStackBuilder.Build();
            
            Assert.AreEqual(resultHandler, mockOfWebRequestHandler);
        }

        [TestMethod]
        public void Test_FailsWithNullHttpMessageHandler()
        {
            Action action = () => new CompleteHttpMessageHandlerStackBuilder(null, null);

            Assert.ThrowsException<ArgumentNullException>(action);
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(12)]
        public void Test_WithDelegatingHandlers(int numDelegatingHandlers)
        {
            // Arrange
            List<MockOfDelegatingHandler> mockOfDelegatingHandlers = new();
            for (int i = 0; i < numDelegatingHandlers; i++)
            {
                mockOfDelegatingHandlers.Add(new MockOfDelegatingHandler());
            }
            MockOfWebRequestHandler mockOfWebRequestHandler = new();

            // Act
            CompleteHttpMessageHandlerStackBuilder completeStackBuilder = new(mockOfWebRequestHandler, 
                mockOfDelegatingHandlers.Select(dh => (DelegatingHandler)dh).ToList());
            HttpMessageHandler resultHandler = completeStackBuilder.Build();
           
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