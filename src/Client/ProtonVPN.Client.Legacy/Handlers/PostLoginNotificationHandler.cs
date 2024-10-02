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

using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Announcements.Contracts;
using ProtonVPN.Client.Logic.Announcements.Contracts.Entities;
using ProtonVPN.Client.Logic.Announcements.Contracts.Messages;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Legacy.Models.Activation;
using ProtonVPN.Client.Legacy.UI.Announcements.Modals;

namespace ProtonVPN.Client.Legacy.Handlers;

public class PostLoginNotificationHandler : IHandler,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<AnnouncementListChangedMessage>
{
    private readonly IAnnouncementsProvider _announcementsProvider;
    private readonly IDialogActivator _dialogActivator;
    private readonly IUIThreadDispatcher _uiThreadDispatcher;

    private bool _isToTriggerOneTimeAnnouncements;

    public PostLoginNotificationHandler(IAnnouncementsProvider announcementsProvider,
        IDialogActivator dialogActivator,
        IUIThreadDispatcher uiThreadDispatcher)
    {
        _announcementsProvider = announcementsProvider;
        _dialogActivator = dialogActivator;
        _uiThreadDispatcher = uiThreadDispatcher;
    }

    public void Receive(LoggedInMessage message)
    {
        _isToTriggerOneTimeAnnouncements = true;
    }

    public void Receive(AnnouncementListChangedMessage message)
    {
        if (_isToTriggerOneTimeAnnouncements)
        {
            _isToTriggerOneTimeAnnouncements = false;

            Announcement? announcement = _announcementsProvider.GetActiveAndUnseenByType(AnnouncementType.OneTime);

            if (announcement is not null)
            {
                _uiThreadDispatcher.TryEnqueue(_dialogActivator.ShowDialog<AnnouncementModalViewModel>);
            }
        }
    }
}
