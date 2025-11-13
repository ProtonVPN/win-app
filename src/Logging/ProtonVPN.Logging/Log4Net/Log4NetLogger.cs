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
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using ProtonVPN.Common.Core.Helpers;
using ProtonVPN.Common.Legacy.Helpers;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Logging.Log4Net;

public class Log4NetLogger : Log4NetLoggerInitializer, ILogger
{
    private const int MAX_RECENT_LOG_LINES = 100;

    private readonly IList<string> _recentLogs = new List<string>();

    public Log4NetLogger(ILoggerConfiguration loggerConfiguration)
        : base(loggerConfiguration)
    {
        Info<AppStartLog>($"=== Booting Proton VPN {AssemblyVersion.Get()} [{OSVersion.GetPlatformString()}] {OSArchitecture.StringValue} ===");
    }

    public IList<string> GetRecentLogs()
    {
        return _recentLogs;
    }

    public void Debug<TEvent>(
        string message,
        Exception exception = null,
        int stackTraceDepth = 0,
        [CallerFilePath] string sourceFilePath = "",
        [CallerMemberName] string sourceMemberName = "",
        [CallerLineNumber] int sourceLineNumber = 0)
        where TEvent : ILogEvent, new()
    {
        CallerProfile callerProfile = new(sourceFilePath, sourceMemberName, sourceLineNumber);
        string fullLogMessage = CreateFullLogMessage<TEvent>(message, callerProfile, stackTraceDepth);
        if (exception == null)
        {
            InternalLogger.Debug(fullLogMessage);
            AddMessageToRecentLogs(fullLogMessage);
        }
        else
        {
            InternalLogger.Debug(fullLogMessage, exception);
            AddMessageToRecentLogs(fullLogMessage, exception);
        }
    }

    public void Info<TEvent>(
        string message,
        Exception exception = null,
        int stackTraceDepth = 0,
        [CallerFilePath] string sourceFilePath = "",
        [CallerMemberName] string sourceMemberName = "",
        [CallerLineNumber] int sourceLineNumber = 0)
        where TEvent : ILogEvent, new()
    {
        CallerProfile callerProfile = new(sourceFilePath, sourceMemberName, sourceLineNumber);
        string fullLogMessage = CreateFullLogMessage<TEvent>(message, callerProfile, stackTraceDepth);
        if (exception == null)
        {
            InternalLogger.Info(fullLogMessage);
            AddMessageToRecentLogs(fullLogMessage);
        }
        else
        {
            InternalLogger.Info(fullLogMessage, exception);
            AddMessageToRecentLogs(fullLogMessage, exception);
        }
    }

    public void Warn<TEvent>(
        string message,
        Exception exception = null,
        int stackTraceDepth = 0,
        [CallerFilePath] string sourceFilePath = "",
        [CallerMemberName] string sourceMemberName = "",
        [CallerLineNumber] int sourceLineNumber = 0)
        where TEvent : ILogEvent, new()
    {
        CallerProfile callerProfile = new(sourceFilePath, sourceMemberName, sourceLineNumber);
        string fullLogMessage = CreateFullLogMessage<TEvent>(message, callerProfile, stackTraceDepth);
        if (exception == null)
        {
            InternalLogger.Warn(fullLogMessage);
            AddMessageToRecentLogs(fullLogMessage);
        }
        else
        {
            InternalLogger.Warn(fullLogMessage, exception);
            AddMessageToRecentLogs(fullLogMessage, exception);
        }
    }

    public void Error<TEvent>(
        string message,
        Exception exception = null,
        int stackTraceDepth = 0,
        [CallerFilePath] string sourceFilePath = "",
        [CallerMemberName] string sourceMemberName = "",
        [CallerLineNumber] int sourceLineNumber = 0)
        where TEvent : ILogEvent, new()
    {
        CallerProfile callerProfile = new(sourceFilePath, sourceMemberName, sourceLineNumber);
        string fullLogMessage = CreateFullLogMessage<TEvent>(message, callerProfile, stackTraceDepth);
        if (exception == null)
        {
            InternalLogger.Error(fullLogMessage);
            AddMessageToRecentLogs(fullLogMessage);
        }
        else
        {
            InternalLogger.Error(fullLogMessage, exception);
            AddMessageToRecentLogs(fullLogMessage, exception);
        }
    }

