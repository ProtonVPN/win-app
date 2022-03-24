/*
 * Copyright (c) 2022 Proton Technologies AG
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
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Update.Helpers;

namespace ProtonVPN.Update.Test.Helpers
{
    [TestClass]
    public class ExceptionExtensionsTest
    {
        [TestMethod]
        public void IsCommunicationException_ShouldBe_When_Exception()
        {
            TestData[] testData =
            {
                new TestData(true, new HttpRequestException()),
                new TestData(true, new OperationCanceledException()),
                new TestData(true, new SocketException()),
                new TestData(false, new IOException()),
                new TestData(false, new UnauthorizedAccessException()),
                new TestData(false, new Win32Exception()),
                new TestData(false, new AppUpdateException("")),
                new TestData(false, new Exception()),
            };

            foreach (var data in testData)
            {
                var result = data.Exception.IsCommunicationException();

                result.Should().Be(data.Expected, $"{data.Exception.GetType()} is not communication exception");
            }
        }

        [TestMethod]
        public void IsProcessException_ShouldBe_When_Exception()
        {
            TestData[] testData =
            {
                new TestData(false, new HttpRequestException()),
                new TestData(false, new OperationCanceledException()),
                new TestData(true, new SocketException()),  // SocketException is Win32Exception
                new TestData(false, new IOException()),
                new TestData(false, new UnauthorizedAccessException()),
                new TestData(true, new Win32Exception()),
                new TestData(false, new AppUpdateException("")),
                new TestData(false, new Exception()),
            };

            foreach (var data in testData)
            {
                var result = data.Exception.IsProcessException();

                result.Should().Be(data.Expected, $"{data.Exception.GetType()} is not process exception");
            }
        }

        #region Helpers

        private class TestData
        {
            public readonly bool Expected;
            public readonly Exception Exception;

            public TestData(bool expected, Exception exception)
            {
                Expected = expected;
                Exception = exception;
            }
        }

        #endregion
    }
}
