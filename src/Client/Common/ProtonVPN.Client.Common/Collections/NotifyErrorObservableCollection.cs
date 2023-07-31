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

using System.Collections.Specialized;
using System.ComponentModel;

namespace ProtonVPN.Client.Common.Collections;

public class NotifyErrorObservableCollection<T> : NotifyObservableCollection<T>
        where T : INotifyPropertyChanged, INotifyDataErrorInfo
{
    public NotifyErrorObservableCollection()
        : base()
    { }

    public NotifyErrorObservableCollection(IEnumerable<T> items)
        : base(items)
    { }

    public event EventHandler<DataErrorsChangedEventArgs>? ItemErrorsChanged;

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (T item in e.NewItems.OfType<T>())
            {
                item.ErrorsChanged += OnItemErrorsChanged;
            }
        }
        if (e.OldItems != null)
        {
            foreach (T item in e.OldItems.OfType<T>())
            {
                item.ErrorsChanged -= OnItemErrorsChanged;
            }
        }

        base.OnCollectionChanged(e);
    }

    protected virtual void OnItemErrorsChanged(DataErrorsChangedEventArgs e)
    {
    }

    private void OnItemErrorsChanged(object? sender, DataErrorsChangedEventArgs e)
    {
        ItemErrorsChanged?.Invoke(sender, e);

        OnItemErrorsChanged(e);
    }
}