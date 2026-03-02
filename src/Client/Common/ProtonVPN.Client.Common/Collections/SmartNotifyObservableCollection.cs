/*
 * Copyright (c) 2026 Proton AG
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

public class SmartNotifyObservableCollection<T> : ObservableCollection<T>
        where T : INotifyPropertyChanged
{
    public SmartNotifyObservableCollection()
        : base()
    { }

    public SmartNotifyObservableCollection(IEnumerable<T> collection)
        : base(collection)
    { }

    public SmartNotifyObservableCollection(List<T> list)
        : base(list)
    { }

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

    public void AddRange(IEnumerable<T> range)
    {
        if (range != null)
        {
            foreach (T? item in range)
            {
                Items.Add(item);
                if (item != null)
                {
                    item.PropertyChanged += OnItemPropertyChanged;
                }
            }
        }

        OnPropertyChanged(new PropertyChangedEventArgs("Count"));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public void Reset(IEnumerable<T> range)
    {
        foreach (T item in Items.OfType<T>())
        {
            item.PropertyChanged -= OnItemPropertyChanged;
        }

        Items.Clear();

        AddRange(range);
    }
}