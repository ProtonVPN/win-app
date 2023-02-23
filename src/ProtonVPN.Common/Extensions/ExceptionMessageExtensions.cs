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
using System.IO;
using System.Linq;
using System.Security;

namespace ProtonVPN.Common.Extensions
{
    public static class ExceptionMessageExtensions
    {
        public static string CombinedMessage(this Exception exception)
        {
            return string.Join(" ---> ", ThisAndInnerExceptions(exception).Select(ex => ex.Message));
        }

        public static bool IsRegistryAccessException(this Exception ex)
        {
            return ex is SecurityException ||
                   ex is UnauthorizedAccessException ||
                   ex is IOException;
        }

        private static IEnumerable<Exception> ThisAndInnerExceptions(Exception e)
        {
            for (; e != null; e = e.InnerException)
            {
                yield return e;
            }
        }
    }
}
