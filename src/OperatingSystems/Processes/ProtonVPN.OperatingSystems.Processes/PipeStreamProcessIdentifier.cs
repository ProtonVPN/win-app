/*
 * Copyright (c) 2024 Proton AG
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
using System.IO.Pipes;
using System.Runtime.InteropServices;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.OperatingSystems.Processes.Contracts;

namespace ProtonVPN.OperatingSystems.Processes;

public class PipeStreamProcessIdentifier : IPipeStreamProcessIdentifier
{
    private readonly ILogger _logger;

    public PipeStreamProcessIdentifier(ILogger logger)
    {
        _logger = logger;
    }

    public string? GetClientProcessFullFilePath(PipeStream pipeStream)
    {
        return GetProcessFullFilePath(GetClientProcessId, pipeStream);
    }

    private string? GetProcessFullFilePath(Func<PipeStream, int> processIdRequester, PipeStream pipeStream)
    {
        try
        {
            int processId = processIdRequester(pipeStream);
            Process? process = processId == 0 ? null : Process.GetProcessById(processId);
            string? filePath = process?.MainModule?.FileName;
            return string.IsNullOrWhiteSpace(filePath) ? null : Path.GetFullPath(filePath);
        }
        catch (Exception ex)
        {
            _logger.Error<AppLog>("Failed to get the PipeStream process.", ex);
            return null;
        }
    }

    private int GetClientProcessId(PipeStream pipeStream)
    {
        if (GetNamedPipeClientProcessId(pipeStream.SafePipeHandle.DangerousGetHandle(), out uint clientProcessId))
        {
            return (int)clientProcessId;
        }
        return 0;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetNamedPipeClientProcessId(IntPtr hNamedPipe, out uint clientProcessId);

    public string? GetServerProcessFullFilePath(PipeStream pipeStream)
    {
        return GetProcessFullFilePath(GetServerProcessId, pipeStream);
    }

    private int GetServerProcessId(PipeStream pipeStream)
    {
        if (GetNamedPipeServerProcessId(pipeStream.SafePipeHandle.DangerousGetHandle(), out uint serverProcessId))
        {
            return (int)serverProcessId;
        }
        return 0;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetNamedPipeServerProcessId(IntPtr hNamedPipe, out uint serverProcessId);
}