    public void Fatal<TEvent>(
        string message,
        Exception exception = null,
        int stackTraceDepth = 0,
        [CallerFilePath] string sourceFilePath = "",
        [CallerMemberName] string sourceMemberName = "",
        [CallerLineNumber] int sourceLineNumber = 0)
        where TEvent : ILogEvent, new()
    {
        CallerProfile callerProfile = new(sourceFilePath, sourceMemberName, sourceLineNumber);
        string fullLogMessage = CreateFullLogMessage<TEvent>(message, callerProfile, stackTraceDepth);
        if (exception == null)
        {
            InternalLogger.Fatal(fullLogMessage);
            AddMessageToRecentLogs(fullLogMessage);
        }
        else
        {
            InternalLogger.Fatal(fullLogMessage, exception);
            AddMessageToRecentLogs(fullLogMessage, exception);
        }
    }

    private string CreateFullLogMessage<TEvent>(string message, CallerProfile callerProfile, int stackTraceDepth = 0)
        where TEvent : ILogEvent, new()
    {
        string json = GenerateMetadataJson(callerProfile, stackTraceDepth);
        return $"{new TEvent()} | {message} | {json}";
    }

    private string GenerateMetadataJson(CallerProfile callerProfile, int stackTraceDepth = 0)
    {
        IDictionary<string, object> metadataDictionary = new Dictionary<string, object>();
        
        string callerInfo = $"{callerProfile.SourceClassName}.{callerProfile.SourceMemberName}:{callerProfile.SourceLineNumber}";
        
        if (stackTraceDepth > 0)
        {
            string[] stackFrames = CaptureStackTrace(stackTraceDepth);
            if (stackFrames.Length > 0)
            {
                callerInfo = $"{callerInfo} < {string.Join(" < ", stackFrames)}";
            }
        }
        
        metadataDictionary.Add("Caller", callerInfo);

        return JsonConvert.SerializeObject(metadataDictionary);
    }

    private static string[] CaptureStackTrace(int depth)
    {
        // Skip: CaptureStackTrace, GenerateMetadataJson, CreateFullLogMessage, logging method (Info/Warn/etc), and the actual caller (already in CallerProfile)
        StackTrace stackTrace = new(skipFrames: 5, fNeedFileInfo: true);

        List<string> frames = new();

        for (int i = 0; i < stackTrace.FrameCount && frames.Count < depth; i++)
        {
            if (stackTrace.GetFrame(i) is not StackFrame frame ||
                frame.GetMethod() is not MethodBase method)
            {
                continue;
            }

            // Filter out framework code, compiler-generated code, and other non-application code
            if (!IsApplicationCode(method))
            {
                continue;
            }

            int lineNumber = frame.GetFileLineNumber();
            string className = method.DeclaringType?.Name ?? "Unknown";
            string methodName = method.Name;

            if (lineNumber > 0)
            {
                frames.Add($"{className}.{methodName}:{lineNumber}");
            }
            else
            {
                frames.Add($"{className}.{methodName}");
            }
        }

        return [.. frames];
    }

    private static bool IsApplicationCode(MethodBase method)
    {
        if (method.DeclaringType == null)
        {
            return false;
        }

        string typeNamespace = method.DeclaringType.Namespace ?? string.Empty;

        // Only include ProtonVPN application code
        if (!typeNamespace.StartsWith("ProtonVPN", StringComparison.Ordinal))
        {
            return false;
        }

        // Exclude compiler-generated code
        if (method.DeclaringType.IsDefined(typeof(CompilerGeneratedAttribute), false) ||
            method.IsDefined(typeof(CompilerGeneratedAttribute), false))
        {
            return false;
        }

        return true;
    }

    private void AddMessageToRecentLogs(string message, Exception exception = null, [CallerMemberName] string level = "")
    {
        message = $"{DateTime.UtcNow:O} | {level.ToUpper()} | {message}";
        if (exception != null)
        {
            message += $" {exception}";
        }
        _recentLogs.Add(message);

        while (_recentLogs.Count > MAX_RECENT_LOG_LINES)
        {
            _recentLogs.RemoveAt(0);
        }
    }
}