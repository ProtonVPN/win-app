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
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Recents.Contracts;
using ProtonVPN.Client.Logic.Recents.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Legacy.UI.Home.ConnectionCard;

public class ConnectionCardViewModel : ConnectionCardViewModelBase,
    IEventMessageReceiver<RecentConnectionsChanged>,
    IEventMessageReceiver<SettingChangedMessage>
{
    private readonly IRecentConnectionsManager _recentConnectionsManager;
    private readonly ISettings _settings;

    public ConnectionCardViewModel(
        IConnectionManager connectionManager,
        IRecentConnectionsManager recentConnectionsManager,
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter,
        ISettings settings,
        HomeViewModel homeViewModel)
        : base(connectionManager, localizationProvider, logger, issueReporter, homeViewModel)
    {
        _recentConnectionsManager = recentConnectionsManager;
        _settings = settings;

        InvalidateCurrentConnectionStatus();
    }

    public void Receive(RecentConnectionsChanged message)
    {
        ExecuteOnUIThread(InvalidateCurrentConnectionIntent);
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.DefaultConnection))
        {
            ExecuteOnUIThread(InvalidateCurrentConnectionIntent);
        }
    }

    protected override void InvalidateCurrentConnectionStatus()
    {
        InvalidateCurrentConnectionIntent();
        base.InvalidateCurrentConnectionStatus();
    }

    private void InvalidateCurrentConnectionIntent()
    {
        IConnectionIntent? currentConnectionIntent = ConnectionManager.CurrentConnectionIntent;

        CurrentConnectionIntent = ConnectionManager.IsDisconnected || currentConnectionIntent == null
            ? _recentConnectionsManager.GetDefaultConnection()
            : currentConnectionIntent;

        // If intent is a profile, the reference might be the same even though profile details/settings have changed. Force invalidating all properties.
        InvalidateAllProperties();
    }
}