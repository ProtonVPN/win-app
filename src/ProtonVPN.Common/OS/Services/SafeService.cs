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
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Abstract;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppServiceLogs;

namespace ProtonVPN.Common.OS.Services
{
    public class SafeService : IService
    {
        private readonly IService _origin;
        private readonly ILogger _logger;

        public SafeService(IService origin, ILogger logger)
        {
            _origin = origin;
            _logger = logger;
        }

        public string Name => _origin.Name;

        public bool Exists()
        {
            try
            {
                return _origin.Exists();
            }
            catch (Win32Exception e)
            {
                _logger.Error<AppServiceLog>($"Failed to check whether the service {Name} exists.", e);
                return false;
            }
        }

        public void Create(string pathAndArgs, bool unrestricted)
        {
            try
            {
                _origin.Create(pathAndArgs, unrestricted);
            }
            catch (Win32Exception e)
            {
                _logger.Error<AppServiceLog>($"Failed to create {Name} service.", e);
            }
        }

        public void UpdatePathAndArgs(string cmd)
        {
            try
            {
                _origin.UpdatePathAndArgs(cmd);
            }
            catch (Win32Exception e)
            {
                _logger.Error<AppServiceLog>($"Failed to update path and args for {Name} service.", e);
            }
        }

        public bool Running()
        {
            try
            {
                return _origin.Running();
            }
            catch (Win32Exception e)
            {
                _logger.Error<AppServiceLog>($"Failed to check whether the service {Name} is running.", e);
                return false;
            }
        }

        public bool IsStopped()
        {
            try
            {
                return _origin.IsStopped();
            }
            catch (Win32Exception e)
            {
                _logger.Error<AppServiceLog>($"Failed to check whether the service {Name} is stopped.", e);
                return false;
            }
        }

        public bool Enabled()
        {
            try
            {
                return _origin.Enabled();
            }
            catch (Win32Exception e)
            {
                _logger.Error<AppServiceLog>($"Failed to check whether the service {Name} is enabled.", e);
                return false;
            }
        }

        public void Enable()
        {
            try
            {
                _origin.Enable();
            }
            catch (Exception e)
            {
                _logger.Error<AppServiceLog>($"Failed to enable {Name} service.", e);
            }
        }

        public Task<Result> StartAsync(CancellationToken cancellationToken)
        {
            return Safe(() => _origin.StartAsync(cancellationToken));
        }

        public Task<Result> StopAsync(CancellationToken cancellationToken)
        {
            return Safe(() => _origin.StopAsync(cancellationToken));
        }

        private async Task<Result> Safe(Func<Task<Result>> action)
        {
            try
            {
                return await action();
            }
            catch (InvalidOperationException ex) when (ex.IsServiceAlreadyRunning() || ex.IsServiceNotRunning())
            {
                return Result.Ok();
            }
            catch (Exception ex) when (ex.IsServiceAccessException() || ex is OperationCanceledException)
            {
                return Result.Fail(ex);
            }
        }
    }
}