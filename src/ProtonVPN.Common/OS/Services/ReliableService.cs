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

using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Abstract;

namespace ProtonVPN.Common.OS.Services
{
    public class ReliableService : IService
    {
        private readonly IServiceRetryPolicy _retryPolicy;
        private readonly IService _origin;

        public ReliableService(IServiceRetryPolicy retryPolicy, IService origin)
        {
            _retryPolicy = retryPolicy;
            _origin = origin;
        }

        public string Name => _origin.Name;

        public bool Exists() => _origin.Exists();

        public void Create(string pathAndArgs, bool unrestricted) => _origin.Create(pathAndArgs, unrestricted);

        public bool Running() => _origin.Running();

        public bool IsStopped() => _origin.IsStopped();

        public bool Enabled() => _origin.Enabled();

        public void Enable() => _origin.Enable();

        public Task<Result> StartAsync(CancellationToken cancellationToken)
        {
            return _retryPolicy.ExecuteAsync(ct => _origin.StartAsync(ct), cancellationToken);
        }

        public Task<Result> StopAsync(CancellationToken cancellationToken)
        {
            return _origin.StopAsync(cancellationToken);
        }
    }
}
