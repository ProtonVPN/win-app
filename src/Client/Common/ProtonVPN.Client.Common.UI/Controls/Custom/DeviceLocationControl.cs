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
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ProtonVPN.Client.Common.UI.Controls.Custom;

[TemplatePart(Name = "PART_ScrambledCountry", Type = typeof(TextBlock))]
[TemplatePart(Name = "PART_ScrambledIpAddress", Type = typeof(TextBlock))]
[TemplatePart(Name = "PART_ScrambledIsp", Type = typeof(TextBlock))]
public class DeviceLocationControl : ContentControl
{
    private const char SCRAMBLING_CHAR = '*';
    private const int SCRAMBLING_TIMER_INTERVAL_IN_MS = 50;

    private readonly Random _random = new();
    private readonly DispatcherTimer _scramblingTimer;

    public static readonly DependencyProperty ProtectionLabelProperty =
        DependencyProperty.Register(
            nameof(ProtectionLabel),
            typeof(string),
            typeof(DeviceLocationControl),
            new PropertyMetadata(default));

    public static readonly DependencyProperty ProtectionSubLabelProperty =
        DependencyProperty.Register(
            nameof(ProtectionSubLabel),
            typeof(string),
            typeof(DeviceLocationControl),
            new PropertyMetadata(default));

    public static readonly DependencyProperty CountryProperty =
        DependencyProperty.Register(
            nameof(Country),
            typeof(string),
            typeof(DeviceLocationControl),
            new PropertyMetadata(default));

    public static readonly DependencyProperty CountryLabelProperty =
        DependencyProperty.Register(
            nameof(CountryLabel),
            typeof(string),
            typeof(DeviceLocationControl),
            new PropertyMetadata(default));

    public static readonly DependencyProperty IpAddressProperty =
        DependencyProperty.Register(
            nameof(IpAddress),
            typeof(string),
            typeof(DeviceLocationControl),
            new PropertyMetadata(default));

    public static readonly DependencyProperty IspProperty =
        DependencyProperty.Register(
            nameof(Isp),
            typeof(string),
            typeof(DeviceLocationControl),
            new PropertyMetadata(default));

    public static readonly DependencyProperty IpAddressLabelProperty =
        DependencyProperty.Register(
            nameof(IpAddressLabel),
            typeof(string),
            typeof(DeviceLocationControl),
            new PropertyMetadata(default));

    public static readonly DependencyProperty IspLabelProperty =
        DependencyProperty.Register(
            nameof(IspLabel),
            typeof(string),
            typeof(DeviceLocationControl),
            new PropertyMetadata(default));

    public static readonly DependencyProperty IsConnectedProperty =
        DependencyProperty.Register(
            nameof(IsConnected),
            typeof(bool),
            typeof(DeviceLocationControl),
            new PropertyMetadata(default, OnIsConnectedPropertyChanged));

    public static readonly DependencyProperty IsDisconnectedProperty =
        DependencyProperty.Register(
            nameof(IsDisconnected),
            typeof(bool),
            typeof(DeviceLocationControl),
            new PropertyMetadata(default, OnIsDisconnectedPropertyChanged));

    public static readonly DependencyProperty IsConnectingProperty =
        DependencyProperty.Register(
            nameof(IsConnecting),
            typeof(bool),
            typeof(DeviceLocationControl),
            new PropertyMetadata(default, OnIsConnectingPropertyChanged));

    protected TextBlock PART_ScrambledCountry;
    protected TextBlock PART_ScrambledIpAddress;
    protected TextBlock PART_ScrambledIsp;

    public DeviceLocationControl()
    {
        DefaultStyleKey = typeof(DeviceLocationControl);

        _scramblingTimer = new() { Interval = TimeSpan.FromMilliseconds(SCRAMBLING_TIMER_INTERVAL_IN_MS) };
        _scramblingTimer.Tick += OnScramblingTimerTick;

        Loaded += DeviceLocationControl_Loaded;
    }

    private void DeviceLocationControl_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateVisualStates();
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        PART_ScrambledCountry = GetTemplateChild("PART_ScrambledCountry") as TextBlock;
        PART_ScrambledIpAddress = GetTemplateChild("PART_ScrambledIpAddress") as TextBlock;
        PART_ScrambledIsp = GetTemplateChild("PART_ScrambledIsp") as TextBlock;

        if (PART_ScrambledCountry is not null)
        {
            PART_ScrambledCountry.Text = Country;
        }

        if (PART_ScrambledIpAddress is not null)
        {
            PART_ScrambledIpAddress.Text = IpAddress;
        }

