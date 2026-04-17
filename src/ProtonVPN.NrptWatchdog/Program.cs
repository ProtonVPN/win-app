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
using System.Diagnostics;
using ProtonVPN.OperatingSystems.NRPT;

namespace ProtonVPN.NrptWatchdog;

public class Program
{
    public static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine("No arguments received.");
            ShowUsage();
            return 1;
        }

        switch (args[0])
        {
            case "--force":
                RunForce();
                return 0;

            case "--pid" when args.Length >= 2 && int.TryParse(args[1], out int pid):
                return RunProcessWatcher(pid);

            default:
                Console.Error.WriteLine($"Arguments '{string.Join(" ", args)}' not recognized.");
                ShowUsage();
                return 1;
        }
    }

    private static void ShowUsage()
    {
        Console.Error.WriteLine("Usage:");
        Console.Error.WriteLine("  ProtonVPN.NrptWatchdog.exe --pid <PID>   Watch a process and delete NRPT rule on exit");
        Console.Error.WriteLine("  ProtonVPN.NrptWatchdog.exe --force       Delete NRPT rule immediately");
    }

    private static void RunForce()
    {
        Console.WriteLine("Force mode: Deleting NRPT rule.");
        DeleteNrptRule();
    }

    private static int RunProcessWatcher(int pid)
    {
        Console.Error.WriteLine($"Process mode: Trying to find process {pid}");
        Process target;
        try
        {
            target = Process.GetProcessById(pid);
        }
        catch (ArgumentException)
        {
            Console.Error.WriteLine($"Error: Process {pid} does not exist.");
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: Failed to fetch process with ID {pid}. Exception: {ex}");
            return 1;
        }

        Console.WriteLine($"Watching PID {pid} ({target.ProcessName})...");

        try
        {
            target.WaitForExit();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"An exception occurred when waiting for process {pid}: {ex}");
        }
        finally
        {
            Console.WriteLine($"PID {pid} exited. Deleting NRPT rule.");
            DeleteNrptRule();
            target.Dispose();
        }
        return 0;
    }

    private static void DeleteNrptRule()
    {
        Console.WriteLine("Deleting NRPT rule...");
        StaticNrptInvoker.DeleteRule(OnException, OnSuccess);
        Console.WriteLine("Finished");
    }

    private static void OnException(string message, Exception exception)
    {
        Console.WriteLine($"{message} - Exception: {exception}");
    }

    private static void OnSuccess(string message)
    {
        Console.WriteLine(message);
    }
}