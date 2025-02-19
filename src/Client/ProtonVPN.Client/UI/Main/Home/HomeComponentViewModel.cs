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

using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Updates.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.Home;

public partial class HomeComponentViewModel : ActivatableViewModelBase,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<ClientUpdateStateChangedMessage>
{
    private readonly IConnectionManager _connectionManager;
    private readonly IUpdatesManager _updatesManager;

    public bool IsUpdateAvailable => _updatesManager.IsUpdateAvailable;

    public bool IsConnected => _connectionManager.IsConnected;

    public bool IsConnecting => _connectionManager.IsConnecting;

    public bool IsDisconnected => _connectionManager.IsDisconnected;

    public HomeComponentViewModel(
            ILocalizationProvider localizer,
            ILogger logger,
            IIssueReporter issueReporter,
            IConnectionManager connectionManager,
            IUpdatesManager updatesManager)
        : base(localizer, logger, issueReporter)
    {
        _connectionManager = connectionManager;
        _updatesManager = updatesManager;
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateCurrentConnectionStatus);
        }
    }

    public void Receive(ClientUpdateStateChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateUpdateStatus);
        }
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        InvalidateCurrentConnectionStatus();
        InvalidateUpdateStatus();
    }

    private void InvalidateCurrentConnectionStatus()
    {
        OnPropertyChanged(nameof(IsConnected));
        OnPropertyChanged(nameof(IsConnecting));
        OnPropertyChanged(nameof(IsDisconnected));
    }

    private void InvalidateUpdateStatus()
    {
        OnPropertyChanged(nameof(IsUpdateAvailable));
    }
}