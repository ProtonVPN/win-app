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
using System.Runtime.CompilerServices;
using ProtonVPN.Common.Helpers;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using Sentry;

namespace ProtonVPN.IssueReporting
{
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
            SentrySdk.WithScope(scope =>
            {
                scope.Level = SentryLevel.Error;
                scope.SetTag("captured_in",
                    $"{callerProfile.SourceClassName}.{callerProfile.SourceMemberName}:{callerProfile.SourceLineNumber}");
                SentrySdk.CaptureException(e);
            });
        }

        public void CaptureError(string message)
        {
            SentrySdk.CaptureEvent(new SentryEvent { Message = message, Level = SentryLevel.Error });
        }

        public void CaptureMessage(string message, string description = null)
        {
            SentrySdk.WithScope(scope =>
            {
                if (!string.IsNullOrWhiteSpace(description))
                {
                    scope.TransactionName = description;
                    scope.SetExtra("description", description);
                }

                scope.SetFingerprint([message]);
                SentrySdk.CaptureMessage(message);
            });
        }
    }
}