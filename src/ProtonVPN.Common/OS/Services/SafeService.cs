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
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Abstract;

namespace ProtonVPN.Common.OS.Services
{
    public class SafeService : IService
    {
        private readonly IService _origin;

        public SafeService(IService origin)
        {
            _origin = origin;
        }

        public string Name => _origin.Name;

        public bool Running()
        {
            try
            {
                return _origin.Running();
            }
            catch (Win32Exception)
            {
                return false;
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
