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

using System.Windows.Input;
using Caliburn.Micro;
using GalaSoft.MvvmLight.Command;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Config.Url;
using ProtonVPN.Core.Models;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Specs;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Users;
using ProtonVPN.Modals;
using ProtonVPN.Translations;

namespace ProtonVPN.Account
{
    public class AccountModalViewModel : BaseModalViewModel, IUserDataAware, IHandle<AccountActionMessage>
    {
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly IAppSettings _appSettings;
        private readonly ServerManager _serverManager;
        private readonly IActiveUrls _urls;
        private readonly IUserStorage _userStorage;

        private string _username;
        private string _planName;
        private string _accountType;
        private string _actionMessage = string.Empty;

        public string ActionMessage
        {
            get => _actionMessage;
            set => Set(ref _actionMessage, value);
        }

        public PromoCodeViewModel PromoCodeViewModel { get; }

        public bool IsToShowUseCoupon => _appSettings.FeaturePromoCodeEnabled && _userStorage.GetUser().CanUsePromoCode();

        public AccountModalViewModel(
            ISubscriptionManager subscriptionManager,
            IAppSettings appSettings,
            ServerManager serverManager,
            IEventAggregator eventAggregator,
            IActiveUrls urls,
            IUserStorage userStorage,
            PromoCodeViewModel promoCodeViewModel)
        {
            eventAggregator.Subscribe(this);
            _subscriptionManager = subscriptionManager;
            _appSettings = appSettings;
            _serverManager = serverManager;
            _urls = urls;
            _userStorage = userStorage;
            PromoCodeViewModel = promoCodeViewModel;

            ManageSubscriptionCommand = new RelayCommand(ManageSubscriptionAction);
            ProtonMailPricingCommand = new RelayCommand(ShowProtonMailPricing);
            CloseActionMessageCommand = new RelayCommand(CloseActionMessageAction);
        }

        public ICommand ManageSubscriptionCommand { get; set; }
        public ICommand ProtonMailPricingCommand { get; set; }
        public ICommand CloseActionMessageCommand { get; set; }

        public string TotalCountries => "Account_lbl_Countries";
        public string TotalServers => "Account_lbl_Servers";
        public string TotalConnections => "Account_lbl_Connection";

        public string FreePlanServers
        {
            get
            {
                int freeServerCount = _serverManager.GetServers(new FreeServer()).Count;
                return string.Format(Translation.GetPlural(TotalServers, freeServerCount), freeServerCount);
            }
        }

        public string FreePlanCountries
        {
            get
            {
                int totalFreePlanCountries = _serverManager.GetCountriesByTier(ServerTiers.Free).Count;
                return string.Format(Translation.GetPlural(TotalCountries, totalFreePlanCountries),
                    totalFreePlanCountries);
            }
        }

        public string PlusPlanServers
        {
            get
            {
                int plusServersCount = _serverManager.GetServers(new MaxTierServer(ServerTiers.Plus)).Count;
                return string.Format(Translation.GetPlural(TotalServers, plusServersCount), plusServersCount);
            }
        }

        public string PlusPlanCountries
        {
            get
            {
                int totalCountries = _serverManager.GetCountries().Count;
                return string.Format(Translation.GetPlural(TotalCountries, totalCountries), totalCountries);
            }
        }

        public bool IsFreePlan => !_userStorage.GetUser().Paid();
        public bool IsPlusPlan => _userStorage.GetUser().IsPlusPlan();

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
            NotifyOfPropertyChange(nameof(IsFreePlan));
            NotifyOfPropertyChange(nameof(IsPlusPlan));
        }

        public void Handle(AccountActionMessage message)
        {
            ActionMessage = message.Message;
        }

        private void SetUserDetails()
        {
            User user = _userStorage.GetUser();
            PlanName = user.VpnPlanName.IsNullOrEmpty() ? Translation.Get("Account_lbl_Free") : user.VpnPlanName;
            Username = user.Username;
            AccountType = user.GetAccountPlan();
        }

        private void ManageSubscriptionAction()
        {
            _subscriptionManager.ManageSubscriptionAsync();
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