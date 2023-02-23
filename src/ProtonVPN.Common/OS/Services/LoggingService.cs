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
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Abstract;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization;
using ProtonVPN.Common.Logging.Categorization.Events.AppServiceLogs;

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

        public bool Exists() => _origin.Exists();

        public void Create(string pathAndArgs, bool unrestricted) => _origin.Create(pathAndArgs, unrestricted);

        public bool Running() => _origin.Running();

        public bool IsStopped() => _origin.IsStopped();

        public bool Enabled() => _origin.Enabled();

        public void Enable()
        {
            Logged<AppServiceLog, AppServiceLog>("Enabling", () =>
            {
                _origin.Enable();
                return Task.FromResult(Result.Ok());
            }).Wait();
        }

        public Task<Result> StartAsync(CancellationToken cancellationToken)
        {
            return Logged<AppServiceStartLog, AppServiceStartFailedLog>("Starting", 
                () => _origin.StartAsync(cancellationToken));
        }

        public Task<Result> StopAsync(CancellationToken cancellationToken)
        {
            return Logged<AppServiceStopLog, AppServiceStopFailedLog>("Stopping", 
                () => _origin.StopAsync(cancellationToken));
        }

        private async Task<Result> Logged<TEventLog, TEventFailedLog>(string actionName, Func<Task<Result>> action) 
            where TEventLog : ILogEvent, new()
            where TEventFailedLog : ILogEvent, new()
        {
            try
            {
                _logger.Info<TEventLog>($"{ActionMessage()}");
                Result result = await action();
                if (result.Success)
                {
                    _logger.Info<TEventLog>($"{ActionMessage()} succeeded");
                }
                else
                {
                    _logger.Warn<TEventFailedLog>($"{ActionMessage()} failed");
                }
                
                return result;
            }
            catch (InvalidOperationException e) when (e.IsServiceAlreadyRunning())
            {
                Warn(": Already running");
                throw;
            }
            catch (InvalidOperationException e) when (e.IsServiceNotRunning())
            {
                Warn(": Not running");
                throw;
            }
            catch (OperationCanceledException)
            {
                Warn("cancelled");
                throw;
            }
            catch (TimeoutException)
            {
                Warn("timed out");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error<TEventFailedLog>($"{ActionMessage()} failed: {ex.CombinedMessage()}");
                throw;
            }

            string ActionMessage()
            {
                return $"{actionName} the service \"{Name}\"";
            }

            void Warn(string resultMessage)
            {
                _logger.Warn<TEventFailedLog>($"{ActionMessage()} {resultMessage}");
            }
        }
    }
}
