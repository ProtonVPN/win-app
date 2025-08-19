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

    public void Execute(string arguments)
    {
        RunCommand(arguments, isElevated: false);
    }

    public void ExecuteElevated(string arguments)
    {
        RunCommand(arguments, isElevated: true);
    }

    private void RunCommand(string arguments, bool isElevated)
    {
        string commandDescription = (isElevated ? "elevated " : "") + "command line argument";

        _logger.Info<ProcessStartLog>($"Running {commandDescription} '{arguments}'.");

        try
        {
            Process process = new()
            {
                StartInfo = new ProcessStartInfo("cmd.exe", arguments)
                {
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                    // Because UseShellExecute is true, the property CreateNoWindow is ignored and therefore there is no need to set it
                }
            };
            if (isElevated)
            {
                process.StartInfo.Verb = "runas";
            }
            process.Start();
            process.WaitForExit(PROCESS_TIMEOUT_IN_MILLISECONDS);

            _logger.Info<ProcessStartLog>($"Finished running the {commandDescription} '{arguments}'.");
        }
        catch (Exception e)
        {
            _logger.Error<ProcessStartLog>($"Failed to run {commandDescription} '{arguments}'.", e);
        }
    }

    public async Task ExecuteMultipleAsync(List<string> commands)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = "cmd.exe",
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = false,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        try
        {
            Process? process = Process.Start(startInfo);

            if (process == null)
            {
                _logger.Error<ProcessStartLog>("Failed to start cmd.exe process.");
                return;
            }

            Task<string> errorTask = process.StandardError.ReadToEndAsync();

            foreach (string command in commands)
            {
                process.StandardInput.WriteLine(command);
            }

            process.StandardInput.WriteLine("exit");
            process.StandardInput.Flush();
            process.StandardInput.Close();

            await process.WaitForExitAsync().ConfigureAwait(false);

            string errors = await errorTask.ConfigureAwait(false);

            if (!string.IsNullOrEmpty(errors))
            {
                _logger.Error<ProcessStartLog>($"Failed to execute multiple commands. Errors: {errors}");
            }
        }
        catch (Exception e)
        {
            _logger.Error<ProcessStartLog>("Failed to execute multiple commands.", e);
        }
    }
}