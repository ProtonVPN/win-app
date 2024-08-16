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

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.Enums;

namespace ProtonVPN.Client.Common.UI.Controls.Custom;

public class ComplexCountryFlag : Control
{
    private const string MAIN_FLAG_ONLY_VISUAL_STATE = "MainFlagOnly";
    private const string BACK_AND_MAIN_FLAGS_VISUAL_STATE = "BackAndMainFlags";
    private const string FRONT_AND_MAIN_FLAGS_VISUAL_STATE = "FrontAndMainFlags";

    public static readonly DependencyProperty MainFlagTypeProperty =
        DependencyProperty.Register(nameof(MainFlagType), typeof(FlagType), typeof(ComplexCountryFlag), new PropertyMetadata(FlagType.Fastest, OnMainFlagTypePropertyChanged));

    public static readonly DependencyProperty ExitCountryCodeProperty =
        DependencyProperty.Register(nameof(ExitCountryCode), typeof(string), typeof(ComplexCountryFlag), new PropertyMetadata(string.Empty, OnExitCountryCodePropertyChanged));

    public static readonly DependencyProperty EntryCountryCodeProperty =
        DependencyProperty.Register(nameof(EntryCountryCode), typeof(string), typeof(ComplexCountryFlag), new PropertyMetadata(string.Empty, OnEntryCountryCodePropertyChanged));

    public static readonly DependencyProperty IsSecureCoreProperty =
        DependencyProperty.Register(nameof(IsSecureCore), typeof(bool), typeof(ComplexCountryFlag), new PropertyMetadata(false, OnIsSecureCorePropertyChanged));

    public static readonly DependencyProperty IsCompactProperty =
        DependencyProperty.Register(nameof(IsCompact), typeof(bool), typeof(ComplexCountryFlag), new PropertyMetadata(false));

    public FlagType MainFlagType
    {
        get => (FlagType)GetValue(MainFlagTypeProperty);
        set => SetValue(MainFlagTypeProperty, value);
    }

    public string ExitCountryCode
    {
        get => (string)GetValue(ExitCountryCodeProperty);
        set => SetValue(ExitCountryCodeProperty, value);
    }

    public string EntryCountryCode
    {
        get => (string)GetValue(EntryCountryCodeProperty);
        set => SetValue(EntryCountryCodeProperty, value);
    }

    public bool IsSecureCore
    {
        get => (bool)GetValue(IsSecureCoreProperty);
        set => SetValue(IsSecureCoreProperty, value);
    }

    public bool IsCompact
    {
        get => (bool)GetValue(IsCompactProperty);
        set => SetValue(IsCompactProperty, value);
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        InvalidateFlagsLayoutVisualState();
    }

    private static void OnMainFlagTypePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ComplexCountryFlag control)
        {
            control.InvalidateFlagsLayoutVisualState();
        }
    }

    private static void OnExitCountryCodePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ComplexCountryFlag control)
        {
            control.InvalidateFlagsLayoutVisualState();
        }
    }

    private static void OnEntryCountryCodePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ComplexCountryFlag control)
        {
            control.InvalidateFlagsLayoutVisualState();
        }
    }

    private static void OnIsSecureCorePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ComplexCountryFlag control)
        {
            control.InvalidateFlagsLayoutVisualState();
        }
    }

    private void InvalidateFlagsLayoutVisualState()
    {
        bool isGenericFlag = MainFlagType != FlagType.Country;
        bool isExitCountryUnknown = string.IsNullOrEmpty(ExitCountryCode);
        bool isEntryCountryUnknown = string.IsNullOrEmpty(EntryCountryCode);

        string flagsLayoutVisualState = MAIN_FLAG_ONLY_VISUAL_STATE;

        if (isGenericFlag && !isExitCountryUnknown)
        {
            flagsLayoutVisualState = FRONT_AND_MAIN_FLAGS_VISUAL_STATE;
        }
        else if (IsSecureCore && !isEntryCountryUnknown)
        {
            flagsLayoutVisualState = BACK_AND_MAIN_FLAGS_VISUAL_STATE;
        }

        VisualStateManager.GoToState(this, flagsLayoutVisualState, false);
    }
}