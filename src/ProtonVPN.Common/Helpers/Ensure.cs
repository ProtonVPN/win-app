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
using System.Linq;

namespace ProtonVPN.Common.Helpers
{
    public static class Ensure
    {
        public static void NotEmpty(string arg, string argName, string message = null)
        {
            if (!string.IsNullOrEmpty(arg))
            {
                return;
            }

            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentException($"ArgumentException: {argName} string not valid.");
            }

            throw new ArgumentException($"{message}");
        }

        public static void NotEmpty<T>(IEnumerable<T> arg, string argName)
        {
            if (arg == null || !arg.Any())
            {
                throw new ArgumentException($"ArgumentException: sequence {argName} is empty or null");
            }
        }

        public static void NotNull<T>(T arg, string argName)
            where T : class
        {
            if (arg == null)
            {
                throw new ArgumentNullException($"ArgumentException: {argName} is null");
            }
        }

        public static void IsTrue(bool condition, string message = "")
        {
            if (!condition)
            {
                throw new ArgumentException($"Condition not satisfied: {message}");
            }
        }

        public static void IsFalse(bool condition, string message = "")
        {
            if (condition)
            {
                throw new ArgumentException($"Condition not satisfied: {message}");
            }
        }
    }
}
