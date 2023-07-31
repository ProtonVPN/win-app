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

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ProtonVPN.Client.Common.Collections;

public class NotifyObservableCollection<T> : ObservableCollection<T>
        where T : INotifyPropertyChanged
{
    public NotifyObservableCollection()
    { }

    public NotifyObservableCollection(IEnumerable<T> items)
        : this()
    {
        foreach (T item in items)
        {
            Add(item);
        }
    }

    public event PropertyChangedEventHandler? ItemPropertyChanged;

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (T item in e.NewItems.OfType<T>())
            {
                item.PropertyChanged += OnItemPropertyChanged;
            }
        }
        if (e.OldItems != null)
        {
            foreach (T item in e.OldItems.OfType<T>())
            {
                item.PropertyChanged -= OnItemPropertyChanged;
            }
        }

        base.OnCollectionChanged(e);
    }

    protected virtual void OnItemPropertyChanged(PropertyChangedEventArgs e)
    {
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        ItemPropertyChanged?.Invoke(sender, e);

        OnItemPropertyChanged(e);
    }
}