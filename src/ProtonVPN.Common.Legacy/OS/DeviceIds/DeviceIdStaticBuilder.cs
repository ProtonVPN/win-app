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
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Legacy.Extensions;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.OperatingSystemLogs;

namespace ProtonVPN.Common.Legacy.OS.DeviceIds;

public static class DeviceIdStaticBuilder
{
    private static ILogger _logger;
    private static Exception _cachedException;
    private static string _deviceId;

    public static void SetLogger(ILogger logger)
    {
        _logger = logger;
        if (_cachedException is not null)
        {
            LogError(_cachedException, "[Cached exception] ");
            _cachedException = null;
        }
    }

    public static string GetDeviceId()
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
            LogOrCacheError(e);
            _deviceId = "Undefined";
        }

        return _deviceId;
    }

    private static void LogOrCacheError(Exception e)
    {
        if (_logger is null)
        {
            _cachedException = e;
        }
        else
        {
            LogError(e);
        }
    }

    private static void LogError(Exception e, string prefix = null)
    {
        _logger.Error<OperatingSystemLog>($"{prefix}Failed to generate device id.", e);
    }
}
