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
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.OperatingSystemLogs;

namespace ProtonVPN.Common.OS.Registry
{
    public class SafeStartupRecord : IStartupRecord
    {
        private readonly ILogger _logger;
        private readonly IStartupRecord _origin;

        public SafeStartupRecord(ILogger logger, IStartupRecord origin)
        {
            _logger = logger;
            _origin = origin;
        }

        public bool Exists()
        {
            return HandleExceptions(() => _origin.Exists(), false, "read");
        }

        public bool Valid()
        {
            return HandleExceptions(() => _origin.Valid(), false, "read");
        }

        public void Create()
        {
            HandleExceptions(() => _origin.Create(), "create");
        }

        public void Remove()
        {
            HandleExceptions(() => _origin.Remove(), "delete");
        }

        private void HandleExceptions(Action action, string actionName)
        {
            HandleExceptions<object>(() => 
            {
                action();
                return null;
            }, null, actionName);
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
    }
}
