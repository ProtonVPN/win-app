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
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Documents;

namespace ProtonVPN.Client.Common.UI.Controls;

[TemplatePart(Name = "PART_ScrambledCountry", Type = typeof(Run))]
[TemplatePart(Name = "PART_ScrambledIpAddress", Type = typeof(Run))]
public sealed partial class UserLocationControl
{
    public static readonly DependencyProperty CountryProperty =
        DependencyProperty.Register(nameof(Country), typeof(string), typeof(UserLocationControl), new PropertyMetadata(default, OnCountryPropertyChanged));

    public static readonly DependencyProperty IpAddressProperty =
        DependencyProperty.Register(nameof(IpAddress), typeof(string), typeof(UserLocationControl), new PropertyMetadata(default, OnIpAddressPropertyChanged));

    public static readonly DependencyProperty IsLocationHiddenProperty =
        DependencyProperty.Register(nameof(IsLocationHidden), typeof(bool), typeof(UserLocationControl), new PropertyMetadata(default, OnIsLocationHiddenPropertyChanged));

    public static readonly DependencyProperty IsLocationVisibleProperty =
        DependencyProperty.Register(nameof(IsLocationVisible), typeof(bool), typeof(UserLocationControl), new PropertyMetadata(default, OnIsLocationVisiblePropertyChanged));

    public static readonly DependencyProperty IsScramblingProperty =
        DependencyProperty.Register(nameof(IsScrambling), typeof(bool), typeof(UserLocationControl), new PropertyMetadata(default, OnIsScramblingPropertyChanged));

    private const char SCRAMBLING_CHAR = '*';

    private const int SCRAMBLING_TIMER_INTERVAL_IN_MS = 50;
    private readonly Random _random = new();

    private readonly DispatcherTimer _scramblingTimer = new();

    public UserLocationControl()
    {
        InitializeComponent();

        _scramblingTimer = new()
        {
            Interval = TimeSpan.FromMilliseconds(SCRAMBLING_TIMER_INTERVAL_IN_MS)
        };
        _scramblingTimer.Tick += OnScramblingTimerTick;
    }

    public string Country
    {
        get => (string)GetValue(CountryProperty);
        set => SetValue(CountryProperty, value);
    }

    public string IpAddress
    {
        get => (string)GetValue(IpAddressProperty);
        set => SetValue(IpAddressProperty, value);
    }

    public bool IsLocationHidden
    {
        get => (bool)GetValue(IsLocationHiddenProperty);
        set => SetValue(IsLocationHiddenProperty, value);
    }

    public bool IsLocationVisible
    {
        get => (bool)GetValue(IsLocationVisibleProperty);
        set => SetValue(IsLocationVisibleProperty, value);
    }

    public bool IsScrambling
    {
        get => (bool)GetValue(IsScramblingProperty);
        set => SetValue(IsScramblingProperty, value);
    }

    private static void OnCountryPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UserLocationControl ulc)
        {
            ulc.PART_ScrambledCountry.Text = ulc.Country ?? string.Empty;
        }
    }

    private static void OnIpAddressPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UserLocationControl ulc)
        {
            ulc.PART_ScrambledIpAddress.Text = ulc.IpAddress ?? string.Empty;
        }
    }

    private static void OnIsLocationHiddenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UserLocationControl ulc && ulc.IsLocationHidden)
        {
            ulc.StopScrambling();
            ulc.CompleteScrambling();
        }
    }

    private static void OnIsLocationVisiblePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UserLocationControl ulc && ulc.IsLocationVisible)
        {
            ulc.StopScrambling();
            ulc.ResetScrambling();
        }
    }

    private static void OnIsScramblingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UserLocationControl ulc)
        {
            if (ulc.IsScrambling)
            {
                ulc.StartScrambling();
                return;
            }
            ulc.StopScrambling();
        }
    }

    private void CompleteScrambling()
    {
        PART_ScrambledCountry.Text = new string(SCRAMBLING_CHAR, Country?.Length ?? 0);
        PART_ScrambledIpAddress.Text = new string(SCRAMBLING_CHAR, IpAddress?.Length ?? 0);
    }

    private void OnScramblingTimerTick(object? sender, object e)
    {
        string userLocation = PART_ScrambledCountry.Text + PART_ScrambledIpAddress.Text;
        if (string.IsNullOrEmpty(userLocation) || userLocation.All(c => c == SCRAMBLING_CHAR))
        {
            // Scrambling process complete
            StopScrambling();
            return;
        }

        int index;
        do
        {
            index = _random.Next(userLocation.Length);
        } while (userLocation.ElementAt(index) == SCRAMBLING_CHAR);

        userLocation = $"{userLocation.Remove(index)}{SCRAMBLING_CHAR}{userLocation.Remove(0, index + 1)}";

        PART_ScrambledCountry.Text = userLocation.Substring(0, Country.Length);
        PART_ScrambledIpAddress.Text = userLocation.Substring(Country.Length, IpAddress.Length);
    }

    private void ResetScrambling()
    {
        PART_ScrambledCountry.Text = Country;
        PART_ScrambledIpAddress.Text = IpAddress;
    }

    private void StartScrambling()
    {
        if (!_scramblingTimer.IsEnabled)
        {
            _scramblingTimer.Start();
        }
    }

    private void StopScrambling()
    {
        if (_scramblingTimer.IsEnabled)
        {
            _scramblingTimer.Stop();
        }
    }
}