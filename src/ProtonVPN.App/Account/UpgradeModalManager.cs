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
using System.Threading.Tasks;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Models;
using ProtonVPN.Core.Settings;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Modals.Welcome;
using ProtonVPN.StatisticalEvents.Contracts;

namespace ProtonVPN.Account
{
    public class UpgradeModalManager : IUpgradeModalManager
    {
        private readonly IModals _modals;
        private readonly IUserStorage _userStorage;
        private readonly IVpnInfoUpdater _vpnInfoUpdater;
        private readonly IUpsellSuccessStatisticalEventSender _upsellSuccessStatisticalEventSender;
        private readonly IIssueReporter _issueReporter;

        public UpgradeModalManager(IModals modals,
            IUserStorage userStorage,
            IVpnInfoUpdater vpnInfoUpdater,
            IUpsellSuccessStatisticalEventSender upsellSuccessStatisticalEventSender,
            IIssueReporter issueReporter)
        {
            _modals = modals;
            _userStorage = userStorage;
            _vpnInfoUpdater = vpnInfoUpdater;
            _upsellSuccessStatisticalEventSender = upsellSuccessStatisticalEventSender;
            _issueReporter = issueReporter;
        }

        public async Task CheckForVpnPlanUpgradeAsync(string modalSource, string notificationReference)
        {
            string oldVpnPlan = _userStorage.GetUser().VpnPlan;
            await _vpnInfoUpdater.Update();
            string newVpnPlan = _userStorage.GetUser().VpnPlan;
            if (oldVpnPlan != newVpnPlan && _userStorage.GetUser().Paid())
            {
                if (!string.IsNullOrWhiteSpace(modalSource))
                {
                    if (!Enum.TryParse(modalSource, true, out ModalSources modalSourceEnum))
                    {
                        _issueReporter.CaptureError($"Failed to parse the string '{modalSource}' into a ModalSources enum.");
                        modalSourceEnum = ModalSources.Undefined;
                    }

                    _upsellSuccessStatisticalEventSender.Send(
                        modalSourceEnum,
                        oldVpnPlan,
                        newVpnPlan,
                        notificationReference);
                }

                await ShowUpgradeModalAsync();
            }
        }

        private async Task ShowUpgradeModalAsync()
        {
            User user = _userStorage.GetUser();
            if (user.IsPlusPlan())
            {
                await _modals.ShowAsync<PlusWelcomeModalViewModel>();
            }
            else if (user.IsUnlimitedPlan())
            {
                await _modals.ShowAsync<UnlimitedWelcomeModalViewModel>();
            }
            else
            {
                await _modals.ShowAsync<FallbackWelcomeModalViewModel>();
            }
        }
    }
}