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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ProtonVPN.Gui.Views.Controls;

public sealed partial class UserLocationControl : UserControl
{
    private const char SCRAMBLING_CHAR = '*';

    private const int SCRAMBLING_TIMER_INTERVAL_IN_MS = 50;

    public static readonly DependencyProperty CountryProperty =
        DependencyProperty.Register(nameof(Country), typeof(string), typeof(UserLocationControl), new PropertyMetadata(default, OnCountryPropertyChanged));

    public static readonly DependencyProperty FormattedCountryProperty =
        DependencyProperty.Register(nameof(FormattedCountry), typeof(string), typeof(UserLocationControl), new PropertyMetadata(default));

    public static readonly DependencyProperty FormattedIpAddressProperty =
        DependencyProperty.Register(nameof(FormattedIpAddress), typeof(string), typeof(UserLocationControl), new PropertyMetadata(default));

    public static readonly DependencyProperty IpAddressProperty =
        DependencyProperty.Register(nameof(IpAddress), typeof(string), typeof(UserLocationControl), new PropertyMetadata(default, OnIpAddressPropertyChanged));

    public static readonly DependencyProperty IsLocationHiddenProperty =
        DependencyProperty.Register(nameof(IsLocationHidden), typeof(bool), typeof(UserLocationControl), new PropertyMetadata(default, OnIsLocationHiddenPropertyChanged));

    public static readonly DependencyProperty IsLocationVisibleProperty =
        DependencyProperty.Register(nameof(IsLocationVisible), typeof(bool), typeof(UserLocationControl), new PropertyMetadata(default, OnIsLocationVisiblePropertyChanged));

    public static readonly DependencyProperty IsScramblingProperty =
        DependencyProperty.Register(nameof(IsScrambling), typeof(bool), typeof(UserLocationControl), new PropertyMetadata(default, OnIsScramblingPropertyChanged));

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

    public string FormattedCountry
    {
        get => (string)GetValue(FormattedCountryProperty);
        set => SetValue(FormattedCountryProperty, value);
    }

    public string FormattedIpAddress
    {
        get => (string)GetValue(FormattedIpAddressProperty);
        set => SetValue(FormattedIpAddressProperty, value);
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
            ulc.FormattedCountry = ulc.Country ?? string.Empty;
        }
    }

    private static void OnIpAddressPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UserLocationControl ulc)
        {
            ulc.FormattedIpAddress = ulc.IpAddress ?? string.Empty;
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
        FormattedCountry = new string(SCRAMBLING_CHAR, Country?.Length ?? 0);
        FormattedIpAddress = new string(SCRAMBLING_CHAR, IpAddress?.Length ?? 0);
    }

    private void OnScramblingTimerTick(object? sender, object e)
    {
        string userLocation = FormattedCountry + FormattedIpAddress;
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

        FormattedCountry = userLocation.Substring(0, FormattedCountry.Length);
        FormattedIpAddress = userLocation.Substring(FormattedCountry.Length, FormattedIpAddress.Length);
    }

    private void ResetScrambling()
    {
        FormattedCountry = Country;
        FormattedIpAddress = IpAddress;
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