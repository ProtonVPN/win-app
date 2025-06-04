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

using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Announcements.Contracts;
using ProtonVPN.Client.Logic.Announcements.Contracts.Entities;
using ProtonVPN.Client.Logic.Announcements.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Updates.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Extensions;
using ProtonVPN.Client.Settings.Contracts.Messages;

namespace ProtonVPN.Client.UI.Main.Home;

public partial class HomeComponentViewModel : ActivatableViewModelBase,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<ClientUpdateStateChangedMessage>,
    IEventMessageReceiver<AnnouncementListChangedMessage>
{
    private readonly ISettings _settings;
    private readonly IConnectionManager _connectionManager;
    private readonly IUpdatesManager _updatesManager;
    private readonly IAnnouncementsProvider _announcementsProvider;

    public bool IsUpdateAvailable => _updatesManager.IsUpdateAvailable && _announcementsProvider.GetActiveAndUnseenByType(AnnouncementType.ProminentBanner) is null;

    public bool IsConnected => _connectionManager.IsConnected;

    public bool IsConnecting => _connectionManager.IsConnecting;

    public bool IsNotConnectedAndAdvancedKillSwitchActive => !IsConnected && _settings.IsAdvancedKillSwitchActive();

    public bool IsDisconnected => _connectionManager.IsDisconnected;

    public HomeComponentViewModel(
        ISettings settings,
        IConnectionManager connectionManager,
        IUpdatesManager updatesManager,
        IAnnouncementsProvider announcementsProvider,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        _settings = settings;
        _connectionManager = connectionManager;
        _updatesManager = updatesManager;
        _announcementsProvider = announcementsProvider;
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

    public void Receive(AnnouncementListChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateUpdateStatus);
        }
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName is nameof(ISettings.KillSwitchMode) or nameof(ISettings.IsKillSwitchEnabled))
        {
            ExecuteOnUIThread(InvalidateCurrentConnectionStatus);
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
        OnPropertyChanged(nameof(IsNotConnectedAndAdvancedKillSwitchActive));
    }

    private void InvalidateUpdateStatus()
    {
        OnPropertyChanged(nameof(IsUpdateAvailable));
    }
}