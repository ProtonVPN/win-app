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

namespace ProtonVPN.Client.Common.UI.Controls;

[TemplatePart(Name = "PART_ExitCountryFlag", Type = typeof(Image))]
[TemplatePart(Name = "PART_EntryCountryFlag", Type = typeof(Image))]
public sealed partial class CountryFlagControl
{
    private const string ASSETS_FOLDER_PATH = "ms-appx:///ProtonVPN.Client.Common.UI/Assets/Flags/";
    private const string ASSETS_FILE_EXTENSION = ".svg";
    private const string ASSETS_FASTEST_COUNTRY = "Fastest";
    private const string ASSETS_PLACEHOLDER = "Placeholder"; 

    private const string GATEWAY_OFF_STATE = "NonGateway";
    private const string GATEWAY_UNKNOWN_COUNTRY_STATE = "GatewayUnknown";
    private const string GATEWAY_KNOWN_COUNTRY_STATE = "Gateway";

    private const string SECURE_CORE_OFF_STATE = "NonSecureCore";
    private const string SECURE_CORE_UNKNOWN_COUNTRY_STATE = "SecureCoreUnknown";
    private const string SECURE_CORE_KNOWN_COUNTRY_STATE = "SecureCore";

    private const string FLAG_SINGLE_STATE = "SingleFlagMode";
    private const string FLAG_DUAL_STATE = "DualFlagsStandardMode";
    private const string FLAG_DUAL_COMPACT_STATE = "DualFlagsCompactMode";
    private const string FLAG_GATEWAY_STATE = "GatewayStandardMode";
    private const string FLAG_GATEWAY_COMPACT_STATE = "GatewayCompactMode";

    public static readonly DependencyProperty ExitCountryCodeProperty =
        DependencyProperty.Register(nameof(ExitCountryCode), typeof(string), typeof(CountryFlagControl), new PropertyMetadata(default, OnExitCountryCodePropertyChanged));

    public static readonly DependencyProperty EntryCountryCodeProperty =
        DependencyProperty.Register(nameof(EntryCountryCode), typeof(string), typeof(CountryFlagControl), new PropertyMetadata(default, OnEntryCountryCodePropertyChanged));

    public static readonly DependencyProperty IsSecureCoreProperty =
        DependencyProperty.Register(nameof(IsSecureCore), typeof(bool), typeof(CountryFlagControl), new PropertyMetadata(default, OnIsSecureCorePropertyChanged));

    public static readonly DependencyProperty IsCompactProperty =
        DependencyProperty.Register(nameof(IsCompact), typeof(bool), typeof(CountryFlagControl), new PropertyMetadata(default, OnIsCompactPropertyChanged));

    public static readonly DependencyProperty IsGatewayProperty =
        DependencyProperty.Register(nameof(IsGateway), typeof(bool), typeof(CountryFlagControl), new PropertyMetadata(default, OnIsGatewayPropertyChanged));

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

    public bool IsGateway
    {
        get => (bool)GetValue(IsGatewayProperty);
        set => SetValue(IsGatewayProperty, value);
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
            control.InvalidateFlagsLayout();
        }
    }

    private static void OnEntryCountryCodePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CountryFlagControl control)
        {
            control.UpdateEntryCountryFlag();
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

    private static void OnIsGatewayPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CountryFlagControl control)
        {
            control.InvalidateFlagsLayout();
        }
    }

    private void InvalidateFlagsLayout()
    {
        bool isExitCountryUnknown = string.IsNullOrEmpty(ExitCountryCode);
        bool isEntryCountryUnknown = string.IsNullOrEmpty(EntryCountryCode);

        string gatewayVisualState = !IsGateway
            ? GATEWAY_OFF_STATE
            : isExitCountryUnknown
                ? GATEWAY_UNKNOWN_COUNTRY_STATE
                : GATEWAY_KNOWN_COUNTRY_STATE;

        string secureCoreVisualState = !IsSecureCore || IsGateway
            ? SECURE_CORE_OFF_STATE
            : isEntryCountryUnknown
                ? SECURE_CORE_UNKNOWN_COUNTRY_STATE
                : SECURE_CORE_KNOWN_COUNTRY_STATE;

        bool isSingleFlag = gatewayVisualState != GATEWAY_KNOWN_COUNTRY_STATE
                         && secureCoreVisualState != SECURE_CORE_KNOWN_COUNTRY_STATE;

        string flagLayoutVisualState = isSingleFlag
            ? FLAG_SINGLE_STATE
            : IsCompact
                ? IsGateway
                    ? FLAG_GATEWAY_COMPACT_STATE
                    : FLAG_DUAL_COMPACT_STATE
                : IsGateway
                    ? FLAG_GATEWAY_STATE
                    : FLAG_DUAL_STATE;

        VisualStateManager.GoToState(this, secureCoreVisualState, false);
        VisualStateManager.GoToState(this, flagLayoutVisualState, false);
        VisualStateManager.GoToState(this, gatewayVisualState, false);
    }

    private void UpdateCountryFlag()
    {
        PART_ExitCountryFlag.Source = GetImageSource(ExitCountryCode);
    }

    private void UpdateEntryCountryFlag()
    {
        PART_EntryCountryFlag.Source = GetImageSource(EntryCountryCode);
    }

    private SvgImageSource GetImageSource(string countryCode)
    {
        if (string.IsNullOrEmpty(countryCode))
        {
            countryCode = ASSETS_FASTEST_COUNTRY;
        }

        Uri uri = BuildUri(countryCode);

        if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, uri.LocalPath.TrimStart('/', '\\'))))
        {
            uri = BuildUri(ASSETS_PLACEHOLDER);
        }

        return new SvgImageSource(uri);
    }

    private Uri BuildUri(string resourceName)
    {
        return new(Path.Combine(ASSETS_FOLDER_PATH, resourceName + ASSETS_FILE_EXTENSION));
    }
}