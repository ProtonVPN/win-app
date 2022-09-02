/*
 * Copyright (c) 2022 Proton Technologies AG
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

using System.Collections.Generic;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Servers;
using ProtonVPN.Translations;

namespace ProtonVPN.Streaming
{
    public class StreamingInfoPopupViewModel : ViewModel
    {
        private readonly string _countryCode;

        public StreamingInfoPopupViewModel(
            string countryCode,
            IReadOnlyList<StreamingServiceViewModel> streamingServices)
        {
            _countryCode = countryCode;
            StreamingServices = streamingServices;
            ClosePopupCommand = new RelayCommand(() => ShowPopup = false);
        }

        public ICommand ClosePopupCommand { get; }

        private bool _showPopup;

        public bool ShowPopup
        {
            get => _showPopup;
            set => Set(ref _showPopup, value);
        }

        public string Title => Translation.Format("Sidebar_Streaming_Country", Countries.GetName(_countryCode));

        public IReadOnlyList<StreamingServiceViewModel> StreamingServices { get; }
    }
}