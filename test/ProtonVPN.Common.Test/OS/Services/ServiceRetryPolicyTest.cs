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
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Common.Abstract;
using ProtonVPN.Common.OS.Services;
using ProtonVPN.Common.Threading;
using ProtonVPN.Test.Common.Breakpoints;

namespace ProtonVPN.Common.Test.OS.Services
{
    [TestClass]
    public class ServiceRetryPolicyTest
    {
        private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(5);

        [TestMethod]
        public async Task ExecuteAsync_ShouldBe_ActionResult()
        {
            // Arrange
            var expected = Result.Ok();
            var policy = new ServiceRetryPolicy(2, TimeSpan.Zero);

            // Act
            var result = await policy.ExecuteAsync( ct => Task.FromResult(expected), CancellationToken.None);

            // Assert
            result.Should().Be(expected);
        }

        [TestMethod]
        public async Task ExecuteAsync_ShouldBeFailure_WhenActionReturnsFailure()
        {
            // Arrange
            var policy = new ServiceRetryPolicy(0, TimeSpan.Zero);

            // Act
            var result = await policy.ExecuteAsync(ct => Task.FromResult(Result.Fail()), CancellationToken.None);

            // Assert
            result.Failure.Should().BeTrue();
        }

        [TestMethod]
        public async Task ExecuteAsync_ShouldCallAction_RetryCountPlusOneTime_WhenActionReturnsFailure()
        {
            // Arrange
            const int retryCount = 2;
            var timesCalled = 0;
            var policy = new ServiceRetryPolicy(retryCount, TimeSpan.Zero);

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
            var policy = new ServiceRetryPolicy(0, TimeSpan.Zero);
            using (var cancellationSource = new CancellationTokenSource())
            using (var breakpoint = new Breakpoint())
            {
                var cancelled = false;

                // Act
                var task =  policy.ExecuteAsync(
                    async ct =>
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        await breakpoint.Hit().WaitForContinue();
                        cancelled = ct.IsCancellationRequested;
                        ct.ThrowIfCancellationRequested();
                        return Result.Ok();
                    },
                    cancellationSource.Token);

                var hit = await breakpoint.WaitForHit().TimeoutAfter(TestTimeout);
                cancellationSource.Cancel();
                hit.Continue();
                var result = await task.TimeoutAfter(TestTimeout);

                // Assert
                result.Success.Should().BeFalse();
                cancelled.Should().BeTrue();
            }
        }
    }
}
