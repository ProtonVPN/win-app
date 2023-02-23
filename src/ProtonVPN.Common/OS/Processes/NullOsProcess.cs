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
using System.IO;

namespace ProtonVPN.Common.OS.Processes
{
    public class NullOsProcess : IOsProcess
    {
        public string Name => "";
        public StreamWriter StandardInput => StreamWriter.Null;
        public StreamReader StandardOutput => StreamReader.Null;
        public void AddEventHandlers() => throw new NotImplementedException();

        public int ExitCode => 0;

        public event EventHandler<EventArgs<string>> OutputDataReceived = delegate { };
        public event EventHandler<EventArgs<string>> ErrorDataReceived = delegate { };
        public event EventHandler Exited = delegate { };

        public void Start()
        {
        }

        public bool HasExited() => true;

        public void WaitForExit(TimeSpan duration)
        {
        }

        public void Kill()
        {
        }

        public void Dispose()
        {
            OutputDataReceived = null;
            ErrorDataReceived = null;
            Exited = null;
        }
    }
}
