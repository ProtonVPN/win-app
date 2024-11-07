/*
 * Copyright (c) 2024 Proton AG
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

namespace ProtonVPN.Common.Core.Queues;

public class FixedSizeQueue<T> : Queue<T>
{
    private readonly int _maxSize;
    private readonly T _defaultValue;

    public FixedSizeQueue(int maxSize, T defaultValue)
    {
        _maxSize = maxSize;
        _defaultValue = defaultValue;

        FillInitialValues();
    }

    private void FillInitialValues()
    {
        for (int i = 0; i < _maxSize; i++)
        {
            Enqueue(_defaultValue);
        }
    }

    public new void Enqueue(T item)
    {
        for (int i = Count; i >= _maxSize; i--)
        {
            Dequeue();
        }

        base.Enqueue(item);
    }

    public void Reset()
    {
        Clear();
        FillInitialValues();
    }
}