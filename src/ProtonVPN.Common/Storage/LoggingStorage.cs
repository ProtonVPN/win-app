/*
 * Copyright (c) 2022 Proton Technologies AG
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
using System.Collections;
using System.Linq;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Helpers;
using ProtonVPN.Common.Logging;

namespace ProtonVPN.Common.Storage
{
    public class LoggingStorage<T> : IStorage<T>, IThrowsExpectedExceptions
    {
        private readonly ILogger _logger;
        private readonly IStorage<T> _origin;

        public LoggingStorage(ILogger logger, IStorage<T> origin)
        {
            Ensure.NotNull(logger, nameof(logger));
            Ensure.NotNull(origin, nameof(origin));
            Ensure.IsTrue(origin is IThrowsExpectedExceptions, $"{nameof(origin)} must implement {nameof(IThrowsExpectedExceptions)} interface");

            _logger = logger;
            _origin = origin;
        }

        public T Get()
        {
            return _logger.Logged(
                () => _origin.Get(),
                $"Failed reading {NameOf(typeof(T))} from storage");
        }

        public void Set(T value)
        {
            _logger.Logged(
                () => _origin.Set(value),
                $"Failed writing {NameOf(typeof(T))} to storage");
        }

        public bool IsExpectedException(Exception ex)
        {
            return ex.IsExpectedExceptionOf(_origin);
        }

        private static string NameOf(Type type)
        {
            if (IsEnumerableType(type) && type.GetGenericArguments().Any())
            {
                return $"{type.GetGenericArguments()[0].Name} collection";
            }

            return type.Name;
        }

        private static bool IsEnumerableType(Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string);
        }
    }
}
