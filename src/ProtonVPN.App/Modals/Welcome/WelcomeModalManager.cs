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

using System;
using System.Linq;
using ProtonVPN.Announcements.Contracts;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Models;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Windows.Popups;
using ProtonVPN.Modals.Upsell;
using ProtonVPN.Windows.Popups.Offers;
using ProtonVPN.Windows.Popups.Rebranding;

namespace ProtonVPN.Modals.Welcome
{
    public class WelcomeModalManager : ILogoutAware
    {
        private readonly Random _random = new();
        private readonly IAppSettings _appSettings;
        private readonly IUserStorage _userStorage;
        private readonly IPopupWindows _popupWindows;
        private readonly IModals _modals;
        private readonly IAnnouncementService _announcementService;
        private readonly OfferPopupViewModel _offerPopupViewModel;

        public WelcomeModalManager(
            IAppSettings appSettings,
            IUserStorage userStorage,
            IPopupWindows popupWindows,
            IModals modals,
            IAnnouncementService announcementService,
            OfferPopupViewModel offerPopupViewModel)
        {
            _appSettings = appSettings;
            _userStorage = userStorage;
            _popupWindows = popupWindows;
            _modals = modals;
            _announcementService = announcementService;
            _offerPopupViewModel = offerPopupViewModel;
        }

        public void Load()
        {
            User user = _userStorage.GetUser();
            if (WelcomeModalHasToBeShown())
            {
                ShowWelcomeModal();
            }
            else if (_appSettings.IsToShowRebrandingPopup)
            {
                ShowRebrandingPopup();
            }
            else if (!user.Paid() && !_userStorage.GetUser().IsDelinquent())
            {
                ShowUpsellModal();
            }
        }

        private void ShowUpsellModal()
        {
            Announcement announcement = _announcementService.Get()
                .FirstOrDefault(a => a.Type == (int)AnnouncementType.OneTime && !a.Seen);
            if (announcement != null)
            {
                ShowUpsellOffer(announcement);
            }
            else
            {
                ShowStandardUpsellModal();
            }
        }

        private void ShowUpsellOffer(Announcement announcement)
        {
            _announcementService.MarkAsSeen(announcement.Id);
            _offerPopupViewModel.Panel = announcement.Panel;
            _popupWindows.Show<OfferPopupViewModel>();
        }

        private void ShowStandardUpsellModal()
        {
            int randomNumber = _random.Next(0, 100);
            if (randomNumber >= 15)
            {
                return;
            }

            _modals.Show<UpsellModalViewModel>();
        }

        private void ShowWelcomeModal()
        {
            _modals.Show<WelcomeModalViewModel>();
            _appSettings.WelcomeModalShown = true;
        }

        private void ShowRebrandingPopup()
        {
            _popupWindows.Show<RebrandingPopupViewModel>();
            _appSettings.IsToShowRebrandingPopup = false;
        }

        private bool WelcomeModalHasToBeShown()
        {
            return !_appSettings.WelcomeModalShown;
        }

        public void OnUserLoggedOut()
        {
            if (_popupWindows.IsOpen<OfferPopupViewModel>())
            {
                _popupWindows.Close<OfferPopupViewModel>();
            }
        }
    }
}