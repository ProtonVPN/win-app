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
using ProtonVPN.Common.Abstract;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.OS.Services;
using ProtonVPN.Tests.Common.Breakpoints;

namespace ProtonVPN.Common.Tests.OS.Services
{
    [TestClass]
    public class ServiceRetryPolicyTest
    {
        private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(5);

        [TestMethod]
        public async Task ExecuteAsync_ShouldBe_ActionResult()
        {
            // Arrange
            Result expected = Result.Ok();
            ServiceRetryPolicy policy = new(2, TimeSpan.Zero);

            // Act
            Result result = await policy.ExecuteAsync( ct => Task.FromResult(expected), CancellationToken.None);

            // Assert
            result.Should().Be(expected);
        }

        [TestMethod]
        public async Task ExecuteAsync_ShouldBeFailure_WhenActionReturnsFailure()
        {
            // Arrange
            ServiceRetryPolicy policy = new(0, TimeSpan.Zero);

            // Act
            Result result = await policy.ExecuteAsync(ct => Task.FromResult(Result.Fail()), CancellationToken.None);

            // Assert
            result.Failure.Should().BeTrue();
        }

        [TestMethod]
        public async Task ExecuteAsync_ShouldCallAction_RetryCountPlusOneTime_WhenActionReturnsFailure()
        {
            // Arrange
            const int retryCount = 2;
            int timesCalled = 0;
            ServiceRetryPolicy policy = new(retryCount, TimeSpan.Zero);

            // Act
            await policy.ExecuteAsync(
                ct =>
                {
                    timesCalled++;
                    return Task.FromResult(Result.Fail());
                },
                CancellationToken.None);

            // Assert
            timesCalled.Should().Be(1 + retryCount);
        }

        [TestMethod]
        public async Task ExecuteAsync_ShouldCancelAction_WhenCancellationToken_IsCancelled()
        {
            // Arrange
            ServiceRetryPolicy policy = new(0, TimeSpan.Zero);
            using (CancellationTokenSource cancellationSource = new())
            using (Breakpoint breakpoint = new())
            {
                bool cancelled = false;

                // Act
                Task<Result> task =  policy.ExecuteAsync(
                    async ct =>
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        await breakpoint.Hit().WaitForContinue();
                        cancelled = ct.IsCancellationRequested;
                        ct.ThrowIfCancellationRequested();
                        return Result.Ok();
                    },
                    cancellationSource.Token);

                BreakpointHit hit = await breakpoint.WaitForHit().TimeoutAfter(TestTimeout);
                cancellationSource.Cancel();
                hit.Continue();
                Result result = await task.TimeoutAfter(TestTimeout);

                // Assert
                result.Success.Should().BeFalse();
                cancelled.Should().BeTrue();
            }
        }
    }
}
