/*
 * Copyright (c) 2024 Proton AG
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
using ProtonVPN.Client.Localization.Contracts.Messages;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Logic.Announcements.Observers;

public class AnnouncementsObserver : PollingObserverBase, IObserver,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<LoggedOutMessage>,
    IEventMessageReceiver<LanguageChangedMessage>,
    IEventMessageReceiver<VpnPlanChangedMessage>
{
    private readonly IAnnouncementsUpdater _announcementsUpdater;
    private readonly IConfiguration _config;

    protected override TimeSpan PollingInterval => _config.AnnouncementsUpdateInterval;

    public AnnouncementsObserver(ILogger logger,
        IIssueReporter issueReporter,
        IAnnouncementsUpdater announcementsUpdater,
        IConfiguration config)
        : base(logger, issueReporter)
    {
        _announcementsUpdater = announcementsUpdater;
        _config = config;
    }

    public void Receive(LoggedInMessage message)
    {
        StartTimerAndTriggerOnStart();
    }

    public void Receive(LoggedOutMessage message)
    {
        StopTimer();
    }

    protected override async Task OnTriggerAsync()
    {
        await _announcementsUpdater.UpdateAsync();
    }

    public void Receive(LanguageChangedMessage message)
    {
        TriggerIfTimerIsEnabled();
    }

    private void TriggerIfTimerIsEnabled()
    {
        if (IsTimerEnabled)
        {
            TriggerAction.Run();
        }
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        TriggerIfTimerIsEnabled();
    }
}