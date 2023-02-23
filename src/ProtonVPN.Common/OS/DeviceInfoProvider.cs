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
using DeviceId;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.OperatingSystemLogs;

namespace ProtonVPN.Common.OS
{
    public class DeviceInfoProvider : IDeviceInfoProvider
    {
        private readonly ILogger _logger;
        private string _deviceId;

        public DeviceInfoProvider(ILogger logger)
        {
            _logger = logger;
        }

        public string GetDeviceId()
        {
            if (!_deviceId.IsNullOrEmpty())
            {
                return _deviceId;
            }

            try
            {
                _deviceId = new DeviceIdBuilder()
                    .AddMachineName()
                    .OnWindows(windows => windows
                        .AddProcessorId()
                        .AddMotherboardSerialNumber())
                    .ToString();
            }
            catch (Exception e)
            {
                _logger.Error<OperatingSystemLog>("Failed to generate device id.", e);
                _deviceId = "Undefined";
            }

            return _deviceId;
        }
    }
}