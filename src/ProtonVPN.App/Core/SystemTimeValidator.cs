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
using System.Threading.Tasks;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.OperatingSystemLogs;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.OS;
using ProtonVPN.Modals;

namespace ProtonVPN.Core
{
    public class SystemTimeValidator
    {
        private const int TIME_LIMIT_IN_SECONDS = 60;

        private readonly INtpClient _ntpClient;
        private readonly IModals _modals;
        private readonly SingleAction _validateAction;
        private readonly ILogger _logger;

        public SystemTimeValidator(INtpClient ntpClient, IModals modals, ILogger logger)
        {
            _ntpClient = ntpClient;
            _modals = modals;
            _logger = logger;
            _validateAction = new(ValidateTime);
        }

        public Task Validate()
        {
            return _validateAction.Run();
        }

        private async Task ValidateTime()
        {
            DateTime? networkTime = _ntpClient.GetNetworkUtcTime();
            if (networkTime.HasValue && Math.Abs((networkTime.Value - DateTime.UtcNow).TotalSeconds) > TIME_LIMIT_IN_SECONDS)
            {
                _logger.Warn<OperatingSystemLog>("Incorrect system time detected.");
                _modals.Show<IncorrectSystemTimeModalViewModel>();
            }
        }
    }
}