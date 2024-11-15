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

using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using ProtonVPN.Client.Common.Attributes;

namespace ProtonVPN.Client.Core.Models.ReportIssue;

public abstract class IssueInputField : ObservableValidator
{
    public bool IsMandatory { get; }

    public string Name { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string Placeholder { get; set; } = string.Empty;

    public bool HasPlaceholder => !string.IsNullOrEmpty(Placeholder);

    protected IssueInputField(bool isFieldMandatory)
    {
        IsMandatory = isFieldMandatory;
    }

    public static ValidationResult? ValidateFieldValue(object? value, ValidationContext context)
    {
        IssueInputField instance = (IssueInputField)context.ObjectInstance;

        return instance.IsValid()
            ? ValidationResult.Success
            : new(string.Empty);
    }

    public (string key, string value) Serialize()
    {
        return (Key, GetSerializedValue());
    }

    protected abstract bool IsValid();

    protected abstract string GetSerializedValue();
}

public abstract partial class IssueInputField<TValue> : IssueInputField
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [RequiredIf(nameof(IsMandatory))]
    [CustomValidation(typeof(IssueInputField), nameof(ValidateFieldValue))]
    private TValue? _value;

    public IssueInputField(bool isFieldMandatory)
        : base(isFieldMandatory)
    {
        ValidateProperty(Value, nameof(Value));
    }

    protected override bool IsValid()
    {
        return true;
    }
}