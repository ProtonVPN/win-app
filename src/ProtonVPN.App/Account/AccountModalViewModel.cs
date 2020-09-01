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
using GalaSoft.MvvmLight.Command;
using ProtonVPN.Config.Url;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.User;
using ProtonVPN.Modals;
using ProtonVPN.Translations;

namespace ProtonVPN.Account
{
    public class AccountModalViewModel : BaseModalViewModel, IUserDataAware
    {
        private readonly IActiveUrls _urls;
        private readonly IUserStorage _userStorage;
        private readonly Common.Configuration.Config _appConfig;

        private string _username;
        private string _planName;
        private string _accountType;
        private string _planColor;

        public AccountModalViewModel(
            Common.Configuration.Config appConfig,
            IActiveUrls urls,
            IUserStorage userStorage)
        {
            _appConfig = appConfig;
            _urls = urls;
            _userStorage = userStorage;

            ManageAccountCommand = new RelayCommand(ManageAccountAction);
            ProtonMailPricingCommand = new RelayCommand(ShowProtonMailPricing);
        }

        public ICommand ManageAccountCommand { get; set; }
        public ICommand ProtonMailPricingCommand { get; set; }

        public string AppVersion => _appConfig.AppVersion;

        public string Username
        {
            get => _username;
            set => Set(ref _username, value);
        }

        public string PlanName
        {
            get => _planName;
            set => Set(ref _planName, value);
        }

        public string PlanColor
        {
            get => _planColor;
            set => Set(ref _planColor, value);
        }

        public string TotalCountries => "Account_lbl_Countries";

        public string TotalConnections => "Account_lbl_Connection";

        public string AccountType
        {
            get => _accountType;
            set => Set(ref _accountType, value);
        }

        protected override void OnActivate()
        {
            SetUserDetails();
        }

        public void OnUserDataChanged()
        {
            SetUserDetails();
        }

        private void SetUserDetails()
        {
            var user = _userStorage.User();
            PlanName = VpnPlanHelper.GetPlanName(user.VpnPlan);
            Username = user.Username;
            AccountType = user.GetAccountPlan();
            PlanColor = VpnPlanHelper.GetPlanColor(user.VpnPlan);
        }

        private void ManageAccountAction()
        {
            _urls.AccountUrl.Open();
        }

        private void ShowProtonMailPricing()
        {
            _urls.ProtonMailPricingUrl.Open();
        }
    }
}
