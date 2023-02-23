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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ProtonVPN.Core.MVVM;

namespace ProtonVPN.Validation
{
    public abstract class ValidationViewModel : ViewModel, INotifyDataErrorInfo
    {
        private readonly Dictionary<string, string> _errors = new();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public bool HasErrors => _errors.Any();

        public IEnumerable GetErrors(string propertyName)
        {
            return _errors.TryGetValue(propertyName, out string value) ? new[] { value } : null;
        }

        protected void SetError(string propertyName, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (_errors.TryGetValue(propertyName, out string oldValue) && value == oldValue)
                {
                    return;
                }

                _errors[propertyName] = value;
                RaiseErrorsChanged(propertyName);
            }
            else
            {
                if (_errors.Remove(propertyName))
                    RaiseErrorsChanged(propertyName);
            }
        }

        protected void ClearError(string propertyName)
        {
            if (_errors.ContainsKey(propertyName))
            {
                _errors.Remove(propertyName);
                RaiseErrorsChanged(propertyName);
            }
        }

        private void RaiseErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }
}