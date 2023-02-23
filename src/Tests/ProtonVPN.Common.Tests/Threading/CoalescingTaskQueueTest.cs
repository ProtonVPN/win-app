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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Threading;
using ProtonVPN.Tests.Common.Breakpoints;

namespace ProtonVPN.Common.Tests.Threading
{
    [TestClass]
    public class CoalescingTaskQueueTest
    {
        private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(5);

        [TestMethod]
        public async Task Enqueue_ShouldSchedule_NewTask()
        {
            // Arrange
            const int expected = 397;
            CoalescingTaskQueue<int, int> queue = new CoalescingTaskQueue<int, int>((newArg, arg, running) => CoalesceDecision.Join);

            // Act
            int result = await queue.Enqueue(() => expected, expected);
            
            // Assert
            result.Should().Be(expected);
        }

        [TestMethod]
        public async Task Enqueue_ShouldJoin_RunningTask()
        {
            // Arrange
            const int expected = 147;
            using (Breakpoint breakpoint = new())
            {
                CoalescingTaskQueue<int, int> queue = new CoalescingTaskQueue<int, int>((newArg, arg, running) => CoalesceDecision.Join);

                async Task<int> TestAction(int result)
                {
                    // ReSharper disable once AccessToDisposedClosure
                    await breakpoint.Hit().WaitForContinue();
                    return result;
                }

                // Act
                Task<int> task1 = queue.Enqueue(() => TestAction(expected), expected);
                BreakpointHit hit1 = await breakpoint.WaitForHit().TimeoutAfter(TestTimeout);

                Task<int> task2 = queue.Enqueue(() => TestAction(328), 328);

                hit1.Continue();

                // Assert
                task1.Result.Should().Be(expected);
                task2.Result.Should().Be(expected);
            }
        }

        [TestMethod]
        public async Task Enqueue_ShouldQueue_PendingTask()
        {
            // Arrange
            const int expected1 = 291;
            const int expected2 = 872;
            using (Breakpoint breakpoint = new())
            {
                CoalescingTaskQueue<int, int> queue = new CoalescingTaskQueue<int, int>((newArg, arg, running) => CoalesceDecision.None);

                async Task<int> TestAction(int result)
                {
                    // ReSharper disable once AccessToDisposedClosure
                    await breakpoint.Hit().WaitForContinue();
                    return result;
                }

                // Act
                Task<int> task1 = queue.Enqueue(() => TestAction(expected1), expected1);
                Task<int> task2 = queue.Enqueue(() => TestAction(expected2), expected2);
                await breakpoint.WaitForHitAndContinue().TimeoutAfter(TestTimeout);
                await breakpoint.WaitForHitAndContinue().TimeoutAfter(TestTimeout);

                // Assert
                task1.Result.Should().Be(expected1);
                task2.Result.Should().Be(expected2);
            }
        }

        [TestMethod]
        public async Task Enqueue_ShouldJoin_PendingTask()
        {
            // Arrange
            const int expected1 = 3905;
            const int expected2 = 4487;
            using (Breakpoint breakpoint = new())
            {
                CoalescingTaskQueue<int, int> queue = new CoalescingTaskQueue<int, int>((newArg, arg, running) =>
                    newArg > 0 ? CoalesceDecision.None : CoalesceDecision.Join);

                async Task<int> TestAction(int result, CancellationToken ct)
                {
                    // ReSharper disable once AccessToDisposedClosure
                    await breakpoint.Hit().WaitForContinue();
                    ct.ThrowIfCancellationRequested();
                    return result;
                }

                // Act
                Task<int> task1 = queue.Enqueue(ct => TestAction(expected1, ct), expected1);
                BreakpointHit hit1 = await breakpoint.WaitForHit().TimeoutAfter(TestTimeout);
                Task<int> task2 = queue.Enqueue(ct => TestAction(expected2, ct), expected2);

                Task<int> task3 = queue.Enqueue(ct => TestAction(-15, ct), -15);

                hit1.Continue();
                await breakpoint.WaitForHitAndContinue().TimeoutAfter(TestTimeout);

                // Assert
                task1.Result.Should().Be(expected1);
                task2.Result.Should().Be(expected2);
                task3.Result.Should().Be(expected2);
            }
        }

