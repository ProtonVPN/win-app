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
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

namespace ProtonVPN.Common.UI.Controls;

[TemplatePart(Name = "PART_ExitCountryFlag", Type = typeof(Image))]
[TemplatePart(Name = "PART_EntryCountryFlag", Type = typeof(Image))]
public sealed partial class CountryFlagControl
{
    private const string FLAG_ASSETS_FILE_EXTENSION = ".svg";
    private const string FLAG_ASSETS_FASTEST = "Fastest";
    private const string FLAG_ASSETS_PLACEHOLDER = "Placeholder";

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

    protected override async void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        InvalidateFlagsLayout();
        await UpdateCountryFlagAsync();
    }

    private static async void OnExitCountryCodePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CountryFlagControl control)
        {
            await control.UpdateCountryFlagAsync();
        }
    }

    private static async void OnEntryCountryCodePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CountryFlagControl control)
        {
            control.InvalidateFlagsLayout();
            await control.UpdateEntryCountryFlagAsync();
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

    private async Task UpdateCountryFlagAsync()
    {
        PART_ExitCountryFlag.Source = await GetImageSourceAsync(ExitCountryCode);
    }

    private async Task UpdateEntryCountryFlagAsync()
    {
        PART_EntryCountryFlag.Source = await GetImageSourceAsync(EntryCountryCode);
    }

    private async Task<SvgImageSource> GetImageSourceAsync(string countryCode)
    {
        if (string.IsNullOrEmpty(countryCode))
        {
            countryCode = FLAG_ASSETS_FASTEST;
        }
        else
        {
            countryCode = countryCode.ToUpperInvariant();
        }

        return await GetFlagResourceAsync(GetFlagResourceName(countryCode)) ??
               await GetFlagResourceAsync(GetFlagResourceName(FLAG_ASSETS_PLACEHOLDER));
    }

    private string GetFlagResourceName(string countryCode)
    {
        return $"ProtonVPN.Common.UI.Assets.Flags.{countryCode}{FLAG_ASSETS_FILE_EXTENSION}";
    }

    private async Task<SvgImageSource> GetFlagResourceAsync(string resourceName)
    {
        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);

        if (stream != null)
        {
            SvgImageSource svgImageSource = new();

            SvgImageSourceLoadStatus status = await svgImageSource.SetSourceAsync(stream.AsRandomAccessStream());
            if (status != SvgImageSourceLoadStatus.Success)
            {
                return null;
            }

            return svgImageSource;
        }

        return null;
    }
}