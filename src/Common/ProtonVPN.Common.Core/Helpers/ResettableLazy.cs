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

namespace ProtonVPN.Common.Core.Helpers;

public class ResettableLazy<T>
{
    private readonly Func<T> _function;
    private readonly object _lock = new();

    private bool _isValueCreated;
    private T _value = default!;

    public ResettableLazy(Func<T> function)
    {
        _function = function;
    }

    public T Value
    {
        get
        {
            lock (_lock)
            {
                if (!_isValueCreated)
                {
                    _value = _function();
                    _isValueCreated = true;
                }

                return _value;
            }
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            _isValueCreated = false;
        }
    }
}