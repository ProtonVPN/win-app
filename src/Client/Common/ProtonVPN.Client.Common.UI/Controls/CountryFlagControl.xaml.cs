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
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

namespace ProtonVPN.Client.Common.UI.Controls;

[TemplatePart(Name = "PART_ExitCountryFlag", Type = typeof(Image))]
[TemplatePart(Name = "PART_EntryCountryFlag", Type = typeof(Image))]
public sealed partial class CountryFlagControl
{
    private const string FASTEST_COUNTRY = "Fastest";

    public static readonly DependencyProperty ExitCountryCodeProperty =
        DependencyProperty.Register(nameof(ExitCountryCode), typeof(string), typeof(CountryFlagControl), new PropertyMetadata(default, OnExitCountryCodePropertyChanged));

    public static readonly DependencyProperty EntryCountryCodeProperty =
        DependencyProperty.Register(nameof(EntryCountryCode), typeof(string), typeof(CountryFlagControl), new PropertyMetadata(default, OnEntryCountryCodePropertyChanged));

    public static readonly DependencyProperty IsSecureCoreProperty =
        DependencyProperty.Register(nameof(IsSecureCore), typeof(bool), typeof(CountryFlagControl), new PropertyMetadata(default, OnIsSecureCorePropertyChanged));

    public static readonly DependencyProperty IsCompactProperty =
        DependencyProperty.Register(nameof(IsCompact), typeof(bool), typeof(CountryFlagControl), new PropertyMetadata(default, OnIsCompactPropertyChanged));

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

    public CountryFlagControl()
    {
        InitializeComponent();
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        InvalidateFlagsLayout();
        UpdateCountryFlag();
    }

    private static void OnExitCountryCodePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CountryFlagControl control)
        {
            control.UpdateCountryFlag();
        }
    }

    private static void OnEntryCountryCodePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CountryFlagControl control)
        {
            control.InvalidateFlagsLayout();
            control.UpdateEntryCountryFlag();
        }
    }

    private static void OnIsSecureCorePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CountryFlagControl control)
        {
            control.InvalidateFlagsLayout();
        }
    }

    private static void OnIsCompactPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CountryFlagControl control)
        {
            control.InvalidateFlagsLayout();
        }
    }

    private void InvalidateFlagsLayout()
    {
        bool isEntryCountryUnknown = string.IsNullOrEmpty(EntryCountryCode);

        string secureCoreVisualState = !IsSecureCore
            ? "NonSecureCore"
            : isEntryCountryUnknown
                ? "SecureCoreUnknown"
                : "SecureCore";

        string flagLayoutVisualState = !IsSecureCore || isEntryCountryUnknown
            ? "SingleFlagMode"
            : IsCompact
                ? "DualFlagsCompactMode"
                : "DualFlagsStandardMode";

        VisualStateManager.GoToState(this, secureCoreVisualState, false);
        VisualStateManager.GoToState(this, flagLayoutVisualState, false);
    }

    private void UpdateCountryFlag()
    {
        PART_ExitCountryFlag.Source = GetImageSource(ExitCountryCode);
    }

    private void UpdateEntryCountryFlag()
    {
        PART_EntryCountryFlag.Source = GetImageSource(EntryCountryCode);
    }

    private SvgImageSource GetImageSource(string? countryCode)
    {
        if (string.IsNullOrEmpty(countryCode))
        {
            countryCode = FASTEST_COUNTRY;
        }

        return new SvgImageSource(
            new Uri($"ms-appx:///ProtonVPN.Client.Common.UI/Assets/Flags/{countryCode}.svg"));
    }
}