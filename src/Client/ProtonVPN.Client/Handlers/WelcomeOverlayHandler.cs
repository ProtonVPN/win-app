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
using ProtonVPN.Client.Contracts.Messages;
using ProtonVPN.Client.Core.Messages;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Handlers.Bases;
using ProtonVPN.Client.Logic.Announcements.Contracts;
using ProtonVPN.Client.Logic.Announcements.Contracts.Entities;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;

namespace ProtonVPN.Client.Handlers;

public class WelcomeOverlayHandler : IHandler,
    IEventMessageReceiver<HomePageDisplayedAfterLoginMessage>,
    IEventMessageReceiver<VpnPlanChangedMessage>,
    IEventMessageReceiver<MainWindowVisibilityChangedMessage>
{
    private readonly ISettings _settings;
    private readonly IUIThreadDispatcher _uiThreadDispatcher;
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;
    private readonly IMainWindowActivator _mainWindowActivator;
    private readonly IAnnouncementsProvider _announcementsProvider;
    private readonly IOneTimeAnnouncementWindowActivator _oneTimeAnnouncementWindowActivator;
    private readonly INpsSurveyWindowActivator _npsSurveyWindowActivator;
    private readonly IUserAuthenticator _userAuthenticator;

    public WelcomeOverlayHandler(
        ISettings settings,
        IUIThreadDispatcher uiThreadDispatcher,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IMainWindowActivator mainWindowActivator,
        IAnnouncementsProvider announcementsProvider,
        IOneTimeAnnouncementWindowActivator oneTimeAnnouncementWindowActivator,
        INpsSurveyWindowActivator npsSurveyWindowActivator,
        IUserAuthenticator userAuthenticator)
    {
        _settings = settings;
        _uiThreadDispatcher = uiThreadDispatcher;
        _mainWindowOverlayActivator = mainWindowOverlayActivator;
        _mainWindowActivator = mainWindowActivator;
        _announcementsProvider = announcementsProvider;
        _oneTimeAnnouncementWindowActivator = oneTimeAnnouncementWindowActivator;
        _npsSurveyWindowActivator = npsSurveyWindowActivator;
        _userAuthenticator = userAuthenticator;
    }

    public void Receive(HomePageDisplayedAfterLoginMessage message)
    {
        _uiThreadDispatcher.TryEnqueue(async () =>
        {
            if (_settings.LastSeenWhatsNewOverlayVersion < DefaultSettings.WhatsNewOverlayVersion)
            {
                _settings.LastSeenWhatsNewOverlayVersion = DefaultSettings.WhatsNewOverlayVersion;
                await _mainWindowOverlayActivator.ShowWhatsNewOverlayAsync();
            }
            else if (!_settings.VpnPlan.IsB2B && !_settings.WasWelcomeOverlayDisplayed)
            {
                _settings.WasWelcomeOverlayDisplayed = true;
                await _mainWindowOverlayActivator.ShowWelcomeOverlayAsync();
            }
            else if (_settings.VpnPlan.IsB2B && !_settings.WasWelcomeB2BOverlayDisplayed)
            {
                _settings.WasWelcomeB2BOverlayDisplayed = true;
                await _mainWindowOverlayActivator.ShowWelcomeToVpnB2BOverlayAsync();
            }
            else
            {
                HandleAnnouncements();
            }
        });
    }

    private void HandleAnnouncements()
    {
        if (!_mainWindowActivator.IsWindowVisible || !_userAuthenticator.IsLoggedIn)
        {
            return;
        }

        Announcement? announcement = _announcementsProvider.GetActiveAndUnseenByType(AnnouncementType.OneTime);
        if (announcement is not null)
        {
            _oneTimeAnnouncementWindowActivator.Activate();
            return;
        }

        Announcement? npsSurvey = _announcementsProvider.GetActiveAndUnseenByType(AnnouncementType.NpsSurvey);
        if (npsSurvey is not null)
        {
            _announcementsProvider.MarkAsSeen(npsSurvey.Id);
            _npsSurveyWindowActivator.Activate();
        }
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        if (!_userAuthenticator.IsLoggedIn)
        {
            return;
        }

        _uiThreadDispatcher.TryEnqueue(async () =>
        {
            if (_settings.VpnPlan.IsPlus && !_settings.WasWelcomePlusOverlayDisplayed)
            {
                _settings.WasWelcomePlusOverlayDisplayed = true;
                await _mainWindowOverlayActivator.ShowWelcomeToVpnPlusOverlayAsync();
            }
            else if (_settings.VpnPlan.IsUnlimited && !_settings.WasWelcomeUnlimitedOverlayDisplayed)
            {
                _settings.WasWelcomeUnlimitedOverlayDisplayed = true;
                await _mainWindowOverlayActivator.ShowWelcomeToVpnUnlimitedOverlayAsync();
            }
        });
    }

    public void Receive(MainWindowVisibilityChangedMessage message)
    {
        _uiThreadDispatcher.TryEnqueue(HandleAnnouncements);
    }
}