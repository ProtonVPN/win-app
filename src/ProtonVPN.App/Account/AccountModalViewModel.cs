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

using GalaSoft.MvvmLight.Command;
using ProtonVPN.Config.Url;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.User;
using ProtonVPN.Modals;
using System.Collections.Generic;
using System.Windows.Input;

namespace ProtonVPN.Account
{
    public class AccountModalViewModel : BaseModalViewModel, IUserDataAware
    {
        private readonly IActiveUrls _urls;
        private readonly IUserStorage _userStorage;
        private readonly PricingBuilder _pricingBuilder;
        private readonly Common.Configuration.Config _appConfig;

        private string _username;
        private string _planName;
        private string _accountType;
        private string _planColor;
        private List<PricingModel> _pricing;

        public AccountModalViewModel(
            Common.Configuration.Config appConfig,
            IActiveUrls urls,
            IUserStorage userStorage,
            PricingBuilder pricingBuilder)
        {
            _appConfig = appConfig;
            _urls = urls;
            _userStorage = userStorage;
            _pricingBuilder = pricingBuilder;

            ManageAccountCommand = new RelayCommand(ManageAccountAction);
            ProtonMailPricingCommand = new RelayCommand(ShowProtonMailPricing);
        }

        public string PopupPlacement => "Bottom";
        public ICommand ManageAccountCommand { get; set; }
        public ICommand UpgradeCommand { get; set; }
        public ICommand ManagePaymentCommand { get; set; }
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

        private bool _pricingLoadFailed;
        public bool PricingLoadFailed
        {
            get => _pricingLoadFailed;
            set => Set(ref _pricingLoadFailed, value);
        }

        public string AccountType
        {
            get => _accountType;
            set => Set(ref _accountType, value);
        }

        public List<PricingModel> Pricing
        {
            get => _pricing;
            set => Set(ref _pricing, value);
        }

        protected override async void OnActivate()
        {
            if (Loaded)
                return;

            try
            {
                Loading = true;
                await _pricingBuilder.Load();
                Pricing = _pricingBuilder.BuildPricing();
                Loading = false;
                Loaded = true;
                PricingLoadFailed = false;
            }
            catch(PricingBuilderException)
            {
                PricingLoadFailed = true;
                Loading = false;
            }

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
