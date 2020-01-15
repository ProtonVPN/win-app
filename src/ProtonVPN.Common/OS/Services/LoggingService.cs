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
using ProtonVPN.Common.Abstract;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;

namespace ProtonVPN.Common.OS.Services
{
    public class LoggingService : IService
    {
        private readonly ILogger _logger;
        private readonly IService _origin;

        public LoggingService(ILogger logger, IService origin)
        {
            _logger = logger;
            _origin = origin;
        }

        public string Name => _origin.Name;

        public event EventHandler<string> ServiceStartedHandler;

        public bool IsRunning() => _origin.IsRunning();

        public Result Start()
        {
            return Logged(() =>
            {
                _logger.Info($"Starting the service \"{Name}\"");
                return _origin.Start();
            });
        }

        public Result Stop()
        {
            return Logged(() =>
            {
                _logger.Info($"Stopping the service \"{Name}\"");
                return _origin.Stop();
            });
        }

        private Result Logged(Func<Result> action)
        {
            try
            {
                return action();
            }
            catch (InvalidOperationException e) when (e.IsServiceAlreadyRunning())
            {
                _logger.Info($"The service \"{Name}\" is already running");
                throw;
            }
            catch (InvalidOperationException e) when (e.IsServiceNotRunning())
            {
                _logger.Info($"The service \"{Name}\" is not running");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to start the service \"{Name}\": {ex.CombinedMessage()}");
                throw;
            }
        }
    }
}
