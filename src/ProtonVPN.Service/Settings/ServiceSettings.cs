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
using ProtonVPN.Common.Helpers;
using ProtonVPN.Common.KillSwitch;
using ProtonVPN.Common.Networking;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;

namespace ProtonVPN.Service.Settings
{
    public class ServiceSettings : IServiceSettings
    {
        private readonly ISettingsFileStorage _storage;

        private MainSettingsIpcEntity _settings;

        public event EventHandler<MainSettingsIpcEntity> SettingsChanged;

        public ServiceSettings(ISettingsFileStorage storage)
        {
            _storage = storage;
        }

        public KillSwitchMode KillSwitchMode
        {
            get
            {
                Load();
                return (KillSwitchMode)_settings.KillSwitchMode;
            }
        }

        public SplitTunnelSettingsIpcEntity SplitTunnelSettings
        {
            get
            {
                Load();
                return _settings.SplitTunnel ??= new SplitTunnelSettingsIpcEntity();
            }
        }

        public bool Ipv6LeakProtection
        {
            get
            {
                Load();
                return _settings.Ipv6LeakProtection;
            }
        }

        public VpnProtocol VpnProtocol
        {
            get
            {
                Load();
                return (VpnProtocol)_settings.VpnProtocol;
            }
        }

        public OpenVpnAdapter OpenVpnAdapter
        {
            get
            {
                Load();
                return (OpenVpnAdapter)_settings.OpenVpnAdapter;
            }
        }

        public void Apply(MainSettingsIpcEntity settings)
        {
            Ensure.NotNull(settings, nameof(settings));

            _settings = settings;
            Save();

            SettingsChanged?.Invoke(this, settings);
        }

        private void Load()
        {
            if (_settings == null)
            {
                _settings = _storage.Get() ?? new MainSettingsIpcEntity();
            }
        }

        private void Save()
        {
            _storage.Set(_settings);
        }
    }
}