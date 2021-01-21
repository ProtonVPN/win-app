/*
 * Copyright (c) 2020 Proton Technologies AG
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

using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.Config.Url;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.User;

namespace ProtonVPN.Modals.Trial
{
    public class TrialModalViewModel : BaseModalViewModel, IUserDataAware
    {
        private readonly IActiveUrls _urls;
        private readonly IUserStorage _userStorage;
        private bool _expired;

        public TrialModalViewModel(IActiveUrls urls, IUserStorage userStorage)
        {
            _userStorage = userStorage;
            _urls = urls;
            UpgradeCommand = new RelayCommand(UpgradeAction);
        }

        public ICommand UpgradeCommand { get; set; }

        public bool Expired
        {
            get => _expired;
            set => Set(ref _expired, value);
        }

        public void UpgradeAction()
        {
            _urls.AccountUrl.Open();
            TryClose(true);
        }

        public void OnUserDataChanged()
        {
            Expired = !_userStorage.User().IsTrial();
        }
    }
}