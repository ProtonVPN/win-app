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

using System.Threading.Tasks;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Models;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Users;
using ProtonVPN.Modals.Welcome;

namespace ProtonVPN.Account
{
    public class UpgradeModalManager : IVpnPlanAware, IUpgradeModalManager
    {
        private readonly IModals _modals;
        private readonly IUserStorage _userStorage;
        private readonly IVpnInfoUpdater _vpnInfoUpdater;

        private bool _isToUpdate;

        public UpgradeModalManager(IModals modals, IUserStorage userStorage, IVpnInfoUpdater vpnInfoUpdater)
        {
            _modals = modals;
            _userStorage = userStorage;
            _vpnInfoUpdater = vpnInfoUpdater;
        }

        public void CheckForVpnPlanUpgrade()
        {
            _isToUpdate = true;
            _vpnInfoUpdater.Update();
        }

        public async Task OnVpnPlanChangedAsync(VpnPlanChangedEventArgs e)
        {
            if (!_isToUpdate)
            {
                return;
            }

            _isToUpdate = false;
            if (e.IsDowngrade)
            {
                return;
            }

            await ShowUpgradeModalAsync();
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