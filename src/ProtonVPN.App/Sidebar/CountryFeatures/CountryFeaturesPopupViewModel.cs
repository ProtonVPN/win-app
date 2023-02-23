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

using Caliburn.Micro;
using ProtonVPN.Config.Url;

namespace ProtonVPN.Sidebar.CountryFeatures
{
    public class CountryFeaturesPopupViewModel : Screen
    {
        private readonly IActiveUrls _urls;

        public CountryFeaturesPopupViewModel(IActiveUrls urls)
        {
            _urls = urls;
        }

        private bool _isPopupOpen;

        public bool IsPopupOpen
        {
            get => _isPopupOpen;
            set => Set(ref _isPopupOpen, value);
        }

        public void OpenP2PUrl()
        {
            _urls.P2PUrl.Open();
        }

        public void OpenStreamingUrl()
        {
            _urls.StreamingUrl.Open();
        }

        public void OpenSmartRoutingUrl()
        {
            _urls.SmartRoutingUrl.Open();
        }

        public void OpenTorUrl()
        {
            _urls.TorUrl.Open();
        }

        public void ClosePopup()
        {
            IsPopupOpen = false;
        }
    }
}