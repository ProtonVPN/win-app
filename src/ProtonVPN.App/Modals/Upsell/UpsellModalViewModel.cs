﻿/*
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
using ProtonVPN.Modals.Dialogs;
using ProtonVPN.Translations;

namespace ProtonVPN.Modals.Upsell
{
    public class UpsellModalViewModel : QuestionModalViewModel
    {
        private const int SERVERS_COUNT_ROUNDED_DOWN = 2500;
        private const int COUNTRIES_COUNT = 67;

        private readonly ISubscriptionManager _subscriptionManager;
        protected readonly IActiveUrls Urls;

        public UpsellModalViewModel(ISubscriptionManager subscriptionManager, IActiveUrls urls)
        {
            _subscriptionManager = subscriptionManager;
            Urls = urls;
        }

        protected override void ContinueAction()
        {
            _subscriptionManager.UpgradeAccountAsync();
            TryClose(true);
        }

        public string UpsellCountriesTitle => GetUpsellCountriesTitleTranslation();

        private string GetUpsellCountriesTitleTranslation()
        {
            string nSecureServers = string.Format(Translation.GetPlural("Secure_Servers_lbl", SERVERS_COUNT_ROUNDED_DOWN), SERVERS_COUNT_ROUNDED_DOWN);
            string nCountries = string.Format(Translation.GetPlural("Countries_lbl", COUNTRIES_COUNT), COUNTRIES_COUNT);
            return Translation.Format("Upsell_Countries_Title", nSecureServers, nCountries);
        }
    }
}