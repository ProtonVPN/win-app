/*
 * Copyright (c) 2020 Proton Technologies AG
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
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;

namespace ProtonVPN.Common.Logging
{
    public class UnhandledExceptionLogging
    {
        private readonly ILogger _logger;

        public UnhandledExceptionLogging(ILogger logger)
        {
            _logger = logger;
        }

        public void CaptureUnhandledExceptions()
        {
            AppDomain.CurrentDomain.UnhandledException += LogUnhandledException;
        }

        public void CaptureTaskExceptions()
        {
            TaskScheduler.UnobservedTaskException += LogTaskException;
        }

        private void LogTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            _logger.Error<AppLog>($"Unobserved exception occurred: {e.Exception.Message}");
            LogAggregatedException(e.Exception);
        }

        private void LogUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is AggregateException aggregateException)
            {
                _logger.Fatal<AppCrashLog>($"Aggregate exception occurred: {aggregateException.Message}");
                LogAggregatedException(aggregateException);
            }
            else
            {
                _logger.Fatal<AppCrashLog>(e.ExceptionObject.ToString());
            }
        }

        private void LogAggregatedException(AggregateException e)
        {
            ReadOnlyCollection<Exception> innerExceptions = e.Flatten().InnerExceptions;
            int i = 1;
            int numOfInnerExceptions = innerExceptions.Count;
            foreach (Exception ex in innerExceptions)
            {
                _logger.Fatal<AppCrashLog>($"Exception {i} of {numOfInnerExceptions}.", ex);
                i++;
            }
        }
    }
}
