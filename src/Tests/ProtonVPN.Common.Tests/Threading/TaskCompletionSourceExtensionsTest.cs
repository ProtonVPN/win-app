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
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Common.Threading;

namespace ProtonVPN.Common.Tests.Threading
{
    [TestClass]
    public class TaskCompletionSourceExtensionsTest
    {
        [TestMethod]
        public async Task Wrap_ShouldSetResult()
        {
            // Arrange
            const int expected = 157;
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
            // Act
            await tcs.Wrap(() => Task.FromResult(expected));
            // Assert
            tcs.Task.Result.Should().Be(expected);
        }

        [TestMethod]
        public async Task Wrap_ShouldSetCanceled_WhenFunction_IsCancelledAsync()
        {
            // Arrange
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
            // Act
            await tcs.Wrap(() => throw new OperationCanceledException());
            // Assert
            tcs.Task.IsCanceled.Should().BeTrue();
        }

        [TestMethod]
        public async Task Wrap_ShouldSetCanceled_WhenFunction_IsCanceled()
        {
            // Arrange
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
            // Act
            await tcs.Wrap(() => Task.FromCanceled<int>(new(true)));
            // Assert
            tcs.Task.IsCanceled.Should().BeTrue();
        }

        [TestMethod]
        public async Task Wrap_ShouldSetException_WhenFunction_Throws()
        {
            // Arrange
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
            // Act
            await tcs.Wrap(() => throw new());
            // Assert
            tcs.Task.IsFaulted.Should().BeTrue();
        }

        [TestMethod]
        public async Task Wrap_ShouldSetException_WhenFunction_ThrowsAsync()
        {
            // Arrange
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
            // Act
            await tcs.Wrap(() => Task.FromException<int>(new()));
            // Assert
            tcs.Task.IsFaulted.Should().BeTrue();
        }
    }
}
