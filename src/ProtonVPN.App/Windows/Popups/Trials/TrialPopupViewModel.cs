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

using ProtonVPN.Config.Url;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.User;

namespace ProtonVPN.Windows.Popups.Trials
{
    public class TrialPopupViewModel : BaseUpgradePlanPopupViewModel, IUserDataAware
    {
        private bool _expired;
        private readonly IUserStorage _userStorage;

        public TrialPopupViewModel(IActiveUrls urls, IUserStorage userStorage, AppWindow appWindow)
            : base(urls, appWindow)
        {
            _userStorage = userStorage;
        }

        public bool Expired
        {
            get => _expired;
            set => Set(ref _expired, value);
        }

        public void OnUserDataChanged()
        {
            Expired = !_userStorage.User().IsTrial();
        }
    }
}
