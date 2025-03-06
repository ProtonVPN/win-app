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

namespace ProtonVPN.Client.Common.UI.Controls;

[TemplatePart(Name = "PART_ScrambledText", Type = typeof(TextBlock))]
public sealed partial class ScramblingTextControl
{
    private const char SCRAMBLING_CHAR = '*';
    private const int SCRAMBLING_TIMER_INTERVAL_IN_MS = 50;

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text),
        typeof(string),
        typeof(ScramblingTextControl),
        new PropertyMetadata(default, OnTextPropertyChanged));

    public static readonly DependencyProperty IsScramblingProperty = DependencyProperty.Register(
        nameof(IsScrambling),
        typeof(bool),
        typeof(ScramblingTextControl),
        new PropertyMetadata(default, OnIsScramblingPropertyChanged));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool IsScrambling
    {
        get => (bool)GetValue(IsScramblingProperty);
        set => SetValue(IsScramblingProperty, value);
    }

    private readonly Random _random = new();

    private readonly DispatcherTimer _scramblingTimer = new();

    public ScramblingTextControl()
    {
        InitializeComponent();

        _scramblingTimer.Interval = TimeSpan.FromMilliseconds(SCRAMBLING_TIMER_INTERVAL_IN_MS);
        _scramblingTimer.Tick += OnScramblingTimerTick;
    }

    private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScramblingTextControl stc)
        {
            stc.PART_ScrambledText.Text = stc.Text;
        }
    }

    private static void OnIsScramblingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScramblingTextControl stc)
        {
            if (stc.IsScrambling)
            {
                stc.StartScrambling();
                return;
            }
            stc.StopScrambling();
            stc.PART_ScrambledText.Text = stc.Text;
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

    private void OnScramblingTimerTick(object? sender, object e)
    {
        string text = PART_ScrambledText.Text;
        if (string.IsNullOrEmpty(text) || text.All(c => c == SCRAMBLING_CHAR))
        {
            StopScrambling();
            return;
        }

        int index;
        do
        {
            index = _random.Next(text.Length);
        } while (text.ElementAt(index) == SCRAMBLING_CHAR);

        text = $"{text.Remove(index)}{SCRAMBLING_CHAR}{text.Remove(0, index + 1)}";

        if (IsScrambling)
        {
            PART_ScrambledText.Text = text.Substring(0, Text.Length);
        }
    }
}