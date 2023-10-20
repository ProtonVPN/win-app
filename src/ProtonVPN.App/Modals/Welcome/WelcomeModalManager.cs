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
using System.ComponentModel;
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
    public class WelcomeModalManager : ILogoutAware, ISettingsAware
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

        private bool IsToShowFreeRescopePopup => _appSettings.FeatureFreeRescopeEnabled &&
                                                 !_appSettings.IsFreeRescopeModalDisplayed &&
                                                 !_userStorage.GetUser().Paid();
        public void Load()
        {
            if (WelcomeModalHasToBeShown())
            {
                ShowWelcomeModal();
            }
            else if (_appSettings.IsToShowRebrandingPopup)
            {
                ShowRebrandingPopup();
            }
            else if (IsToShowFreeRescopePopup)
            {
                ShowFreeRescopeModal();
            }
            else
            {
                ShowUpsellModal();
            }
        }

        private async void ShowFreeRescopeModal()
        {
            if (_appSettings.WelcomeModalShown)
            {
                await _modals.ShowAsync<FreeRescopeModalViewModel>();
                _appSettings.IsFreeRescopeModalDisplayed = true;
            }
        }

        private void ShowUpsellModal()
        {
            User user = _userStorage.GetUser();
            DateTime now = DateTime.UtcNow;
            Announcement announcement = _announcementService.Get()
                .Where(a => a.Type == (int)AnnouncementType.OneTime && !a.Seen && a.StartDateTimeUtc <= now && a.EndDateTimeUtc > now)
                .OrderBy(a => a.EndDateTimeUtc)
                .FirstOrDefault();

            if (announcement != null)
            {
                ShowUpsellOffer(announcement);
            }
            else if (!user.Paid() && !user.IsDelinquent())
            {
                ShowStandardUpsellModal();
            }
        }

        private void ShowUpsellOffer(Announcement announcement)
        {
            _announcementService.MarkAsSeen(announcement.Id);
            _offerPopupViewModel.SetByAnnouncement(announcement);
            _popupWindows.Show<OfferPopupViewModel>();
        }

        private async void ShowStandardUpsellModal()
        {
            int randomNumber = _random.Next(0, 100);
            if (randomNumber >= 15)
            {
                return;
            }

            await _modals.ShowAsync<UpsellModalViewModel>();
        }

        private async void ShowWelcomeModal()
        {
            await _modals.ShowAsync<WelcomeModalViewModel>();
            _appSettings.WelcomeModalShown = true;
            if (_appSettings.FeatureFreeRescopeEnabled && !_appSettings.IsFreeRescopeModalDisplayed)
            {
                _appSettings.IsFreeRescopeModalDisplayed = true;
            }
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

        public void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IAppSettings.FeatureFreeRescopeEnabled) && IsToShowFreeRescopePopup)
            {
                ShowFreeRescopeModal();
            }
        }
    }
}