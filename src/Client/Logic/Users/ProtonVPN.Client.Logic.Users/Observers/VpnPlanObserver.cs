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

using ProtonVPN.Client.Common.Observers;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Users.Contracts;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Logic.Users.Observers;

public class VpnPlanObserver : PollingObserverBase, IObserver,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<LoggedOutMessage>
{
    private readonly IVpnPlanUpdater _vpnPlanUpdater;
    private readonly IConfiguration _config;

    protected override TimeSpan PollingInterval => _config.VpnPlanRequestInterval;

    public VpnPlanObserver(ILogger logger,
        IIssueReporter issueReporter,
        IVpnPlanUpdater vpnPlanUpdater,
        IConfiguration config)
        : base(logger, issueReporter)
    {
        _vpnPlanUpdater = vpnPlanUpdater;
        _config = config;
    }

    public void Receive(LoggedInMessage message)
    {
        StartTimer();
    }

    public void Receive(LoggedOutMessage message)
    {
        StopTimer();
    }

    protected override async Task OnTriggerAsync()
    {
        await _vpnPlanUpdater.UpdateAsync();
    }
}