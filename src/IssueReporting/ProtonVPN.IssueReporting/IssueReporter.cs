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
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using ProtonVPN.Common.Legacy.Helpers;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using Sentry;

namespace ProtonVPN.IssueReporting;

public class IssueReporter : IIssueReporter
{
    public IssueReporter(ILogger logger)
    {
        SentryInitializer.SetLogger(logger);
    }

    public void CaptureError(
        Exception e,
        [CallerFilePath] string sourceFilePath = "",
        [CallerMemberName] string sourceMemberName = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        CallerProfile callerProfile = new(sourceFilePath, sourceMemberName, sourceLineNumber);
        IEnumerable<string> fingerprint = GenerateExceptionFingerprint(e);

        SentrySdk.ConfigureScope(scope =>
        {
            scope.Level = SentryLevel.Error;
            scope.SetTag("captured_in",
                $"{callerProfile.SourceClassName}.{callerProfile.SourceMemberName}:{callerProfile.SourceLineNumber}");

            scope.SetFingerprint(fingerprint);
            SentrySdk.CaptureException(e);
        });
    }

    private IEnumerable<string> GenerateExceptionFingerprint(Exception e)
    {
        string exceptionTypeName = string.Empty;
        string classFullName = string.Empty;
        string methodName = string.Empty;
        int? line = null;
        
        try
        {
            exceptionTypeName = e.GetType()?.FullName;
            StackTrace stackTrace = new(e, true);
            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                StackFrame frame = stackTrace.GetFrame(i);
                MethodBase method = frame?.GetMethod();
                classFullName = method?.DeclaringType?.FullName;
                methodName = method?.Name;
                line = frame?.GetFileLineNumber();

                // Our line of code that ended up triggering the Exception
                if (classFullName is not null && classFullName.Contains("ProtonVPN"))
                {
                    break;
                }
            }
        }
        catch
        {
            yield break;
        }

        yield return $"Exception: {exceptionTypeName}";
        yield return $"Type: {classFullName}";
        yield return $"Method: {methodName}()";
        yield return $"Line: {line}";
    }

    public void CaptureError(string message, string description = null)
    {
        CaptureMessage(SentryLevel.Error, message, description);
    }

    private void CaptureMessage(SentryLevel level, string message, string description)
    {
        SentrySdk.ConfigureScope(scope =>
        {
            scope.Level = level;
            if (!string.IsNullOrWhiteSpace(description))
            {
                scope.TransactionName = description;
                scope.SetExtra("description", description);
            }

            scope.SetFingerprint([message]);
            SentrySdk.CaptureMessage(message);
        });
    }

    public void CaptureMessage(string message, string description = null)
    {
        CaptureMessage(SentryLevel.Info, message, description);
    }
}