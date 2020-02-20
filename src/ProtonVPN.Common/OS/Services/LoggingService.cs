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
using System.Threading;
using System.Threading.Tasks;
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

        public bool Running() => _origin.Running();

        public Task<Result> StartAsync(CancellationToken cancellationToken)
        {
            return Logged(
                "Starting", 
                () => _origin.StartAsync(cancellationToken));
        }

        public Task<Result> StopAsync(CancellationToken cancellationToken)
        {
            return Logged(
                "Stopping", 
                () => _origin.StopAsync(cancellationToken));
        }

        private async Task<Result> Logged(string actionName, Func<Task<Result>> action)
        {
            try
            {
                _logger.Info($"{ActionMessage()}");
                var result = await action();
                _logger.Info($"{ActionMessage()} {(result.Success ? "succeeded" : "failed")}");
                
                return result;
            }
            catch (InvalidOperationException e) when (e.IsServiceAlreadyRunning())
            {
                Info(": Already running");
                throw;
            }
            catch (InvalidOperationException e) when (e.IsServiceNotRunning())
            {
                Info(": Not running");
                throw;
            }
            catch (OperationCanceledException)
            {
                Info("cancelled");
                throw;
            }
            catch (TimeoutException)
            {
                Info("timed out");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"{ActionMessage()} failed: {ex.CombinedMessage()}");
                throw;
            }

            string ActionMessage()
            {
                return $"{actionName} the service \"{Name}\"";
            }

            void Info(string resultMessage)
            {
                _logger.Info($"{ActionMessage()} {resultMessage}");
            }
        }
    }
}
