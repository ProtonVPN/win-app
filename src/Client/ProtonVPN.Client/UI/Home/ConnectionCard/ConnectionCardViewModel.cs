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

using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Recents.Contracts;
using ProtonVPN.Client.Logic.Recents.Contracts.Messages;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Home.ConnectionCard;

public class ConnectionCardViewModel : ConnectionCardViewModelBase,
    IEventMessageReceiver<RecentConnectionsChanged>
{
    private readonly IRecentConnectionsProvider _recentConnectionsProvider;

    public ConnectionCardViewModel(
        IConnectionManager connectionManager,
        IRecentConnectionsProvider recentConnectionsProvider,
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter,
        HomeViewModel homeViewModel)
        : base(connectionManager, localizationProvider, logger, issueReporter, homeViewModel)
    {
        _recentConnectionsProvider = recentConnectionsProvider;

        InvalidateCurrentConnectionStatus();
    }

    public void Receive(RecentConnectionsChanged message)
    {
        ExecuteOnUIThread(InvalidateCurrentConnectionIntent);
    }

    protected override void InvalidateCurrentConnectionStatus()
    {
        InvalidateCurrentConnectionIntent();
        base.InvalidateCurrentConnectionStatus();
    }

    private void InvalidateCurrentConnectionIntent()
    {
        CurrentConnectionIntent = ConnectionManager.IsDisconnected || ConnectionManager.CurrentConnectionIntent == null
            ? _recentConnectionsProvider.GetMostRecentConnection()?.ConnectionIntent
            : ConnectionManager.CurrentConnectionIntent;
    }
}