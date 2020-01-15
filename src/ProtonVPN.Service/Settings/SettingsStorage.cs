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
using System.IO;
using Newtonsoft.Json;
using ProtonVPN.Common.Logging;
using ProtonVPN.Service.Contract.Settings;

namespace ProtonVPN.Service.Settings
{
    public class SettingsStorage
    {
        private readonly ILogger _logger;
        private readonly  Common.Configuration.Config _config;
        private readonly JsonSerializer _serializer = new JsonSerializer();

        public SettingsStorage(ILogger logger,  Common.Configuration.Config config)
        {
            _logger = logger;
            _config = config;
        }

        public SettingsContract Load()
        {
            var filename = _config.ServiceSettingsFilePath;
            SettingsContract result = null;

            HandleException(() =>
                {
                    using var stream = new StreamReader(filename);
                    using var reader = new JsonTextReader(stream);

                    result = _serializer.Deserialize<SettingsContract>(reader);
                },
                "Loading");

            return result;
        }

        public void Save(SettingsContract settings)
        {
            var filename = _config.ServiceSettingsFilePath;

            HandleException(() =>
                {
                    using var stream = new StreamWriter(filename);
                    using var writer = new JsonTextWriter(stream);
                    _serializer.Serialize(writer, settings);
                },
                "Saving");
        }

        private void HandleException(Action action, string actionName)
        {
            try
            {
                action();
            }
            catch (Exception ex) when (IsFileAccessException(ex) || IsSerializationException(ex))
            {
                LogException(actionName, ex);
            }
        }

        private void LogException(string action, Exception ex)
        {
            _logger.Error($"{action} settings has failed: {ex.Message}");
        }

        private static bool IsSerializationException(Exception ex)
        {
            return ex is JsonException;
        }

        private static bool IsFileAccessException(Exception ex)
        {
            return ex is IOException ||
                   ex is UnauthorizedAccessException;
        }
    }
}
