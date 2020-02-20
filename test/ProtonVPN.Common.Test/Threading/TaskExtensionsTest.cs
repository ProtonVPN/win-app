/*
 * Copyright (c) 2020 Proton Technologies AG
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
using ProtonVPN.Test.Common.Breakpoints;
using TaskExtensions = ProtonVPN.Common.Threading.TaskExtensions;

namespace ProtonVPN.Common.Test.Threading
{
    [TestClass]
    public class TaskExtensionsTest
    {
        private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(5);

        [TestMethod]
        public async Task TimeoutAfter_ShouldSucceed_WhenActionSucceeds()
        {
            // Arrange
            Task Action(CancellationToken ct)
            {
                ct.ThrowIfCancellationRequested();
                return Task.CompletedTask;
            }

            // Act
            Func<Task> action = async () => await TaskExtensions.TimeoutAfter(Action, TimeSpan.FromSeconds(10), CancellationToken.None);

            // Assert
            await action.Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task TimeoutAfter_ShouldThrow_TaskCanceledException_WhenActionCancelled()
        {
            // Arrange
            var breakpoint = new Breakpoint();
            var cancellationSource = new CancellationTokenSource();
            async Task Action(CancellationToken ct)
            {
                await breakpoint.Hit().WaitForContinue();
                ct.ThrowIfCancellationRequested();
            }

            // Act
            var task = TaskExtensions.TimeoutAfter(Action, TimeSpan.Zero, cancellationSource.Token);
            var hit = await breakpoint.WaitForHit().TimeoutAfter(TestTimeout);

            cancellationSource.Cancel();
            hit.Continue();
            Func<Task> action = async () => await task.TimeoutAfter(TestTimeout);

            // Assert
            await action.Should().ThrowAsync<OperationCanceledException>();
        }

        [TestMethod]
        public async Task TimeoutAfter_ShouldThrow_TimeoutException_WhenActionTimedOut()
        {
            // Arrange
            var breakpoint = new Breakpoint();
            async Task Action(CancellationToken ct)
            {
                await breakpoint.Hit().WaitForContinue();
                ct.ThrowIfCancellationRequested();
            }

            // Act
            var task = TaskExtensions.TimeoutAfter(Action, TimeSpan.Zero, CancellationToken.None);
            var hit = await breakpoint.WaitForHit().TimeoutAfter(TestTimeout);

            await Task.Delay(TimeSpan.FromMilliseconds(100));
            hit.Continue();

            Func<Task> action = async () => await task;

            // Assert
            await action.Should().ThrowAsync<TimeoutException>();
        }

        [TestMethod]
        public async Task TimeoutAfter_ShouldThrow_WhenActionThrows()
        {
            // Arrange
            Task Action(CancellationToken ct)
            {
                throw new Exception();
            }

            // Act
            Func<Task> action = async () => await TaskExtensions.TimeoutAfter(Action, TimeSpan.FromSeconds(10), CancellationToken.None);

            // Assert
            await action.Should().ThrowAsync<Exception>();
        }
    }
}
