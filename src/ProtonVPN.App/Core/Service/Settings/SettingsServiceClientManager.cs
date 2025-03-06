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
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppServiceLogs;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Core.Service.Settings
{
    public class SettingsServiceClientManager : ISettingsServiceClientManager, ISettingsAware
    {
        private readonly ILogger _logger;
        private readonly IVpnServiceCaller _vpnServiceCaller;
        private readonly MainSettingsProvider _settingsContractProvider;

        public SettingsServiceClientManager(
            ILogger logger,
            IVpnServiceCaller vpnServiceCaller,
            MainSettingsProvider settingsContractProvider)
        {
            _logger = logger;
            _vpnServiceCaller = vpnServiceCaller;
            _settingsContractProvider = settingsContractProvider;
        }

        public async Task UpdateServiceSettings()
        {
            await UpdateServiceSettingsInternal(_settingsContractProvider.Create());
        }

        private async Task UpdateServiceSettingsInternal(MainSettingsIpcEntity settings)
        {
            await _vpnServiceCaller.ApplySettings(settings);
        }

        public async Task DisableKillSwitch()
        {
            MainSettingsIpcEntity settings = _settingsContractProvider.Create();
            settings.KillSwitchMode = KillSwitchModeIpcEntity.Off;
            await UpdateServiceSettingsInternal(settings);
        }

        public async Task EnableHardKillSwitch()
        {
            MainSettingsIpcEntity settings = _settingsContractProvider.Create();
            settings.KillSwitchMode = KillSwitchModeIpcEntity.Hard;
            await UpdateServiceSettingsInternal(settings);
        }

        public async void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IAppSettings.KillSwitchMode) ||
                e.PropertyName == nameof(IAppSettings.VpnAcceleratorEnabled) ||
                e.PropertyName == nameof(IAppSettings.FeatureVpnAcceleratorEnabled) ||
                e.PropertyName == nameof(IAppSettings.ModerateNat) ||
                e.PropertyName == nameof(IAppSettings.OvpnProtocol) ||
                e.PropertyName == nameof(IAppSettings.NetworkAdapterType) ||
                e.PropertyName == nameof(IAppSettings.NetShieldMode) ||
                e.PropertyName == nameof(IAppSettings.NetShieldEnabled) ||
                e.PropertyName == nameof(IAppSettings.FeatureNetShieldEnabled) ||
                e.PropertyName == nameof(IAppSettings.Ipv6LeakProtection) ||
                e.PropertyName == nameof(IAppSettings.PortForwardingEnabled) ||
                e.PropertyName == nameof(IAppSettings.FeaturePortForwardingEnabled))
            {
                _logger.Info<AppServiceLog>($"Setting \"{e.PropertyName}\" changed, updating service settings.");
                await UpdateServiceSettings();
            }
        }
    }
}