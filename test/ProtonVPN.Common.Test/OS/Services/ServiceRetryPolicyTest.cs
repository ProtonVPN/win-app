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
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Common.Abstract;
using ProtonVPN.Common.OS.Services;

namespace ProtonVPN.Common.Test.OS.Services
{
    [TestClass]
    public class ServiceRetryPolicyTest
    {
        [TestMethod]
        public void Execute_ShouldBeSuccess_WhenActionReturnsSuccess()
        {
            // Arrange
            var policy = new ServiceRetryPolicy(2, TimeSpan.Zero);

            // Act
            var result = policy.Value().Execute(Result.Ok);

            // Assert
            result.Success.Should().BeTrue();
        }

        [TestMethod]
        public void Execute_ShouldBeFailure_WhenActionReturnsFailure()
        {
            // Arrange
            var policy = new ServiceRetryPolicy(2, TimeSpan.Zero);

            // Act
            var result = policy.Value().Execute(() => Result.Fail());

            // Assert
            result.Failure.Should().BeTrue();
        }

        [TestMethod]
        public void Execute_ShouldCallAction_RetryCountPlusOneTime_WhenActionReturnsFailure()
        {
            // Arrange
            const int retryCount = 2;
            var timesCalled = 0;
            var policy = new ServiceRetryPolicy(retryCount, TimeSpan.Zero);

            // Act
            policy.Value().Execute(() =>
            {
                timesCalled++;
                return Result.Fail();
            });

            // Assert
            timesCalled.Should().Be(1 + retryCount);
        }
    }
}
