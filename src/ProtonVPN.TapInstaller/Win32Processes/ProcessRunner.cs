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
using System.ComponentModel;
using System.Diagnostics;
using TapInstaller.Logging;

namespace TapInstaller.Win32Processes
{
    public class ProcessRunner
    {
        private readonly ILogger _logger;
        private readonly string _processPath;
        private readonly string _workingDir;

        public ProcessRunner(string processPath, string workingDir = "")
        {
            _logger = Logger.GetLogger();
            _processPath = processPath;
            _workingDir = workingDir;
        }

        public virtual ProcessExecutionResult Run(string args)
        {
            var process = CreateProcess(args);
            string output = "";
            process.Start();
            _logger.Log($"ProcessRunner: Executing process: {ProcessName(process)}, id: {process.Id}");

            while (!process.StandardOutput.EndOfStream)
                output += process.StandardOutput.ReadLine() + "\n";

            if (!process.WaitForExit(60000))
                throw new Win32Exception("Waiting for process exit timed out.");

            return new ProcessExecutionResult(output, process.ExitCode);
        }

        private Process CreateProcess(string args)
        {
            _logger.Log($"ProcessRunner: Creating process {_processPath} with args: {args}");

            var process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.FileName = _processPath;
            process.StartInfo.Arguments = args;
            if (!string.IsNullOrEmpty(_workingDir))
                process.StartInfo.WorkingDirectory = _workingDir;

            return process;
        }

        private string ProcessName(Process process)
        {
            try
            {
                // Accessing Process.ProcessName throws InvalidOperationException if process has already exited.
                return process.ProcessName;
            }
            catch (InvalidOperationException)
            {
                return string.Empty;
            }
        }
    }
}
