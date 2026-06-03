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
using System.Runtime.CompilerServices;
using TaskExtensions = ProtonVPN.Common.Core.Extensions.TaskExtensions;

namespace ProtonVPN.Logging.Events;

public static class GlobalExceptionHandler
{
    public static void Initialize()
    {
        EventLogger.Initialize();

        AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        TaskExtensions.SetDefaultExceptionHandler(ex =>
            TryWriteEventLog("Fire-and-forget task exception", ex));
    }

    private static void OnAppDomainUnhandledException(object? sender, UnhandledExceptionEventArgs eventArgs)
    {
        const string HANDLER = "AppDomain unhandled exception";
        string terminatingText = eventArgs.IsTerminating ? "(Terminating)" : string.Empty;

        Exception ex = eventArgs.ExceptionObject switch
        {
            RuntimeWrappedException wrappedException => new Exception(
                $"Non-Exception object thrown: " +
                $"{wrappedException.WrappedException?.GetType().FullName} — {wrappedException.WrappedException}",
                wrappedException),

            Exception exception => exception,

            object thrownObject => new Exception(
                $"Non-Exception object thrown: " +
                $"{thrownObject.GetType().FullName} — {thrownObject}"),

            null => new Exception("Non-Exception object thrown: <null>")
        };

        TryWriteEventLog($"{HANDLER} {terminatingText}", ex);
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs ex)
    {
        TryWriteEventLog("Unobserved task exception", ex.Exception);
        ex.SetObserved();
    }

    public static void TryWriteEventLog(string handler, Exception? ex)
    {
        try
        {
            if (ex is null)
            {
                return;
            }

            Exception diagnosticEx = ex is AggregateException agg && agg.Flatten().InnerExceptions.Count == 1
                ? agg.Flatten().InnerExceptions[0]
                : ex;

            string flattenedDetails = ex is AggregateException aggregate
                ? FormatAggregateException(aggregate)
                : string.Empty;

            string message =
                $"Proton VPN Windows error event log{Environment.NewLine}" +
                $"Date: {DateTimeOffset.UtcNow:o}{Environment.NewLine}" +
                $"Handler: {handler}{Environment.NewLine}" +
                Environment.NewLine +
                $"Exception HResult: 0x{diagnosticEx.HResult:X8}{Environment.NewLine}" +
                $"Exception type: {diagnosticEx.GetType().FullName}{Environment.NewLine}" +
                $"Exception message: {diagnosticEx.Message}{Environment.NewLine}" +
                flattenedDetails +
                Environment.NewLine +
                $"Full exception: {ex}";

            EventLogger.Log(EventLogEntryType.Error, message);
        }
        catch { }
    }

    private static string FormatAggregateException(AggregateException aggregate)
    {
        AggregateException flattened = aggregate.Flatten();
        if (flattened.InnerExceptions.Count <= 1)
        {
            return string.Empty;
        }

        string details = string.Join(Environment.NewLine,
            flattened.InnerExceptions.Select((e, i) =>
                $"  [{i + 1}] {e.GetType().FullName}: {e.Message}"));

        return $"Inner exceptions ({flattened.InnerExceptions.Count}):{Environment.NewLine}" +
               details + Environment.NewLine;
    }
}