        if (PART_ScrambledIsp is not null)
        {
            PART_ScrambledIsp.Text = Isp;
        }
    }

    public string ProtectionLabel
    {
        get => (string)GetValue(ProtectionLabelProperty);
        set => SetValue(ProtectionLabelProperty, value);
    }

    public string ProtectionSubLabel
    {
        get => (string)GetValue(ProtectionSubLabelProperty);
        set => SetValue(ProtectionSubLabelProperty, value);
    }

    public string Country
    {
        get => (string)GetValue(CountryProperty);
        set => SetValue(CountryProperty, value);
    }

    public string CountryLabel
    {
        get => (string)GetValue(CountryLabelProperty);
        set => SetValue(CountryLabelProperty, value);
    }

    public string IpAddress
    {
        get => (string)GetValue(IpAddressProperty);
        set => SetValue(IpAddressProperty, value);
    }

    public string Isp
    {
        get => (string)GetValue(IspProperty);
        set => SetValue(IspProperty, value);
    }

    public string IpAddressLabel
    {
        get => (string)GetValue(IpAddressLabelProperty);
        set => SetValue(IpAddressLabelProperty, value);
    }

    public string IspLabel
    {
        get => (string)GetValue(IspLabelProperty);
        set => SetValue(IspLabelProperty, value);
    }

    public bool IsConnected
    {
        get => (bool)GetValue(IsConnectedProperty);
        set => SetValue(IsConnectedProperty, value);
    }

    public bool IsDisconnected
    {
        get => (bool)GetValue(IsDisconnectedProperty);
        set => SetValue(IsDisconnectedProperty, value);
    }

    public bool IsConnecting
    {
        get => (bool)GetValue(IsConnectingProperty);
        set => SetValue(IsConnectingProperty, value);
    }

    private static void OnIsConnectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DeviceLocationControl dlc)
        {
            dlc.UpdateVisualStates();

            if (dlc.IsConnected)
            {
                dlc.StopScrambling();
                dlc.CompleteScrambling();
            }
        }
    }

    private static void OnIsDisconnectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DeviceLocationControl dlc)
        {
            dlc.UpdateVisualStates();

            if (dlc.IsDisconnected)
            {
                dlc.StopScrambling();
                dlc.ResetScrambling();
            }
        }
    }

    private static void OnIsConnectingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DeviceLocationControl dlc)
        {
            dlc.UpdateVisualStates();

            if (dlc.IsConnecting)
            {
                dlc.StartScrambling();
                return;
            }

            dlc.StopScrambling();
        }
    }

    private void CompleteScrambling()
    {
        PART_ScrambledCountry.Text = new string(SCRAMBLING_CHAR, Country?.Length ?? 0);
        PART_ScrambledIpAddress.Text = new string(SCRAMBLING_CHAR, IpAddress?.Length ?? 0);
    }

    private void OnScramblingTimerTick(object? sender, object e)
    {
        string deviceLocation = PART_ScrambledCountry.Text + PART_ScrambledIpAddress.Text + PART_ScrambledIsp.Text;
        if (string.IsNullOrEmpty(deviceLocation) || deviceLocation.All(c => c == SCRAMBLING_CHAR))
        {
            // Scrambling process complete
            StopScrambling();
            return;
        }
        
        int index;
        do
        {
            index = _random.Next(deviceLocation.Length);
        } while (deviceLocation.ElementAt(index) == SCRAMBLING_CHAR);
        
        deviceLocation = $"{deviceLocation.Remove(index)}{SCRAMBLING_CHAR}{deviceLocation.Remove(0, index + 1)}";
        
        PART_ScrambledCountry.Text = deviceLocation.Substring(0, Country.Length);
        PART_ScrambledIpAddress.Text = deviceLocation.Substring(Country.Length, IpAddress.Length);
        PART_ScrambledIsp.Text = deviceLocation.Substring(Isp.Length, Isp.Length);
    }

    private void ResetScrambling()
    {
        if (PART_ScrambledCountry is not null)
        {
            PART_ScrambledCountry.Text = Country;
        }

        if (PART_ScrambledIpAddress is not null)
        {
            PART_ScrambledIpAddress.Text = IpAddress;
        }

        if (PART_ScrambledIsp is not null)
        {
            PART_ScrambledIsp.Text = Isp;
        }
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

    private void UpdateVisualStates()
    {
        VisualStateManager.GoToState(this, IsConnecting ? "Connecting" : "Disconnected", true);
    }
}