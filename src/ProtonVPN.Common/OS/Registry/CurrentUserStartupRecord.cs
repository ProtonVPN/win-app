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
using Microsoft.Win32;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.OperatingSystemLogs;

namespace ProtonVPN.Common.OS.Registry
{
    public class CurrentUserStartupRecord : ICurrentUserStartupRecord
    {
        private const string RUN_KEY = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

        private readonly ILogger _logger;
        private readonly string _name;
        private readonly string _command;

        public CurrentUserStartupRecord(ILogger logger, IConfiguration appConfig)
        {
            _logger = logger;
            _name = appConfig.AppName;
            _command = appConfig.AppExePath;
        }

        public bool Exists()
        {
            return HandleExceptions(ExecuteExists, false, "read");
        }

        private TResult HandleExceptions<TResult>(Func<TResult> function, TResult defaultResult, string actionName)
        {
            try
            {
                return function();
            }
            catch (Exception ex) when (ex.IsRegistryAccessException())
            {
                _logger.Error<OperatingSystemRegistryAccessFailedLog>(
                    $"Can't {actionName} auto start record in Windows registry", ex);
            }

            return defaultResult;
        }

        private bool ExecuteExists()
        {
            return Execute(key => key?.GetValue(_name) != null);
        }

        private T Execute<T>(Func<RegistryKey, T> function)
        {
            using (RegistryKey registryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(RUN_KEY, false))
            {
                return function(registryKey);
            }
        }

        public bool Valid()
        {
            return HandleExceptions(ExecuteValid, false, "read");
        }

        private bool ExecuteValid()
        {
            return Execute(key => key?.GetValue(_name) is string command && _command == command);
        }

        public void Create()
        {
            HandleExceptions(ExecuteCreate, "create");
        }

        private void HandleExceptions(Action action, string actionName)
        {
            HandleExceptions<object>(() => 
            {
                action();
                return null;
            }, null, actionName);
        }

        private void ExecuteCreate()
        {
            Execute(key => key?.SetValue(_name, _command));
        }

        private void Execute(Action<RegistryKey> action)
        {
            using (RegistryKey registryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(RUN_KEY, true))
            {
                action(registryKey);
            }
            _logger.Info<OperatingSystemRegistryChangedLog>($"Written registry key '{_name}':'{_command}'.");
        }

        public void Remove()
        {
            HandleExceptions(ExecuteRemove, "delete");
        }

        private void ExecuteRemove()
        {
            Execute(key =>
            {
                if (key.GetValue(_name) != null)
                {
                    key.DeleteValue(_name);
                    _logger.Info<OperatingSystemRegistryChangedLog>($"Deleted registry key '{_name}'.");
                }
            });
        }
    }
}
