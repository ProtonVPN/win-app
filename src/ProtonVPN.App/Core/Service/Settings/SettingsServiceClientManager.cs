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

using ProtonVPN.Common.Logging;
using ProtonVPN.Core.Settings;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using ProtonVPN.Common.Extensions;

namespace ProtonVPN.Core.Service.Settings
{
    internal class SettingsServiceClientManager
    {
        private readonly SettingsServiceClient _client;
        private readonly ILogger _logger;
        private readonly SettingsContractProvider _settingsContractProvider;

        public SettingsServiceClientManager(
            SettingsServiceClient client,
            IAppSettings appSettings,
            ILogger logger,
            SettingsContractProvider settingsContractProvider)
        {
            _settingsContractProvider = settingsContractProvider;
            _client = client;
            _logger = logger;

            appSettings.PropertyChanged += async (s, e) =>
            {
                if (e.PropertyName == nameof(IAppSettings.KillSwitch) ||
                    e.PropertyName == nameof(IAppSettings.Ipv6LeakProtection))
                {
                    _logger.Info($"Setting \"{e.PropertyName}\" changed");
                    await UpdateServiceSettings();
                }
            };
        }

        public async Task UpdateServiceSettings()
        {
            try
            {
                await Task.Run(() => _client.Apply(_settingsContractProvider.GetSettingsContract()));
            }
            catch (Exception ex) when (ex is CommunicationException || ex is TimeoutException || ex is TaskCanceledException)
            {
                _logger.Error(ex.CombinedMessage());
            }
        }
    }
}
