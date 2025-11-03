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

using System.Diagnostics;

namespace ProtonVPN.Logging.Events;

public static class GlobalExceptionHandler
{
    public static void Initialize()
    {
        EventLogger.Initialize();

        AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    private static void OnAppDomainUnhandledException(object? sender, UnhandledExceptionEventArgs eventArgs)
    {
        const string HANDLER = "AppDomain unhandled exception";
        Exception? ex = eventArgs.ExceptionObject as Exception;
        string terminatingText = eventArgs.IsTerminating ? "(Terminating)" : string.Empty;

        TryWriteEventLog($"{HANDLER} {terminatingText}", ex);
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs ex)
    {
        TryWriteEventLog("Unobserved task exception", ex.Exception);
    }

    public static void TryWriteEventLog(string handler, Exception? ex)
    {
        try
        {
            if (ex is null)
            {
                return;
            }

            string message = $"Proton VPN Windows error event log{Environment.NewLine}" +
                    $"Date: {DateTime.UtcNow:o}{Environment.NewLine}" +
                    $"Handler: {handler}{Environment.NewLine}" +
                    Environment.NewLine +
                    $"Exception HResult: 0x{ex.HResult:X8}{Environment.NewLine}" +
                    $"Exception type: {ex.GetType().FullName}{Environment.NewLine}" +
                    $"Exception message: {ex.Message}{Environment.NewLine}" +
                    Environment.NewLine +
                    $"Full exception: {ex}";

            EventLogger.Log(EventLogEntryType.Error, message);
        }
        catch { }
    }
}
