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

using System.Diagnostics;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ProcessLogs;
using ProtonVPN.OperatingSystems.Processes.Contracts;

namespace ProtonVPN.OperatingSystems.Processes;

public class CommandLineCaller : ICommandLineCaller
{
    private const int PROCESS_TIMEOUT_IN_MILLISECONDS = 1000;

    private readonly ILogger _logger;

    public CommandLineCaller(ILogger logger)
    {
        _logger = logger;
    }

    public void ExecuteElevated(string arguments)
    {
        _logger.Info<ProcessStartLog>($"Running command line argument '{arguments}'.");
        try
        {
            Process process = new()
            {
                StartInfo = new ProcessStartInfo("cmd.exe", arguments)
                {
                    CreateNoWindow = true,
                    UseShellExecute = true,
                    RedirectStandardOutput = false,
                    Verb = "runas"
                }
            };
            process.Start();
            process.WaitForExit(PROCESS_TIMEOUT_IN_MILLISECONDS);
        } 
        catch
        {
            _logger.Error<ProcessStartLog>($"Failed to run command line argument '{arguments}'.");
        }
    }
}