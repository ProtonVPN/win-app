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

using System.ComponentModel.DataAnnotations;

namespace ProtonVPN.Client.Common.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class RequiredIfAttribute : RequiredAttribute
{
    public string PropertyName { get; }

    public RequiredIfAttribute(string propertyName)
    {
        PropertyName = propertyName;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        object instance = validationContext.ObjectInstance;

        bool propertyValue = Convert.ToBoolean(instance.GetType().GetProperty(PropertyName)?.GetValue(instance));

        return propertyValue
            ? base.IsValid(value, validationContext)
            : ValidationResult.Success;
    }
}