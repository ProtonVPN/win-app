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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Common.Legacy.OS.Processes;

public class SystemProcesses : IOsProcesses
{
    private readonly ILogger _logger;

    public SystemProcesses(ILogger logger)
    {
        _logger = logger;
    }

    public IOsProcess BuildProcess(string filename, IEnumerable<string> arguments)
    {
        Process process = CreateProcess(filename, arguments);
        return new SystemProcess(_logger, process);
    }

    public IOsProcess BuildElevatedProcess(string filename, IEnumerable<string> arguments)
    {
        Process process = CreateElevatedProcess(filename, arguments);
        return new SystemProcess(_logger, process);
    }

    public IOsProcess BuildCommandLineProcess(IEnumerable<string> arguments)
    {
        return new CmdOutputProcess(_logger, new Process
        {
            StartInfo = new ProcessStartInfo("cmd.exe", arguments ?? []).NoRedirectInfo()
        });
    }

    public IOsProcess[] GetProcessesByName(string filename)
    {
        string name = GetProcessName(filename);
        return GetProcessesByNameInner(name).Select(p => (IOsProcess)new SystemProcess(_logger, p)).ToArray();
    }

    public IOsProcess[] GetProcessesByPath(string path)
    {
        string filename = Path.GetFileNameWithoutExtension(path) ?? string.Empty;
        Process[] list = Process.GetProcessesByName(filename);
        List<IOsProcess> result = [];

        foreach (Process p in list)
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
            Process.Start(new ProcessStartInfo
            {
                FileName = filename,
                UseShellExecute = true
            });
        }
        catch (Win32Exception)
        {
        }
    }

    private static Process CreateProcess(string filename, IEnumerable<string> arguments)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo(filename).StandardInfo();

        foreach (string argument in arguments ?? [])
        {
            startInfo.ArgumentList.Add(argument);
        }

        return new Process
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true
        };
    }

    private static Process CreateElevatedProcess(string filename, IEnumerable<string> arguments)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = filename,
            CreateNoWindow = true,
            UseShellExecute = true,
            Verb = "runas",
            WorkingDirectory = Environment.CurrentDirectory,
        };

        foreach (string argument in arguments ?? [])
        {
            startInfo.ArgumentList.Add(argument);
        }

        return new Process
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true
        };
    }

    private static Process[] GetProcessesByNameInner(string name)
    {
        return Process.GetProcessesByName(name);
    }

    private static string GetProcessName(string filename)
    {
        return Path.GetFileNameWithoutExtension(filename);
    }
}