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
using GalaSoft.MvvmLight.Command;
using ProtonVPN.Account;
using ProtonVPN.StatisticalEvents;
using ProtonVPN.StatisticalEvents.Contracts;

namespace ProtonVPN.Windows.Popups
{
    public abstract class BaseUpgradePlanPopupViewModel : BasePopupViewModel
    {
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly IUpsellUpgradeAttemptStatisticalEventSender _upsellUpgradeAttemptEventSender;

        protected BaseUpgradePlanPopupViewModel(ISubscriptionManager subscriptionManager, AppWindow appWindow,
            IUpsellUpgradeAttemptStatisticalEventSender upsellUpgradeAttemptStatisticalEventSender)
            : base(appWindow)
        {
            _subscriptionManager = subscriptionManager;
            _upsellUpgradeAttemptEventSender = upsellUpgradeAttemptStatisticalEventSender;

            UpgradeCommand = new RelayCommand(UpgradeAction);
        }

        public ICommand UpgradeCommand { get; set; }

        protected abstract ModalSources ModalSource { get; }

        protected virtual async void UpgradeAction()
        {
            _upsellUpgradeAttemptEventSender.Send(ModalSource);
            await _subscriptionManager.UpgradeAccountAsync(ModalSource);
            await TryCloseAsync();
        }
    }
}