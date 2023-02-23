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
using System.Runtime.CompilerServices;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization;

namespace ProtonVPN.ErrorHandling
{
    internal class NullLogger : ILogger
    {
        public IList<string> GetRecentLogs() => new List<string>();

        public void Debug<TEvent>(string message, Exception exception = null,
            [CallerFilePath] string sourceFilePath = "",
            [CallerMemberName] string sourceMemberName = "",
            [CallerLineNumber] int sourceLineNumber = 0) where TEvent : ILogEvent, new() { }

        public void Info<TEvent>(string message, Exception exception = null,
            [CallerFilePath] string sourceFilePath = "",
            [CallerMemberName] string sourceMemberName = "",
            [CallerLineNumber] int sourceLineNumber = 0) where TEvent : ILogEvent, new() { }

        public void Warn<TEvent>(string message, Exception exception = null,
            [CallerFilePath] string sourceFilePath = "",
            [CallerMemberName] string sourceMemberName = "",
            [CallerLineNumber] int sourceLineNumber = 0) where TEvent : ILogEvent, new() { }

        public void Error<TEvent>(string message, Exception exception = null,
            [CallerFilePath] string sourceFilePath = "",
            [CallerMemberName] string sourceMemberName = "",
            [CallerLineNumber] int sourceLineNumber = 0) where TEvent : ILogEvent, new() { }

        public void Fatal<TEvent>(string message, Exception exception = null,
            [CallerFilePath] string sourceFilePath = "",
            [CallerMemberName] string sourceMemberName = "",
            [CallerLineNumber] int sourceLineNumber = 0) where TEvent : ILogEvent, new() { }
    }
}