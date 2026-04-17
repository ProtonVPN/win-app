/*
 * Copyright (c) 2026 Proton AG
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

using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.OperatingSystemLogs;
using ProtonVPN.OperatingSystems.NRPT.Contracts;
using ProtonVPN.OperatingSystems.TaskScheduling.Contracts;

namespace ProtonVPN.OperatingSystems.NRPT;

public class NrptWatchdogScheduler : INrptWatchdogScheduler
{
    private const string TASK_NAME = "Proton VPN NRPT watchdog";

    private readonly IStaticConfiguration _staticConfig;
    private readonly ILogger _logger;
    private readonly ITaskScheduler _taskScheduler;

    public NrptWatchdogScheduler(IStaticConfiguration staticConfig, ILogger logger, ITaskScheduler taskScheduler)
    {
        _staticConfig = staticConfig;
        _logger = logger;
        _taskScheduler = taskScheduler;
    }

    public void Schedule()
    {
        _logger.Info<OperatingSystemNrptLog>("Scheduling NRPT watchdog to start on device boot");

        bool result = _taskScheduler.Create(TASK_NAME, _staticConfig.NrptWatchdogExePath, "--force");

        if (result)
        {
            _logger.Info<OperatingSystemNrptLog>("NRPT watchdog task scheduled");
        }
        else
        {
            _logger.Error<OperatingSystemNrptLog>("Failed to schedule NRPT watchdog to start on device boot");
        }
    }
}
