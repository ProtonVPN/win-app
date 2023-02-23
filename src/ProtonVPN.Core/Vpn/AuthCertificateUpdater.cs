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

using System.ComponentModel;
using System.Threading.Tasks;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Core.Vpn
{
    public class VpnAuthCertificateUpdater : ISettingsAware, IVpnStateAware
    {
        private readonly IAppSettings _appSettings;
        private readonly IVpnServiceManager _vpnServiceManager;

        private VpnStatus _vpnStatus;

        public VpnAuthCertificateUpdater(IAppSettings appSettings, IVpnServiceManager vpnServiceManager)
        {
            _appSettings = appSettings;
            _vpnServiceManager = vpnServiceManager;
        }

        public async void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IAppSettings.AuthenticationCertificatePem) &&
                _vpnStatus == VpnStatus.Connected &&
                !_appSettings.AuthenticationCertificatePem.IsNullOrEmpty())
            {
                await _vpnServiceManager.UpdateAuthCertificate(_appSettings.AuthenticationCertificatePem);
            }
        }

        public async Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _vpnStatus = e.State.Status;
        }
    }
}