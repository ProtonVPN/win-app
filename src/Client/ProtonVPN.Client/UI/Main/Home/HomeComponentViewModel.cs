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
using ProtonVPN.Client.Logic.Updates.Contracts;

namespace ProtonVPN.Client.UI.Main.Home;

public partial class HomeComponentViewModel : ActivatableViewModelBase,
    IEventMessageReceiver<ClientUpdateStateChangedMessage>,
    IEventMessageReceiver<AnnouncementListChangedMessage>
{
    private readonly IUpdatesManager _updatesManager;
    private readonly IAnnouncementsProvider _announcementsProvider;

    public bool IsUpdateAvailable => _updatesManager.IsUpdateAvailable && _announcementsProvider.GetActiveAndUnseenByType(AnnouncementType.ProminentBanner) is null;

    public HomeComponentViewModel(
        IUpdatesManager updatesManager,
        IAnnouncementsProvider announcementsProvider,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        _updatesManager = updatesManager;
        _announcementsProvider = announcementsProvider;
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

    protected override void OnActivated()
    {
        base.OnActivated();

        InvalidateUpdateStatus();
    }

    private void InvalidateUpdateStatus()
    {
        OnPropertyChanged(nameof(IsUpdateAvailable));
    }
}