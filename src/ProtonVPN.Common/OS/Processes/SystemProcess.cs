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

using ProtonVPN.Common.Logging;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace ProtonVPN.Common.OS.Processes
{
    public class SystemProcess : IOsProcess
    {
        private ILogger _logger;
        private Process _process;
        private bool _isDisposed;

        public SystemProcess(ILogger logger, Process process)
        {
            _logger = logger;
            _process = process;

            AddEventHandlers();
        }

        public string Name => _process.ProcessName;

        public StreamWriter StandardInput => _process.StandardInput;

        public int ExitCode => _process.ExitCode;

        public event EventHandler<EventArgs<string>> OutputDataReceived;
        public event EventHandler<EventArgs<string>> ErrorDataReceived;
        public event EventHandler Exited;

        public void Start()
        {
            var processName = GetProcessName(_process.StartInfo.FileName);
            _logger.Info($"Process: Starting new {processName} process");
            _process.Start();

            if (_process.StartInfo.RedirectStandardError)
                _process.BeginErrorReadLine();

            if (_process.StartInfo.RedirectStandardOutput)
                _process.BeginOutputReadLine();
        }

        public bool HasExited()
        {
            try
            {
                return _process?.HasExited != false;
            }
            catch (Win32Exception) { }
            catch (InvalidOperationException) { }

            return true;
        }

        public void WaitForExit(TimeSpan duration)
        {
            if (HasExited())
                return;

            try
            {
                var processName = _process.ProcessName;

                _logger.Info($"Process: Waiting for {processName} process to exit");
                _process.WaitForExit(Convert.ToInt32(duration.TotalMilliseconds));
                _logger.Info($"Process: Done waiting for {processName} process to exit");
            }
            catch (SystemException) { }
        }

        public void Kill()
        {
            if (HasExited())
                return;

            int processId = 0;
            string processName = null;
            try
            {
                processId = _process.Id;
                processName = _process.ProcessName;

                _logger.Info($"Process: Killing {processName} process ID {processId}");
                _process.Kill();
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is Win32Exception)
            {
                _logger.Warn($"Process: Failed to kill {processName} process ID {processId}. {ex.Message}");
            }
        }

        private void AddEventHandlers()
        {
            _process.OutputDataReceived += Process_OutputDataReceived;
            _process.ErrorDataReceived += Process_ErrorDataReceived;
            _process.Exited += Process_Exited;
        }

        private void RemoveEventHandlers()
        {
            _process.OutputDataReceived -= Process_OutputDataReceived;
            _process.ErrorDataReceived -= Process_ErrorDataReceived;
            _process.Exited -= Process_Exited;
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                OutputDataReceived?.Invoke(this, new EventArgs<string>(e.Data));
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                ErrorDataReceived?.Invoke(this, new EventArgs<string>(e.Data));
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            Exited?.Invoke(this, e);
        }

        private string GetProcessName(string executablePath)
        {
            return Path.GetFileNameWithoutExtension(executablePath);
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _logger = null;
            if (_process != null)
            { 
                RemoveEventHandlers();

                _process.Dispose();
                _process = null;
            }

            OutputDataReceived = null;
            ErrorDataReceived = null;
            Exited = null;

            _isDisposed = true;
        }
    }
}
