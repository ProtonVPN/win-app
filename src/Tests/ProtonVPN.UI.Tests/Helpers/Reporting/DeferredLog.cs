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

using System.Collections.Generic;
using NUnit.Framework;

namespace ProtonVPN.UI.Tests.TestsHelper;

/// <summary>
/// Collects log lines produced by retried operations and only writes them to the test output when the test failed.
/// </summary>
public static class DeferredLog
{
    private static readonly object _lock = new();
    private static readonly List<string> _messages = new();

    public static void Add(string message)
    {
        lock (_lock)
        {
            _messages.Add(message);
        }
    }

    public static void Clear()
    {
        lock (_lock)
        {
            _messages.Clear();
        }
    }

    public static void FlushIfTestFailed()
    {
        string[] messages;
        lock (_lock)
        {
            messages = _messages.ToArray();
            _messages.Clear();
        }

        if (TestContext.CurrentContext.Result.Outcome.Status != NUnit.Framework.Interfaces.TestStatus.Failed)
        {
            return;
        }

        foreach (string message in messages)
        {
            TestContext.WriteLine(message);
        }
    }
}
