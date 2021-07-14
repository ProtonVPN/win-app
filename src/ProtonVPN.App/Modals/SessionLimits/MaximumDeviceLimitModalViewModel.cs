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
using ProtonVPN.Modals.Dialogs;

namespace ProtonVPN.Modals.SessionLimits
{
    public class MaximumDeviceLimitModalViewModel : QuestionModalViewModel
    {
        private readonly IActiveUrls _urls;

        public bool HasMaxTierPlan { get; private set; } = true;
        public bool NotHasMaxTierPlan => !HasMaxTierPlan;

        public MaximumDeviceLimitModalViewModel(IActiveUrls urls)
        {
            _urls = urls;

            UpgradeCommand = new RelayCommand(UpgradeAction);
        }

        public void SetPlan(bool hasMaxTierPlan)
        {
            HasMaxTierPlan = hasMaxTierPlan;
        }

        public ICommand UpgradeCommand { get; set; }

        protected virtual void UpgradeAction()
        {
            _urls.AccountUrl.Open();
            TryClose();
        }
    }
}