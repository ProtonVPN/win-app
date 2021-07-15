/*
 * Copyright (c) 2021 Proton Technologies AG
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
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.OS.Services;

namespace ProtonVPN.Vpn.WireGuard
{
    internal class WireGuardService : IService
    {
        private readonly ILogger _logger;
        private readonly ProtonVPN.Common.Configuration.Config _config;
        private readonly IService _origin;

        public WireGuardService(ILogger logger, ProtonVPN.Common.Configuration.Config config, IService origin)
        {
            _logger = logger;
            _config = config;
            _origin = origin;
        }

        public string Name => _origin.Name;

        public bool Exists() => _origin.Exists();

        public void Create(string pathAndArgs, bool unrestricted)
        {
            _origin.Create(pathAndArgs, unrestricted);
        }

        public bool Running() => _origin.Running();

        public bool IsStopped() => _origin.IsStopped();

        public bool Enabled() => _origin.Enabled();

        public void Enable() => _origin.Enabled();

        public Task<Result> StartAsync(CancellationToken cancellationToken)
        {
            if (!_origin.Exists())
            {
                _logger.Info("[WireGuardService] Service is missing. Installing.");
                _origin.Create(_config.WireGuard.ServicePath + " " + _config.WireGuard.ConfigFilePath,
                    unrestricted: true);
            }

            return _origin.StartAsync(cancellationToken);
        }

        public Task<Result> StopAsync(CancellationToken cancellationToken)
        {
            if (_origin.Running())
            {
                return _origin.StopAsync(cancellationToken);
            }

            return Task.Run(Result.Ok);
        }
    }
}