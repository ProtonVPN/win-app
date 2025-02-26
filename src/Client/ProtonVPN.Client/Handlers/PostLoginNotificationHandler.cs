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

using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Core.Messages;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Handlers.Bases;
using ProtonVPN.Client.Logic.Announcements.Contracts;
using ProtonVPN.Client.Logic.Announcements.Contracts.Entities;

namespace ProtonVPN.Client.Handlers;

public class PostLoginNotificationHandler : IHandler,
    IEventMessageReceiver<HomePageDisplayedAfterLoginMessage>
{
    private readonly IAnnouncementsProvider _announcementsProvider;
    private readonly IOneTimeAnnouncementWindowActivator _oneTimeAnnouncementWindowActivator;
    private readonly IUIThreadDispatcher _uiThreadDispatcher;

    public PostLoginNotificationHandler(
        IAnnouncementsProvider announcementsProvider,
        IOneTimeAnnouncementWindowActivator oneTimeAnnouncementWindowActivator,
        IUIThreadDispatcher uiThreadDispatcher)
    {
        _announcementsProvider = announcementsProvider;
        _oneTimeAnnouncementWindowActivator = oneTimeAnnouncementWindowActivator;
        _uiThreadDispatcher = uiThreadDispatcher;
    }

    public void Receive(HomePageDisplayedAfterLoginMessage message)
    {
        Announcement? announcement = _announcementsProvider.GetActiveAndUnseenByType(AnnouncementType.OneTime);
        if (announcement is not null)
        {
            _uiThreadDispatcher.TryEnqueue(_oneTimeAnnouncementWindowActivator.Activate);
        }
    }
}