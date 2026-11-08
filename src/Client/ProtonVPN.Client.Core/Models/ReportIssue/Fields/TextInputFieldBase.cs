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

using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ProtonVPN.Client.Core.Models.ReportIssue.Fields;

public abstract partial class TextInputFieldBase : IssueInputField<string>
{
    // Fields are mandatory unless the API explicitly sets a minimum length of 0
    private const int DEFAULT_MINIMUM_LENGTH = 1;

    protected abstract int DefaultMaximumLength { get; }

    [ObservableProperty]
    private int? _minimumLength;

    [ObservableProperty]
    private int? _maximumLength;

    public int EffectiveMinimumLength => MinimumLength ?? DEFAULT_MINIMUM_LENGTH;

    public int EffectiveMaximumLength => MaximumLength ?? Math.Max(DefaultMaximumLength, EffectiveMinimumLength);

    // 0 = optional, 1 = mandatory, 2 or more = mandatory with a minimum length limitation
    public bool IsMandatory => EffectiveMinimumLength > 0;

    public bool HasLengthConstraint => HasMinimumLength || HasMaximumLength;

    public bool HasMinimumLength => EffectiveMinimumLength > 1;

    public bool HasMaximumLength => EffectiveMaximumLength > 0;

    public int CurrentLength => Value?.Length ?? 0;

    public string FormattedCurrentLength => $"{CurrentLength}/{EffectiveMaximumLength}";

    public bool IsMinimumLengthMet => !HasMinimumLength || CurrentLength >= EffectiveMinimumLength;

    public string LengthHint { get; set; } = string.Empty;

    // The value is validated on construction, before the constraints are set
    partial void OnMinimumLengthChanged(int? value)
    {
        ValidateProperty(Value, nameof(Value));
    }

    partial void OnMaximumLengthChanged(int? value)
    {
        ValidateProperty(Value, nameof(Value));
    }

    protected override string GetSerializedValue()
    {
        return Value ?? string.Empty;
    }

    protected override void OnValueChanged()
    {
        base.OnValueChanged();

        OnPropertyChanged(nameof(CurrentLength));
        OnPropertyChanged(nameof(FormattedCurrentLength));
        OnPropertyChanged(nameof(IsMinimumLengthMet));
    }

    protected override bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(Value))
        {
            return !IsMandatory;
        }

        int length = Value.Length;

        return (!HasMinimumLength || length >= EffectiveMinimumLength) &&
               (!HasMaximumLength || length <= EffectiveMaximumLength);
    }
}