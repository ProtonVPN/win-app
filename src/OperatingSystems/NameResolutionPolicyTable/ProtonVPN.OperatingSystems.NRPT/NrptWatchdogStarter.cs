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

using System.Diagnostics;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.OperatingSystemLogs;
using ProtonVPN.OperatingSystems.NRPT.Contracts;

namespace ProtonVPN.OperatingSystems.NRPT;

public class NrptWatchdogStarter : INrptWatchdogStarter
{
    private readonly IStaticConfiguration _staticConfig;
    private readonly ILogger _logger;

    private Process? _nrptWatchdogProcess;

    public NrptWatchdogStarter(IStaticConfiguration staticConfig, ILogger logger)
    {
        _staticConfig = staticConfig;
        _logger = logger;
    }

    public void Start()
    {
        _logger.Info<OperatingSystemNrptLog>($"Starting NRPT watchdog");

        try
        {
            _nrptWatchdogProcess = Process.Start(new ProcessStartInfo
            {
                FileName = _staticConfig.NrptWatchdogExePath,
                Arguments = $"--pid {Environment.ProcessId}",
                UseShellExecute = false,
                CreateNoWindow = true
            });

            if (_nrptWatchdogProcess is null)
            {
                _logger.Error<OperatingSystemNrptLog>("Failed to start the NRPT watchdog");
            }
            else
            {
                _logger.Info<OperatingSystemNrptLog>($"NRPT watchdog started (process {_nrptWatchdogProcess.Id})");
            }
        }
        catch (Exception ex)
        {
            _logger.Error<OperatingSystemNrptLog>("An exception occurred when starting the NRPT watchdog", ex);
        }
    }
}
