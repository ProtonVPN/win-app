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

using System.Threading.Tasks;
using ProtonVPN.Core.Models;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.User;
using ProtonVPN.Settings;

namespace ProtonVPN.PlanDowngrading
{
    public class PlanDowngradeHandler : IVpnPlanAware
    {
        private readonly IUserStorage _userStorage;
        private readonly IVpnReconnector _vpnReconnector;
        private readonly IAppSettings _appSettings;

        public PlanDowngradeHandler(
            IUserStorage userStorage,
            IVpnReconnector vpnReconnector, 
            IAppSettings appSettings)
        {
            _userStorage = userStorage;
            _vpnReconnector = vpnReconnector;
            _appSettings = appSettings;
        }

        public async Task OnVpnPlanChangedAsync(VpnPlanChangedEventArgs e)
        {
            if (e.IsDowngrade)
            {
                User user = _userStorage.User();
                if (!user.IsDelinquent())
                {
                    await DowngradeUserAsync(e, user);
                }
            }
        }

        private async Task DowngradeUserAsync(VpnPlanChangedEventArgs e, User user)
        {
            if (user.MaxTier < ServerTiers.Plus)
            {
                _appSettings.SecureCore = false;
                _appSettings.PortForwardingEnabled = false;
            }

            if (user.MaxTier < ServerTiers.Basic)
            {
                _appSettings.NetShieldEnabled = false;
            }

            await _vpnReconnector.ReconnectAsync();
        }
    }
}