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

using ProtonVPN.Core.MVVM;
using System;

namespace ProtonVPN.Settings
{
    public class SelectableItemWrapper<T>: ViewModel, IEquatable<SelectableItemWrapper<T>>
    {
        private readonly Func<T, bool> _getSelected;
        private readonly Action<T, bool> _setSelected;

        private bool _selected;
        public virtual bool Selected
        {
            get
            {
                if (_getSelected != null)
                    _selected = _getSelected(Item);

                return _selected;
            }
            set
            {
                _setSelected?.Invoke(Item, value);
                Set(ref _selected, value);
            }
        }

        public T Item { get; }

        public SelectableItemWrapper(T item) : this(item, null, null)
        {
        }

        public SelectableItemWrapper(T item, Func<T, bool> getSelected, Action<T, bool> setSelected)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            Item = item;
            _getSelected = getSelected;
            _setSelected = setSelected;
        }

        #region IEquatable

        public bool Equals(SelectableItemWrapper<T> other)
        {
            if (other == null)
                return false;

            return Item.Equals(other.Item);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SelectableItemWrapper<T>);
        }

        public override int GetHashCode()
        {
            return Item.GetHashCode();
        }

        #endregion
    }
}
