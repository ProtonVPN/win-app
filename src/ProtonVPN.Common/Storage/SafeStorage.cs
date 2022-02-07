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
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Helpers;

namespace ProtonVPN.Common.Storage
{
    public class SafeStorage<T> : IStorage<T>
    {
        private readonly IStorage<T> _origin;

        public SafeStorage(IStorage<T> origin)
        {
            Ensure.NotNull(origin, nameof(origin));
            Ensure.IsTrue(origin is IThrowsExpectedExceptions, $"{nameof(origin)} must implement {nameof(IThrowsExpectedExceptions)} interface");

            _origin = origin;
        }

        public T Get()
        {
            try
            {
                return _origin.Get();
            }
            catch (Exception ex) when (ex.IsExpectedExceptionOf(_origin))
            {
                return default;
            }
        }

        public void Set(T value)
        {
            try
            {
                _origin.Set(value);
            }
            catch (Exception ex) when (ex.IsExpectedExceptionOf(_origin))
            {
            }
        }
    }
}
