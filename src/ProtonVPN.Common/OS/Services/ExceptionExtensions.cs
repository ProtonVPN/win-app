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
using System.ComponentModel;

namespace ProtonVPN.Common.OS.Services
{
    public static class ExceptionExtensions
    {
        private const int ServiceAlreadyRunning = 1056;
        private const int ServiceNotRunning = 1062;

        public static bool IsServiceAccessException(this Exception ex) =>
            ex is InvalidOperationException ||
            ex is System.ServiceProcess.TimeoutException ||
            ex is TimeoutException;

        public static bool IsServiceAlreadyRunning(this InvalidOperationException e) =>
            e.InnerException is Win32Exception ex &&
            ex.NativeErrorCode == ServiceAlreadyRunning;

        public static bool IsServiceNotRunning(this InvalidOperationException e) =>
            e.InnerException is Win32Exception ex &&
            ex.NativeErrorCode == ServiceNotRunning;
    }
}
