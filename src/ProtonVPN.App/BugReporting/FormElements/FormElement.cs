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

using ProtonVPN.Common.Extensions;
using ProtonVPN.Core.MVVM;

namespace ProtonVPN.BugReporting.FormElements
{
    public abstract class FormElement : ViewModel
    {
        private string _value;
        public string Value
        {
            get => _value;
            set => Set(ref _value, value);
        }

        public string Label { get; set; }
        public string SubmitLabel { get; set; }
        public string Placeholder { get; set; }
        public bool IsMandatory { get; set; }

        public virtual bool IsValid()
        {
            return !IsMandatory || !Value.IsNullOrEmpty() && !Value.Trim().IsNullOrEmpty();
        }
    }
}