        [TestMethod]
        public async Task Enqueue_ShouldCancel_RunningTask()
        {
            // Arrange
            const int expected2 = 4487;
            using (Breakpoint breakpoint = new())
            {
                CoalescingTaskQueue<int, int> queue = new CoalescingTaskQueue<int, int>((newArg, arg, running) => 
                    CoalesceDecision.Cancel);

                async Task<int> TestAction(int result, CancellationToken ct)
                {
                    // ReSharper disable once AccessToDisposedClosure
                    await breakpoint.Hit().WaitForContinue();
                    ct.ThrowIfCancellationRequested();
                    return result;
                }

                // Act
                Task<int> task1 = queue.Enqueue(ct => TestAction(101, ct), 101);
                BreakpointHit hit1 = await breakpoint.WaitForHit().TimeoutAfter(TestTimeout);
                Task<int> task2 = queue.Enqueue(ct => TestAction(expected2, ct), expected2);
                hit1.Continue();
                await breakpoint.WaitForHitAndContinue().TimeoutAfter(TestTimeout);

                // Assert
                task1.IsCanceled.Should().BeTrue();
                task2.Result.Should().Be(expected2);
            }
        }

        [TestMethod]
        public async Task Enqueue_ShouldCancel_PendingTask()
        {
            // Arrange
            const int expected1 = 657;
            const int expected3 = 134;
            using (Breakpoint breakpoint = new())
            {
                CoalescingTaskQueue<int, int> queue = new CoalescingTaskQueue<int, int>((newArg, arg, running) => CoalesceDecision.None);

                async Task<int> TestAction(int result, CancellationToken ct)
                {
                    // ReSharper disable once AccessToDisposedClosure
                    await breakpoint.Hit().WaitForContinue();
                    ct.ThrowIfCancellationRequested();
                    return result;
                }

                // Act
                Task<int> task1 = queue.Enqueue(ct => TestAction(expected1, ct), expected1);
                BreakpointHit hit1 = await breakpoint.WaitForHit().TimeoutAfter(TestTimeout);
                Task<int> task2 = queue.Enqueue(ct => TestAction(101, ct), 101);
                Task<int> task3 = queue.Enqueue(ct => TestAction(expected3, ct), expected3);
                hit1.Continue();
                await breakpoint.WaitForHitAndContinue().TimeoutAfter(TestTimeout);

                // Assert
                task1.Result.Should().Be(expected1);
                task2.IsCanceled.Should().BeTrue();
                task3.Result.Should().Be(expected3);
            }
        }

        [TestMethod]
        public async Task Enqueue_ShouldCancel_PendingAndRunningTasks()
        {
            // Arrange
            const int expected3 = 3617;
            using (Breakpoint breakpoint = new())
            {
                CoalescingTaskQueue<int, int> queue = new CoalescingTaskQueue<int, int>((newArg, arg, running) => 
                    newArg == expected3 ? CoalesceDecision.Cancel : CoalesceDecision.None);

                async Task<int> TestAction(int result, CancellationToken ct)
                {
                    // ReSharper disable once AccessToDisposedClosure
                    await breakpoint.Hit().WaitForContinue();
                    ct.ThrowIfCancellationRequested();
                    return result;
                }

                // Act
                Task<int> task1 = queue.Enqueue(ct => TestAction(202, ct), 202);
                BreakpointHit hit1 = await breakpoint.WaitForHit().TimeoutAfter(TestTimeout);
                Task<int> task2 = queue.Enqueue(ct => TestAction(303, ct), 303);
                Task<int> task3 = queue.Enqueue(ct => TestAction(expected3, ct), expected3);
                hit1.Continue();
                await breakpoint.WaitForHitAndContinue().TimeoutAfter(TestTimeout);

                // Assert
                task1.IsCanceled.Should().BeTrue();
                task2.IsCanceled.Should().BeTrue();
                task3.Result.Should().Be(expected3);
            }
        }

        [TestMethod]
        public async Task Enqueue_ShouldNotJoin_CancelledRunningTask()
        {
            // Arrange
            const int expected = 6874;
            using (Breakpoint breakpoint = new())
            {
                CoalescingTaskQueue<int, int> queue = new CoalescingTaskQueue<int, int>((newArg, arg, running) => 
                    CoalesceDecision.Join);

                async Task<int> TestAction(int result, CancellationToken ct)
                {
                    // ReSharper disable once AccessToDisposedClosure
                    await breakpoint.Hit().WaitForContinue();
                    ct.ThrowIfCancellationRequested();
                    return result;
                }

                // Act
                _ = queue.Enqueue(ct => TestAction(1981, ct), 1981);
                BreakpointHit hit1 = await breakpoint.WaitForHit().TimeoutAfter(TestTimeout);
                queue.Cancel();
                Task<int> task2 = queue.Enqueue(ct => TestAction(expected, ct), expected);
                hit1.Continue();
                await breakpoint.WaitForHitAndContinue().TimeoutAfter(TestTimeout);

                // Assert
                task2.Result.Should().Be(expected);
            }
        }

