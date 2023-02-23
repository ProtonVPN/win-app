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

using System.Configuration;
using System.Diagnostics;
using ProtonVPN.Core;
using ProtonVPN.Core.Storage;
using File = System.IO.File;

namespace ProtonVPN.Settings
{
    internal class SelfRepairingSettings : ISettingsStorage
    {
        private readonly AppSettingsStorage _settings;
        private readonly IAppExitInvoker _appExitInvoker;

        private bool _loaded;

        public SelfRepairingSettings(AppSettingsStorage settings, IAppExitInvoker appExitInvoker)
        {
            _settings = settings;
            _appExitInvoker = appExitInvoker;
        }

        public T Get<T>(string key)
        {
            Load();
            return _settings.Get<T>(key);
        }

        public void Set<T>(string key, T value)
        {
            Load();
            _settings.Set(key, value);
        }

        private void Load()
        {
            if (_loaded) return;

            _loaded = true;

            try
            {
                _settings.Load();
            }
            catch (ConfigurationException ex)
            {
                if (!(ex.InnerException is ConfigurationErrorsException configError) ||
                    string.IsNullOrEmpty(configError.Filename))
                {
                    throw;
                }

                File.Delete(configError.Filename);
                SingleInstanceApplication.ReleaseSingleInstanceLock();
                Process.Start(System.Windows.Application.ResourceAssembly.Location);
                _appExitInvoker.Exit();
            }
        }
    }
}