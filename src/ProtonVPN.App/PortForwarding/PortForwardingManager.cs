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
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Modals;
using ProtonVPN.Modals.Upsell;

namespace ProtonVPN.PortForwarding
{
    public class PortForwardingManager : IPortForwardingManager
    {
        private readonly IAppSettings _appSettings;
        private readonly IUserStorage _userStorage;
        private readonly IModals _modals;
        private readonly IVpnManager _vpnManager;

        public PortForwardingManager(
            IAppSettings appSettings,
            IUserStorage userStorage,
            IModals modals,
            IVpnManager vpnManager)
        {
            _appSettings = appSettings;
            _userStorage = userStorage;
            _modals = modals;
            _vpnManager = vpnManager;
        }

        public async Task EnableAsync()
        {
            if (_appSettings.PortForwardingEnabled)
            {
                return;
            }
            if (_userStorage.GetUser().Paid())
            {
                await EnablePortForwardingIfConfirmedAsync();
            }
            else
            {
                _modals.Show<PortForwardingUpsellModalViewModel>();
            }
        }

        private async Task EnablePortForwardingIfConfirmedAsync()
        {
            if (_appSettings.DoNotShowPortForwardingConfirmationDialog)
            {
                await EnablePortForwardingAsync();
            }
            else
            {
                bool? result = _modals.Show<PortForwardingConfirmationModalViewModel>();

                if (result.HasValue && result.Value)
                {
                    await EnablePortForwardingAsync();
                }
            }
        }

        private async Task EnablePortForwardingAsync()
        {
            bool secureCore = _appSettings.SecureCore;
            _appSettings.SecureCore = false;
            _appSettings.PortForwardingEnabled = true;
            _appSettings.ModerateNat = true;
            _appSettings.AllowNonStandardPorts = true;

            if (secureCore)
            {
                await _vpnManager.ReconnectAsync();
            }
        }

        public async Task DisableAsync()
        {
            if (_appSettings.PortForwardingEnabled)
            {
                _appSettings.PortForwardingEnabled = false;
            }
        }
    }
}