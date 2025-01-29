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
using ProtonVPN.Common.Helpers;
using ProtonVPN.Config.Url;
using ProtonVPN.Core.Settings;
using ProtonVPN.Modals.Dialogs;
using ProtonVPN.StatisticalEvents.Contracts;
using ProtonVPN.Translations;

namespace ProtonVPN.Modals.Upsell
{
    public class UpsellModalViewModel : QuestionModalViewModel
    {
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly IUpsellUpgradeAttemptStatisticalEventSender _upsellUpgradeAttemptStatisticalEventSender;
        private readonly IUpsellDisplayStatisticalEventSender _upsellDisplayStatisticalEventSender;

        protected readonly IAppSettings AppSettings;
        protected readonly IActiveUrls Urls;

        protected virtual ModalSources ModalSource { get; } = ModalSources.Countries;

        public UpsellModalViewModel(ISubscriptionManager subscriptionManager,
            IAppSettings appSettings,
            IActiveUrls urls,
            IUpsellUpgradeAttemptStatisticalEventSender upsellUpgradeAttemptStatisticalEventSender,
            IUpsellDisplayStatisticalEventSender upsellDisplayStatisticalEventSender)
        {
            _subscriptionManager = subscriptionManager;
            AppSettings = appSettings;
            Urls = urls;
            _upsellUpgradeAttemptStatisticalEventSender = upsellUpgradeAttemptStatisticalEventSender;
            _upsellDisplayStatisticalEventSender = upsellDisplayStatisticalEventSender;
        }

        protected override void ContinueAction()
        {
            SendUpgradeAttemptStatisticalEvent();
            _subscriptionManager.UpgradeAccountAsync(ModalSource);
            TryClose(true);
        }

        public string UpsellCountriesTitle => GetUpsellCountriesTitleTranslation();

        private string GetUpsellCountriesTitleTranslation()
        {
            int totalCountries = AppSettings.CountryCount;
            int totalServers = RoundDownCalculator.RoundDown(AppSettings.ServerCount);
            string nSecureServers = string.Format(Translation.GetPlural("Secure_Servers_lbl", totalServers), totalServers);
            string nCountries = string.Format(Translation.GetPlural("Countries_lbl", totalCountries), totalCountries);
            return Translation.Format("Upsell_Countries_Title", nSecureServers, nCountries);
        }

        public override void BeforeOpenModal(dynamic options)
        {
            SendDisplayStatisticalEvent();
        }

        protected void SendDisplayStatisticalEvent()
        {
            _upsellDisplayStatisticalEventSender.Send(ModalSource);
        }
        protected void SendUpgradeAttemptStatisticalEvent()
        {
            _upsellUpgradeAttemptStatisticalEventSender.Send(ModalSource);
        }
    }
}