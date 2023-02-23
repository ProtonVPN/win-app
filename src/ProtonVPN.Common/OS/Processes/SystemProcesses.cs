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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ProtonVPN.Common.Logging;

namespace ProtonVPN.Common.OS.Processes
{
    public class SystemProcesses : IOsProcesses
    {
        private readonly ILogger _logger;

        public SystemProcesses(ILogger logger)
        {
            _logger = logger;
        }

        public IOsProcess Process(string filename, string arguments)
        {
            var process = CreateProcess(filename, arguments);
            return new SystemProcess(_logger, process);
        }

        public IOsProcess ElevatedProcess(string filename, string arguments)
        {
            var process = CreateElevatedProcess(filename, arguments);
            return new SystemProcess(_logger, process);
        }

        public IOsProcess CommandLineProcess(string arguments)
        {
            return new CmdOutputProcess(_logger, new Process
            {
                StartInfo = new ProcessStartInfo("cmd.exe", arguments).NoRedirectInfo()
            });
        }

        public IOsProcess ElevatedCommandLineProcess(string arguments)
        {
            return new CmdOutputProcess(_logger, new Process
            {
                StartInfo = new ProcessStartInfo("cmd.exe", arguments).ElevatedInfo()
            });
        }

        public IOsProcess[] ProcessesByName(string filename)
        {
            var name = GetProcessName(filename);
            return GetProcessesByName(name).Select(p => (IOsProcess) new SystemProcess(_logger, p)).ToArray();
        }

        public IOsProcess[] ProcessesByPath(string path)
        {
            var filename = Path.GetFileNameWithoutExtension(path) ?? string.Empty;
            var list = System.Diagnostics.Process.GetProcessesByName(filename);
            var result = new List<IOsProcess>();

            foreach (var p in list)
            {
                try
                {
                    if (p.MainModule?.FileName == path)
                    {
                        result.Add(new SystemProcess(_logger, p));
                    }
                    else
                    {
                        p.Dispose();
                    }
                }
                catch (Win32Exception)
                {
                    p.Dispose();
                }
            }

            return result.ToArray();
        }

        public void Open(string filename)
        {
            try
            {
                System.Diagnostics.Process.Start(filename);
            }
            catch (Win32Exception)
            {
            }
        }

        private Process CreateProcess(string filename, string arguments)
        {
            return new Process
            {
                StartInfo = new ProcessStartInfo(filename, arguments).StandardInfo(),
                EnableRaisingEvents = true
            };
        }

        private Process CreateElevatedProcess(string filename, string arguments)
        {
            return new Process
            {
                StartInfo =
                {
                    FileName = filename,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    UseShellExecute = true,
                    Verb = "runas",
                    WorkingDirectory = Environment.CurrentDirectory
                },
                EnableRaisingEvents = true
            };
        }

        private Process[] GetProcessesByName(string name)
        {
            return System.Diagnostics.Process.GetProcessesByName(name);
        }

        private string GetProcessName(string filename)
        {
            return Path.GetFileNameWithoutExtension(filename);
        }
    }
}
