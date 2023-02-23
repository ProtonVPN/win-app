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
using System.ComponentModel;
using System.ServiceModel;
using System.Threading.Tasks;
using ProtonVPN.Common.Abstract;
using ProtonVPN.Common.KillSwitch;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppServiceLogs;
using ProtonVPN.Core.Settings;
using ProtonVPN.Service.Contract.Settings;

namespace ProtonVPN.Core.Service.Settings
{
    public class SettingsServiceClientManager : ISettingsServiceClientManager, ISettingsAware
    {
        private readonly SettingsServiceClient _client;
        private readonly ILogger _logger;
        private readonly SettingsContractProvider _settingsContractProvider;
        private readonly VpnSystemService _service;

        public SettingsServiceClientManager(
            SettingsServiceClient client,
            ILogger logger,
            VpnSystemService service,
            SettingsContractProvider settingsContractProvider)
        {
            _client = client;
            _logger = logger;
            _service = service;
            _settingsContractProvider = settingsContractProvider;
        }

        public async Task UpdateServiceSettings()
        {
            await UpdateServiceSettingsInternal(_settingsContractProvider.GetSettingsContract());
        }

        public async Task DisableKillSwitch()
        {
            SettingsContract settingsContract = _settingsContractProvider.GetSettingsContract();
            settingsContract.KillSwitchMode = KillSwitchMode.Off;
            await UpdateServiceSettingsInternal(settingsContract);
        }

        public async Task EnableHardKillSwitch()
        {
            SettingsContract settingsContract = _settingsContractProvider.GetSettingsContract();
            settingsContract.KillSwitchMode = KillSwitchMode.Hard;
            await UpdateServiceSettingsInternal(settingsContract);
        }

        private async Task UpdateServiceSettingsInternal(SettingsContract settingsContract)
        {
            await _service.InvokeServiceAction(async () =>
            {
                try
                {
                    await Task.Run(() => _client.Apply(settingsContract));
                    return Result.Ok();
                }
                catch (Exception ex) when (ex is CommunicationException or TimeoutException or TaskCanceledException)
                {
                    _logger.Error<AppServiceCommunicationFailedLog>("The request to update service settings failed.", ex);
                    return Result.Fail();
                }
            });
        }

        public async void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IAppSettings.KillSwitchMode) ||
                e.PropertyName == nameof(IAppSettings.VpnAcceleratorEnabled) ||
                e.PropertyName == nameof(IAppSettings.FeatureVpnAcceleratorEnabled) ||
                e.PropertyName == nameof(IAppSettings.ModerateNat) ||
                e.PropertyName == nameof(IAppSettings.AllowNonStandardPorts) ||
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