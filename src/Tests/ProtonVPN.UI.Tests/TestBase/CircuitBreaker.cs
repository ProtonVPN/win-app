/*
 * Copyright (c) 2026 Proton AG
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
using NUnit.Framework;
using NUnit.Framework.Interfaces;

[assembly: NonParallelizable]
[assembly: LevelOfParallelism(1)]
[assembly: CircuitBreaker]

public sealed class CircuitBreakerAttribute : Attribute, ITestAction
{
    private static readonly int _failureLimit = int.TryParse(Environment.GetEnvironmentVariable("FAILURE_LIMIT"), out int value) ? value : 7;
    private static readonly object _lock = new();

    private static int _consecutiveFailures;
    private static bool _isCategoryAborted;

    private static readonly string _category = Environment.GetEnvironmentVariable("CATEGORY") ?? "?";

    public ActionTargets Targets => ActionTargets.Test;

    public void BeforeTest(ITest test)
    {
        lock (_lock)
        {
            if (_isCategoryAborted)
            {
                Assert.Ignore($"Skipping Test - Category {_category} had " +
                    $"{_failureLimit} consecutive failures.");
            }
        }
    }

    public void AfterTest(ITest test)
    {
        TestStatus status = TestContext.CurrentContext.Result.Outcome.Status;

        lock (_lock)
        {
            switch (status)
            {
                case TestStatus.Failed:
                    _consecutiveFailures++;
                    break;

                case TestStatus.Passed:
                    _consecutiveFailures = 0;
                    break;

                case TestStatus.Skipped:
                case TestStatus.Inconclusive:
                case TestStatus.Warning:
                    break;
            }

            if (_consecutiveFailures >= _failureLimit)
            {
                _isCategoryAborted = true;
            }
        }
    }
}