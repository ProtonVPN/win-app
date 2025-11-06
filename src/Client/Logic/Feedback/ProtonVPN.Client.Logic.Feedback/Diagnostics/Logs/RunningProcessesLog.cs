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
using System.Text;
using ProtonVPN.Configurations.Contracts;

namespace ProtonVPN.Client.Logic.Feedback.Diagnostics.Logs;

public class RunningProcessesLog : LogBase
{
    public RunningProcessesLog(IStaticConfiguration config) : base(config.DiagnosticLogsFolder, "Processes.txt")
    {
    }

    public override void Write()
    {
        File.WriteAllText(Path, GenerateContent());
    }

    private string GenerateContent()
    {
        StringBuilder result = new();
        try
        {
            Process[] processes = Process.GetProcesses();

            foreach (Process process in processes)
            {
                try
                {
                    result.AppendLine($"Name: {process.ProcessName}");
                    result.AppendLine($"PID: {process.Id}");

                    string version = process.MainModule?.FileVersionInfo?.FileVersion ?? string.Empty;
                    if (!string.IsNullOrEmpty(version))
                    {
                        result.AppendLine($"Version: {version}");
                    }
                }
                catch
                {
                    // ignored
                }
                finally
                {
                    result.AppendLine();
                    process.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            result.AppendLine($"Error retrieving process list: {ex.Message}");
        }

        return result.ToString();
    }
}