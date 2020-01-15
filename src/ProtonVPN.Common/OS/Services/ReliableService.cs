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
using Polly;
using ProtonVPN.Common.Abstract;

namespace ProtonVPN.Common.OS.Services
{
    public class ReliableService : IService
    {
        private readonly Policy<Result> _retryPolicy;
        private readonly IService _origin;

        public ReliableService(Policy<Result> retryPolicy, IService origin)
        {
            _retryPolicy = retryPolicy;
            _origin = origin;
        }

        public string Name => _origin.Name;

        public event EventHandler<string> ServiceStartedHandler;

        public bool IsRunning() => _origin.IsRunning();

        public Result Start()
        {
            var result = _retryPolicy.Execute(() => _origin.Start());
            if (result.Success)
            {
                ServiceStartedHandler?.Invoke(this, Name);
            }

            return result;
        }

        public Result Stop()
        {
            return _retryPolicy.Execute(() => _origin.Stop());
        }
    }
}
