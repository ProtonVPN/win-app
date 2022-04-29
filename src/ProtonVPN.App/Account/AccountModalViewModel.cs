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

using System.Windows.Input;
using Caliburn.Micro;
using GalaSoft.MvvmLight.Command;
using ProtonVPN.Config.Url;
using ProtonVPN.Core.Models;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.User;
using ProtonVPN.Modals;
using ProtonVPN.Translations;

namespace ProtonVPN.Account
{
    public class AccountModalViewModel : BaseModalViewModel, IUserDataAware, IHandle<AccountActionMessage>
    {
        private readonly IAppSettings _appSettings;
        private readonly IActiveUrls _urls;
        private readonly IUserStorage _userStorage;

        private string _username;
        private string _planName;
        private string _accountType;
        private string _planColor;
        private string _actionMessage = string.Empty;

        public string ActionMessage
        {
            get => _actionMessage;
            set => Set(ref _actionMessage, value);
        }

        public PromoCodeViewModel PromoCodeViewModel { get; }

        public bool IsToShowUseCoupon => _appSettings.FeaturePromoCodeEnabled && _userStorage.User().CanUsePromoCode();

        public AccountModalViewModel(
            IAppSettings appSettings,
            IEventAggregator eventAggregator,
            IActiveUrls urls,
            IUserStorage userStorage,
            PromoCodeViewModel promoCodeViewModel)
        {
            eventAggregator.Subscribe(this);
            _appSettings = appSettings;
            _urls = urls;
            _userStorage = userStorage;
            PromoCodeViewModel = promoCodeViewModel;

            ManageAccountCommand = new RelayCommand(ManageAccountAction);
            ProtonMailPricingCommand = new RelayCommand(ShowProtonMailPricing);
            CloseActionMessageCommand = new RelayCommand(CloseActionMessageAction);
        }

        public ICommand ManageAccountCommand { get; set; }
        public ICommand ProtonMailPricingCommand { get; set; }
        public ICommand CloseActionMessageCommand { get; set; }

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

        public string BasicPlanTotalCountries => string.Format(Translation.GetPlural(TotalCountries, 40), "40+");

        public string AccountType
        {
            get => _accountType;
            set => Set(ref _accountType, value);
        }

        protected override void OnActivate()
        {
            SetUserDetails();
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            CloseActionMessageAction();
        }

        public void OnUserDataChanged()
        {
            SetUserDetails();
            NotifyOfPropertyChange(nameof(IsToShowUseCoupon));
        }

        public void Handle(AccountActionMessage message)
        {
            ActionMessage = message.Message;
        }

        private void SetUserDetails()
        {
            User user = _userStorage.User();
            PlanName = VpnPlanHelper.GetPlanName(user.OriginalVpnPlan);
            Username = user.Username;
            AccountType = user.GetAccountPlan();
            PlanColor = VpnPlanHelper.GetPlanColor(user.OriginalVpnPlan);
        }

        private void ManageAccountAction()
        {
            _urls.AccountUrl.Open();
        }

        private void ShowProtonMailPricing()
        {
            _urls.ProtonMailPricingUrl.Open();
        }

        private void CloseActionMessageAction()
        {
            ActionMessage = string.Empty;
        }
    }
}