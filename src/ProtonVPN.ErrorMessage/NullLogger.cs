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
using ProtonVPN.Common.Logging;

namespace ProtonVPN.ErrorMessage
{
    internal class NullLogger : ILogger
    {
        public void Info(string message) { }

        public void Error(string message) { }

        public void Fatal(string message) { }

        public void Debug(string message) { }

        public void Trace(string message) { }

        public void Warn(string message) { }

        public void Error(Exception exception) { }

        public bool IsTraceEnabled()
        {
            return false;
        }
    }
}
