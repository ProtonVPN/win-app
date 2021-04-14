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

using System;
using System.Threading.Tasks;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;

namespace ProtonVPN.Core
{
    internal class AutoConnect : IVpnStateAware
    {
        private readonly IAppSettings _appSettings;
        private readonly IVpnManager _vpnManager;
        private readonly ILogger _logger;
        private readonly ProfileManager _profileManager;
        private VpnStatus _vpnStatus;

        public AutoConnect(
            IAppSettings appSettings,
            IVpnManager vpnManager,
            ILogger logger,
            ProfileManager profileManager)
        {
            _appSettings = appSettings;
            _vpnManager = vpnManager;
            _logger = logger;
            _profileManager = profileManager;
        }

        public async Task Load(bool autoLogin)
        {
            if (!AutoConnectRequired(autoLogin))
                return;
            try
            {
                var profile = await _profileManager.GetProfileById(_appSettings.AutoConnect);

                if (profile == null)
                {
                    _logger.Warn("Profile configured for auto connect is missing!");
                    return;
                }

                _logger.Info("Automatically connecting to selected profile");

                await _vpnManager.ConnectAsync(profile);
            }
            catch (OperationCanceledException ex)
            {
                _logger.Error(ex);
            }
        }

        private bool AutoConnectRequired(bool autoLogin)
        {
            return autoLogin && _vpnStatus.Equals(VpnStatus.Disconnected) && !string.IsNullOrEmpty(_appSettings.AutoConnect);
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _vpnStatus = e.State.Status;

            return Task.CompletedTask;
        }
    }
}
