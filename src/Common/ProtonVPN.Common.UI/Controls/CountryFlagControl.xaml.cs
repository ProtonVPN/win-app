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
using System.IO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

namespace ProtonVPN.Common.UI.Controls;

[TemplatePart(Name = "PART_ExitCountryFlag", Type = typeof(Image))]
[TemplatePart(Name = "PART_MiddleCountryFlag", Type = typeof(Image))]
public sealed partial class CountryFlagControl
{
    private const string FLAG_ASSETS_FOLDER_PATH = "ms-appx:///ProtonVPN.Common.UI/Assets/Flags/";
    private const string FLAG_ASSETS_FILE_EXTENSION = ".svg";
    private const string FLAG_ASSETS_FASTEST = "Fastest";
    private const string FLAG_ASSETS_PLACEHOLDER = "Placeholder";

    public static readonly DependencyProperty CountryCodeProperty =
        DependencyProperty.Register(nameof(CountryCode), typeof(string), typeof(CountryFlagControl), new PropertyMetadata(default, OnCountryCodePropertyChanged));

    public static readonly DependencyProperty MiddleCountryCodeProperty =
        DependencyProperty.Register(nameof(MiddleCountryCode), typeof(string), typeof(CountryFlagControl), new PropertyMetadata(default, OnMiddleCountryCodePropertyChanged));

    public static readonly DependencyProperty IsSecureCoreProperty =
        DependencyProperty.Register(nameof(IsSecureCore), typeof(bool), typeof(CountryFlagControl), new PropertyMetadata(default, OnIsSecureCorePropertyChanged));

    public static readonly DependencyProperty IsCompactProperty =
        DependencyProperty.Register(nameof(IsCompact), typeof(bool), typeof(CountryFlagControl), new PropertyMetadata(default, OnIsCompactPropertyChanged));

    public CountryFlagControl()
    {
        InitializeComponent();
    }

    public string CountryCode
    {
        get => (string)GetValue(CountryCodeProperty);
        set => SetValue(CountryCodeProperty, value);
    }

    public string MiddleCountryCode
    {
        get => (string)GetValue(MiddleCountryCodeProperty);
        set => SetValue(MiddleCountryCodeProperty, value);
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

    private static void OnCountryCodePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CountryFlagControl control)
        {
            control.UpdateCountryFlag();
        }
    }

    private static void OnMiddleCountryCodePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CountryFlagControl control)
        {
            control.UpdateMiddleCountryFlag();
            control.InvalidateFlagsLayout();
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
        bool isMiddleCountryUnknown = string.IsNullOrEmpty(MiddleCountryCode);

        string secureCoreVisualState = !IsSecureCore
            ? "NonSecureCore"
            : isMiddleCountryUnknown
                ? "SecureCoreUnknown"
                : "SecureCore";

        string flagLayoutVisualState = !IsSecureCore || isMiddleCountryUnknown
            ? "SingleFlagMode"
            : IsCompact
                ? "DualFlagsCompactMode"
                : "DualFlagsStandardMode";

        VisualStateManager.GoToState(this, secureCoreVisualState, false);
        VisualStateManager.GoToState(this, flagLayoutVisualState, false);
    }

    private void UpdateCountryFlag()
    {
        PART_ExitCountryFlag.Source = GetImageSource(CountryCode);
    }

    private void UpdateMiddleCountryFlag()
    {
        PART_MiddleCountryFlag.Source = GetImageSource(MiddleCountryCode);
    }

    private SvgImageSource GetImageSource(string countryCode)
    {
        if (string.IsNullOrEmpty(countryCode))
        {
            countryCode = FLAG_ASSETS_FASTEST;
        }

        Uri uri = BuildUri(countryCode);

        if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + uri.LocalPath))
        {
            uri = BuildUri(FLAG_ASSETS_PLACEHOLDER);
        }

        return new SvgImageSource(uri);
    }

    private Uri BuildUri(string resourceName)
    {
        return new(Path.Combine(FLAG_ASSETS_FOLDER_PATH, resourceName + FLAG_ASSETS_FILE_EXTENSION));
    }
}