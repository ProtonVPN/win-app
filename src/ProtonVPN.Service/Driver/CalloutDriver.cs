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
using ProtonVPN.Common.OS.Services;

namespace ProtonVPN.Service.Driver
{
    public class CalloutDriver : IDriver
    {
        private readonly IService _service;

        public CalloutDriver(IService service)
        {
            _service = service;
        }

        public void Start()
        {
            if (!_service.Running())
            {
                _service.StartAsync(CancellationToken.None).Wait();
            }
        }

        public void Stop()
        {
            if (_service.Running())
            {
                _service.StopAsync(CancellationToken.None).Wait();
            }
        }
    }
}
