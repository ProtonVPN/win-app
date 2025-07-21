/*
 * Copyright (c) 2025 Proton AG
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

using System.Runtime.CompilerServices;
using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;
using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Services.Dispatching;

public class UIThreadDispatcher : IUIThreadDispatcher
{
    private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    private readonly ILogger _logger;
    private readonly IIssueReporter _issueReporter;

    public UIThreadDispatcher(
        ILogger logger, 
        IIssueReporter issueReporter)
    {
        _logger = logger;
        _issueReporter = issueReporter;
    }

    public bool TryEnqueue(Action callback,
        [CallerFilePath] string sourceFilePath = "",
        [CallerMemberName] string sourceMemberName = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        return _dispatcherQueue.TryEnqueue(() => ExecuteSafely(callback, sourceFilePath, sourceMemberName, sourceLineNumber));
    }

    public Task<bool> TryEnqueueAsync(Func<Task> callback,
        [CallerFilePath] string sourceFilePath = "",
        [CallerMemberName] string sourceMemberName = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        return _dispatcherQueue.EnqueueAsync(async () => await ExecuteSafelyAsync(callback, sourceFilePath, sourceMemberName, sourceLineNumber));
    }

    public IDispatcherTimer GetTimer(TimeSpan interval)
    {
        return new DispatcherTimer(interval);
    }

    private void ExecuteSafely(Action callback, string sourceFilePath, string sourceMemberName, int sourceLineNumber)
    {
        try
        {
            callback();
        }
        catch (Exception ex)
        {
            _logger.Error<AppLog>($"Exception handled by {nameof(UIThreadDispatcher)} {nameof(ExecuteSafely)}.",
                ex, sourceFilePath, sourceMemberName, sourceLineNumber);
            _issueReporter.CaptureError(ex, sourceFilePath, sourceMemberName, sourceLineNumber);
        }
    }

    private async Task<bool> ExecuteSafelyAsync(Func<Task> task, string sourceFilePath, string sourceMemberName, int sourceLineNumber)
    {
        try
        {
            await task();
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error<AppLog>($"Exception handled by {nameof(UIThreadDispatcher)} {nameof(ExecuteSafelyAsync)}.",
                ex, sourceFilePath, sourceMemberName, sourceLineNumber);
            _issueReporter.CaptureError(ex, sourceFilePath, sourceMemberName, sourceLineNumber);
            return false;
        }
    }
}