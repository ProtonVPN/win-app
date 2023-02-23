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
using System.IO;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.ProcessLogs;

namespace ProtonVPN.Common.OS.Processes
{
    public abstract class BaseSystemProcess : IOsProcess
    {
        private ILogger _logger;
        protected Process Process;
        private bool _isDisposed;

        protected BaseSystemProcess(ILogger logger, Process process)
        {
            _logger = logger;
            Process = process;
        }

        public string Name => Process.ProcessName;

        public StreamWriter StandardInput => Process.StandardInput;
        public StreamReader StandardOutput => Process.StandardOutput;

        public int ExitCode => Process.ExitCode;

        public event EventHandler<EventArgs<string>> OutputDataReceived;
        public event EventHandler<EventArgs<string>> ErrorDataReceived;
        public event EventHandler Exited;

        public virtual void Start()
        {
            string processName = GetProcessName(Process.StartInfo.FileName);
            _logger.Info<ProcessStartLog>($"Starting new process '{processName}'.");
            Process.Start();
        }

        public bool HasExited()
        {
            try
            {
                return Process?.HasExited != false;
            }
            catch (Win32Exception) { }
            catch (InvalidOperationException) { }

            return true;
        }

        public void WaitForExit(TimeSpan duration)
        {
            if (HasExited())
            {
                return;
            }

            try
            {
                string processName = Process.ProcessName;

                _logger.Info<ProcessStopLog>($"Waiting for process '{processName}' to exit.");
                bool hasExited = Process.WaitForExit(Convert.ToInt32(duration.TotalMilliseconds));
                if (hasExited)
                {
                    _logger.Info<ProcessStopLog>($"The process '{processName}' has exited.");
                }
                else
                {
                    _logger.Warn<ProcessStopLog>($"The process '{processName}' has not exited in the provided time ({duration}).");
                }
            }
            catch (SystemException) { }
        }

        public void Kill()
        {
            if (HasExited())
            {
                return;
            }

            int processId = 0;
            string processName = null;
            try
            {
                processId = Process.Id;
                processName = Process.ProcessName;

                _logger.Info<ProcessStopLog>($"Killing process '{processName}' with ID '{processId}'.");
                Process.Kill();
            }
            catch (Exception ex) when (ex is InvalidOperationException or Win32Exception)
            {
                _logger.Warn<ProcessStopLog>($"Failed to kill process '{processName}' with ID '{processId}'.", ex);
            }
        }

        protected void AddEventHandlers()
        {
            Process.OutputDataReceived += Process_OutputDataReceived;
            Process.ErrorDataReceived += Process_ErrorDataReceived;
            Process.Exited += Process_Exited;
        }

        private string GetProcessName(string executablePath)
        {
            return Path.GetFileNameWithoutExtension(executablePath);
        }

        private void RemoveEventHandlers()
        {
            Process.OutputDataReceived -= Process_OutputDataReceived;
            Process.ErrorDataReceived -= Process_ErrorDataReceived;
            Process.Exited -= Process_Exited;
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                OutputDataReceived?.Invoke(this, new EventArgs<string>(e.Data));
            }
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                ErrorDataReceived?.Invoke(this, new EventArgs<string>(e.Data));
            }
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            Exited?.Invoke(this, e);
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _logger = null;
            if (Process != null)
            {
                RemoveEventHandlers();

                Process.Dispose();
                Process = null;
            }

            OutputDataReceived = null;
            ErrorDataReceived = null;
            Exited = null;

            _isDisposed = true;
        }
    }
}
