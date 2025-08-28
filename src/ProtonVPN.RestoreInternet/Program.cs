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

using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Runtime.InteropServices;
using ProtonVPN.OperatingSystems.NRPT;

namespace ProtonVPN.RestoreInternet;

class Program
{
    static void Main()
    {
        DeleteWfpFilters();
        DeleteNrptRule();
    }

    private static void DeleteWfpFilters()
    {
        Console.WriteLine("Deleting WFP filters...");

        uint result = RemoveWfpObjects(0);

        if (result == 0)
        {
            Console.WriteLine("Deleted WFP filters");
        }
        else
        {
            Console.WriteLine("Error when deleting WFP filters: " + result);
        }
    }

    [DllImport(
        "ProtonVPN.InstallActions.dll",
        EntryPoint = "RemoveWfpObjects",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint RemoveWfpObjects(long handle);

    private static void DeleteNrptRule()
    {
        Console.WriteLine("Deleting NRPT rule...");
        StaticNrptInvoker.DeleteRule(OnException, OnError, OnSuccess);
    }

    private static void OnException(string message, Exception exception)
    {
        Console.WriteLine($"{message} - Exception: {exception}");
    }

    private static void OnError(string message, List<ErrorRecord> errors)
    {
        foreach (ErrorRecord error in errors)
        {
            Console.WriteLine($"{message} - {error}");
        }
    }

    private static void OnSuccess(string message)
    {
        Console.WriteLine(message);
    }
}