        [TestMethod]
        public void Cancel_ShouldSucceed_WhenNoTasksRunning()
        {
            // Arrange
            CoalescingTaskQueue<int, int> queue = new CoalescingTaskQueue<int, int>((newArg, arg, running) => 
                CoalesceDecision.None);

            // Act
            Action action = () => queue.Cancel();

            // Assert
            action.Should().NotThrow();
        }

        [TestMethod]
        public async Task Cancel_ShouldCancel_RunningTask()
        {
            // Arrange
            using (Breakpoint breakpoint = new())
            {
                CoalescingTaskQueue<int, int> queue = new CoalescingTaskQueue<int, int>((newArg, arg, running) => 
                    CoalesceDecision.Join);

                async Task<int> TestAction(int result, CancellationToken ct)
                {
                    // ReSharper disable once AccessToDisposedClosure
                    await breakpoint.Hit().WaitForContinue();
                    ct.ThrowIfCancellationRequested();
                    return result;
                }

                // Act
                Task<int> task = queue.Enqueue(ct => TestAction(9874, ct), 9874);
                BreakpointHit hit = await breakpoint.WaitForHit().TimeoutAfter(TestTimeout);
                queue.Cancel();
                hit.Continue();
                await Task.WhenAny(task);

                // Assert
                task.IsCanceled.Should().BeTrue();
            }
        }

        [TestMethod]
        public async Task Cancel_ShouldCancel_PendingAndRunningTasks()
        {
            // Arrange
            using (Breakpoint breakpoint = new())
            {
                CoalescingTaskQueue<int, int> queue = new CoalescingTaskQueue<int, int>((newArg, arg, running) => 
                    CoalesceDecision.None);

                async Task<int> TestAction(int result, CancellationToken ct)
                {
                    // ReSharper disable once AccessToDisposedClosure
                    await breakpoint.Hit().WaitForContinue();
                    ct.ThrowIfCancellationRequested();
                    return result;
                }

                // Act
                Task<int> task1 = queue.Enqueue(ct => TestAction(6517, ct), 6517);
                BreakpointHit hit1 = await breakpoint.WaitForHit().TimeoutAfter(TestTimeout);
                Task<int> task2 = queue.Enqueue(ct => TestAction(101, ct), 101);

                queue.Cancel();

                hit1.Continue();
                await Task.WhenAny(task1);

                // Assert
                task1.IsCanceled.Should().BeTrue();
                task2.IsCanceled.Should().BeTrue();
            }
        }

        [TestMethod]
        public async Task Cancel_ShouldCancel_PendingAndRunningTasks_UnderConcurrency()
        {
            // Arrange
            using (Breakpoint breakpoint = new())
            {
                CoalescingTaskQueue<int, int> queue = new CoalescingTaskQueue<int, int>((newArg, arg, running) => 
                    CoalesceDecision.None);

                async Task<int> TestAction(int result, CancellationToken ct)
                {
                    // ReSharper disable once AccessToDisposedClosure
                    await breakpoint.Hit().WaitForContinue();
                    ct.ThrowIfCancellationRequested();
                    return result;
                }

                // Act
                Task<int> task1 = queue.Enqueue(ct => TestAction(6517, ct), 6517);
                BreakpointHit hit1 = await breakpoint.WaitForHit().TimeoutAfter(TestTimeout);
                Task<int> task2 = queue.Enqueue(ct => TestAction(101, ct), 101);

                Task[] cancelTasks = Enumerable.Range(1, 10)
                                               .Select(i => Task.Run(() => queue.Cancel()))
                                               .ToArray();

                await Task.WhenAny(cancelTasks);
                hit1.Continue();
                await Task.WhenAll(cancelTasks);
                await Task.WhenAny(task1);

                // Assert
                task1.IsCanceled.Should().BeTrue();
                task2.IsCanceled.Should().BeTrue();
            }
        }
    }
}
