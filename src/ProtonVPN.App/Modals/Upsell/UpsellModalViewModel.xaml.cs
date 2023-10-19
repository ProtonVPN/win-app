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

using ProtonVPN.Account;
using ProtonVPN.Config.Url;
using ProtonVPN.Core.Servers;
using ProtonVPN.Modals.Dialogs;
using ProtonVPN.StatisticalEvents.Contracts;
using ProtonVPN.Translations;

namespace ProtonVPN.Modals.Upsell
{
    public class UpsellModalViewModel : QuestionModalViewModel
    {
        private const int SERVERS_COUNT_ROUNDED_DOWN = 3000;

        private readonly ISubscriptionManager _subscriptionManager;
        private readonly IUpsellUpgradeAttemptStatisticalEventSender _upsellUpgradeAttemptStatisticalEventSender;
        private readonly IUpsellDisplayStatisticalEventSender _upsellDisplayStatisticalEventSender;

        protected readonly ServerManager ServerManager;
        protected readonly IActiveUrls Urls;

        protected virtual ModalSources ModalSource { get; } = ModalSources.Countries;

        public UpsellModalViewModel(ISubscriptionManager subscriptionManager,
            ServerManager serverManager,
            IActiveUrls urls,
            IUpsellUpgradeAttemptStatisticalEventSender upsellUpgradeAttemptStatisticalEventSender,
            IUpsellDisplayStatisticalEventSender upsellDisplayStatisticalEventSender)
        {
            _subscriptionManager = subscriptionManager;
            ServerManager = serverManager;
            Urls = urls;
            _upsellUpgradeAttemptStatisticalEventSender = upsellUpgradeAttemptStatisticalEventSender;
            _upsellDisplayStatisticalEventSender = upsellDisplayStatisticalEventSender;
        }

        protected override void ContinueAction()
        {
            _upsellUpgradeAttemptStatisticalEventSender.Send(ModalSource);
            _subscriptionManager.UpgradeAccountAsync();
            TryClose(true);
        }

        public string UpsellCountriesTitle => GetUpsellCountriesTitleTranslation();

        private string GetUpsellCountriesTitleTranslation()
        {
            int totalCountries = ServerManager.GetCountries().Count; 
            string nSecureServers = string.Format(Translation.GetPlural("Secure_Servers_lbl", SERVERS_COUNT_ROUNDED_DOWN), SERVERS_COUNT_ROUNDED_DOWN);
            string nCountries = string.Format(Translation.GetPlural("Countries_lbl", totalCountries), totalCountries);
            return Translation.Format("Upsell_Countries_Title", nSecureServers, nCountries);
        }

        public override void BeforeOpenModal(dynamic options)
        {
            _upsellDisplayStatisticalEventSender.Send(ModalSource);
        }
    }
}