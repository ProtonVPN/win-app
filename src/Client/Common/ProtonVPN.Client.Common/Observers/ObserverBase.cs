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

using ProtonVPN.Common.Core.Threading;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Common.Observers;

public abstract class ObserverBase : IObserver
{
    protected readonly ILogger Logger;
    protected readonly IIssueReporter IssueReporter;
    protected readonly SingleAction TriggerAction;

    public ObserverBase(ILogger logger, IIssueReporter issueReporter)
    {
        Logger = logger;
        IssueReporter = issueReporter;

        TriggerAction = new SingleAction(OnSafeTriggerAsync);
    }

    private async Task OnSafeTriggerAsync()
    {
        try
        {
            await OnTriggerAsync();
        }
        catch (Exception ex)
        {
            Logger.Error<AppLog>("A loose exception was caught from a triggered task of Observer.", ex);
            IssueReporter.CaptureError(ex);
        }
    }

    protected abstract Task OnTriggerAsync();
}