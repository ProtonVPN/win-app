/*
 * Copyright (c) 2025 Proton AG
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

using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Microsoft.PowerShell;

namespace ProtonVPN.OperatingSystems.NRPT;

public static class StaticNrptInvoker
{
    private const string NRPT_COMMENT = "Force all DNS requests via Proton VPN";
    private const string NRPT_ADD_SCRIPT = "Add-DnsClientNrptRule -Namespace \".\" -NameServers {0} -Comment \"" + NRPT_COMMENT + "\"";
    private const string NRPT_REMOVE_SCRIPT = "Get-DnsClientNrptRule | Where-Object { $_.Comment -eq \"" + NRPT_COMMENT + "\" } | Remove-DnsClientNrptRule -Force";
    
    private static readonly object _lock = new();

    /// <returns>If the NRPT rule was added successfully</returns>
    public static bool CreateRule(string nameServers, Action<string, Exception> onException = null,
        Action<string, List<ErrorRecord>> onError = null, Action<string> onSuccess = null)
    {
        try
        {
            string script = string.Format(NRPT_ADD_SCRIPT, nameServers);
            return ExecuteNrptPowershellCommand("Add", ps => ps.AddScript(script), onError, onSuccess);
        }
        catch (Exception ex)
        {
            if (onException is not null)
            {
                onException("Exception thrown when adding the NRPT rule", ex);
            }
            return false;
        }
    }

    /// <returns>If the NRPT script was executed successfully</returns>
    private static bool ExecuteNrptPowershellCommand(string actionVerb, Func<PowerShell, PowerShell> value,
        Action<string, List<ErrorRecord>> onError = null, Action<string> onSuccess = null)
    {
        lock(_lock)
        {
            InitialSessionState iss = InitialSessionState.CreateDefault();
            iss.ExecutionPolicy = ExecutionPolicy.Bypass;

            using (Runspace runspace = RunspaceFactory.CreateRunspace(iss))
            {
                runspace.Open();
                using (PowerShell ps = PowerShell.Create(runspace))
                {
                    ps.AddCommand("Import-Module").AddArgument("DnsClient").Invoke();
                    ps.Commands.Clear();

                    value(ps).Invoke();

                    if (ps.HadErrors)
                    {
                        List<ErrorRecord> errors = ps.Streams.Error.ToList();
                        if (onError is not null)
                        {
                            onError($"Error when performing NRPT rule '{actionVerb}' action.", errors);
                        }
                        return false;
                    }
                    else
                    {
                        if (onSuccess is not null)
                        {
                            onSuccess($"Success when performing NRPT rule '{actionVerb}' action.");
                        }
                        return true;
                    }
                }
            }
        }
    }

    /// <returns>If the NRPT rule was removed successfully</returns>
    public static bool DeleteRule(Action<string, Exception> onException = null,
        Action<string, List<ErrorRecord>> onError = null, Action<string> onSuccess = null)
    {
        try
        {
            return ExecuteNrptPowershellCommand("Remove", ps => ps.AddScript(NRPT_REMOVE_SCRIPT), onError, onSuccess);
        }
        catch (Exception ex)
        {
            if (onException is not null)
            {
                onException("Exception thrown when removing the NRPT rule", ex);
            }
            return false;
        }
    }
}
