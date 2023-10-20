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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;
using ProtonVPN.StatisticalEvents.Contracts;

namespace ProtonVPN.StatisticalEvents.DimensionBuilders
{
    public class UpsellDimensionBuilder : IUpsellDimensionBuilder, IVpnStateAware
    {
        private bool _isConnected;
        private readonly IUserStorage _userStorage;
        private readonly IAppSettings _appSettings;

        public UpsellDimensionBuilder(IUserStorage userStorage, IAppSettings appSettings)
        {
            _userStorage = userStorage;
            _appSettings = appSettings;
        }

        public Dictionary<string, string> Build(ModalSources modalSource, string? reference = null)
        {
            string country = _userStorage.GetLocation().Country;
            return new()
            {
                { "modal_source", ModalSourceTranslator.Translate(modalSource) },
                { "user_plan", _userStorage.GetUser().OriginalVpnPlan },
                { "vpn_status", _isConnected ? "on" : "off" },
                { "user_country", string.IsNullOrWhiteSpace(country) ? "n/a" : country },
                { "new_free_plan_ui", _appSettings.FeatureFreeRescopeEnabled ? "yes" : "no" },
                { "days_since_account_creation", GetDaysSinceAccountCreation() },
                { "reference", string.IsNullOrWhiteSpace(reference) ? "n/a" : reference }
            };
        }

        private string GetDaysSinceAccountCreation()
        {
            DateTime? creationDateUtc = _userStorage.GetCreationDateUtc();
            DateTime nowUtc = DateTime.UtcNow;
            if (creationDateUtc is null)
            {
                return "n/a";
            }
            if (nowUtc <= creationDateUtc.Value)
            {
                return "0";
            }

            long daysSinceAccountCreation = (long)(nowUtc - creationDateUtc.Value).TotalDays;
            return daysSinceAccountCreation <= 0
                ? "0"
                : daysSinceAccountCreation <= 3
                ? "1-3"
                : daysSinceAccountCreation <= 7
                ? "4-7"
                : daysSinceAccountCreation <= 14
                ? "8-14"
                : ">14";
        }

        public async Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _isConnected = e.State.Status == Common.Vpn.VpnStatus.Connected;
        }
    }
}