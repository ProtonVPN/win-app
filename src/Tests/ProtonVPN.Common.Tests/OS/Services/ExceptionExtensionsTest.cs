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
using System.ComponentModel;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Common.OS.Services;

namespace ProtonVPN.Common.Tests.OS.Services
{
    [TestClass]
    public class ExceptionExtensionsTest
    {
        private const int ServiceAlreadyRunning = 1056;
        private const int ServiceNotRunning = 1062;

        [TestMethod]
        public void IsServiceAlreadyRunning_ShouldBeTrue()
        {
            // Arrange
            InvalidOperationException exception = new("", new Win32Exception(ServiceAlreadyRunning));
            // Act
            bool result = exception.IsServiceAlreadyRunning();
            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void IsServiceAlreadyRunning_ShouldBeFalse_WhenNativeErrorCode_IsDifferent()
        {
            // Arrange
            InvalidOperationException exception = new("", new Win32Exception(1055));
            // Act
            bool result = exception.IsServiceAlreadyRunning();
            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void IsServiceAlreadyRunning_ShouldBeFalse_WhenInnerException_IsNull()
        {
            // Arrange
            InvalidOperationException exception = new();
            // Act
            bool result = exception.IsServiceAlreadyRunning();
            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void IsServiceNotRunning_ShouldBeTrue()
        {
            // Arrange
            InvalidOperationException exception = new("", new Win32Exception(ServiceNotRunning));
            // Act
            bool result = exception.IsServiceNotRunning();
            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void IsServiceNotRunning_ShouldBeFalse_WhenNativeErrorCode_IsDifferent()
        {
            // Arrange
            InvalidOperationException exception = new("", new Win32Exception(1063));
            // Act
            bool result = exception.IsServiceNotRunning();
            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void IsServiceNotRunning_ShouldBeFalse_WhenInnerException_IsNull()
        {
            // Arrange
            InvalidOperationException exception = new();
            // Act
            bool result = exception.IsServiceNotRunning();
            // Assert
            result.Should().BeFalse();
        }
    }
}
